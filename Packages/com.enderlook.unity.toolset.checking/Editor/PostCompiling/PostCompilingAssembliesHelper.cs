﻿using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling
{
    internal class PostCompilingAssembliesHelper
    {
#pragma warning disable CS0649
        private const string MENU_NAME = "Enderlook/Toolset/Check All Assemblies";
        private static bool checkAllAssemblies;

        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private readonly Container bag = new Container();

        private readonly int progressId;
        private readonly CancellationToken token;

        private int current;

        static PostCompilingAssembliesHelper()
        {
            checkAllAssemblies = EditorPrefs.GetBool(MENU_NAME, true);
            EditorApplication.delayCall += () => SetFeature(checkAllAssemblies);
        }

        [MenuItem(MENU_NAME)]
        private static void ToggleFeatureButton() => SetFeature(!checkAllAssemblies);

        private static void SetFeature(bool enabled)
        {
            checkAllAssemblies = enabled;
            Menu.SetChecked(MENU_NAME, enabled);
            EditorPrefs.SetBool(MENU_NAME, enabled);
        }

        public PostCompilingAssembliesHelper(int progressId, CancellationToken token)
        {
            this.progressId = progressId;
            this.token = token;
        }

        private class Container
        {
            // TODO: Check if lazy initialization improves perfomance.

            public readonly Dictionary<int, Action<Type>> executeOnEachTypeLessEnums = new Dictionary<int, Action<Type>>();
            public readonly Dictionary<int, Action<Type>> executeOnEachTypeEnum = new Dictionary<int, Action<Type>>();
            public readonly Dictionary<int, Action<MemberInfo>> executeOnEachMemberOfTypes = new Dictionary<int, Action<MemberInfo>>();
            public readonly Dictionary<int, Action<FieldInfo>> executeOnEachSerializableByUnityFieldOfTypes = new Dictionary<int, Action<FieldInfo>>();
            public readonly Dictionary<int, Action<FieldInfo>> executeOnEachNonSerializableByUnityFieldOfTypes = new Dictionary<int, Action<FieldInfo>>();
            public readonly Dictionary<int, Action<PropertyInfo>> executeOnEachPropertyOfTypes = new Dictionary<int, Action<PropertyInfo>>();
            public readonly Dictionary<int, Action<MethodInfo>> executeOnEachMethodOfTypes = new Dictionary<int, Action<MethodInfo>>();
            public readonly Dictionary<int, Action> executeOnce = new Dictionary<int, Action>();

            public readonly List<Type> enumTypes = new List<Type>();
            public readonly List<Type> nonEnumTypes = new List<Type>();
            public readonly List<MemberInfo> memberInfos = new List<MemberInfo>();
            public readonly List<FieldInfo> fieldInfosNonSerializableByUnity = new List<FieldInfo>();
            public readonly List<FieldInfo> fieldInfosSerializableByUnity = new List<FieldInfo>();
            public readonly List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            public readonly List<MethodInfo> methodInfos = new List<MethodInfo>();
        }

        [DidReloadScripts(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity Editor")]
        private static async void ExecuteAnalysis()
        {
            // When Unity is started the assemblies haven't loaded yet but this method is called, by adding this timer we reduce the chance of error
            // TODO: This is prone to race condition
            int millisecondsDelay = (10 - (int)EditorApplication.timeSinceStartup) * 1000;
            if (millisecondsDelay > 0)
                await Task.Delay(millisecondsDelay).ConfigureAwait(true);

            // Unsafe code muset be executed in main thread.
            Assembly[] assemblies = checkAllAssemblies ? AppDomain.CurrentDomain.GetAssemblies() : AssembliesHelper.GetAllAssembliesOfPlayerAndEditorAssemblies().ToArray();

            BackgroundTask.Enqueue(
                token => Progress.Start("Execute Post Compiling Checkings", "Ensure that Enderlook attributes are being used correctly."),
                (id, token) =>
                {
                    if (token.IsCancellationRequested)
                        goto cancelled;

                    PostCompilingAssembliesHelper self = new PostCompilingAssembliesHelper(id, token);

                    int scanId = Progress.Start("Scan Assemblies", parentId: id);
                    int analysisId = Progress.Start("Execute Analysis", parentId: id);

                    self.ScanAssemblies(scanId, assemblies);

                    if (token.IsCancellationRequested)
                    {
                        Progress.Finish(scanId, Progress.Status.Canceled);
                        goto cancelled2;
                    }

                    Progress.Finish(scanId);
                    Progress.Report(id, .5f);

                    self.ExecuteCallbacks(analysisId);

                    if (token.IsCancellationRequested)
                        goto cancelled2;

                    Progress.Finish(analysisId);
                    Progress.Finish(id);
                    return;
                cancelled2:
                    Progress.Finish(analysisId, Progress.Status.Canceled);
                cancelled:
                    Progress.Finish(id, Progress.Status.Canceled);
                }
            );
        }

        private void ScanAssemblies(int id, Assembly[] assemblies)
        {
            int total = 0;
            foreach (Assembly assembly in assemblies)
            {
                if (token.IsCancellationRequested)
                    goto end;

                total += assembly.GetTypes().Length;
            }
            Progress.Report(id, 0, total);

            // Sharing collections and using Interlocked.Exchange as synchronization per collection decreases perfomances by 23%.
            // Instead, allocating a collection per assembly and merge them later increases perfomance by 40%.
            Container[] containers = new Container[assemblies.Length];

            current = 0;
            Parallel.For(0, assemblies.Length, i =>
            {
                if (token.IsCancellationRequested)
                    return;

                Container container = new Container();
                containers[i] = container;

                Assembly assembly = assemblies[i];

                if (assembly.IsDefined(typeof(DoNotInspectAttribute)))
                {
                    Progress.Report(id, Interlocked.Add(ref current, assembly.GetTypes().Length), total);
                    return;
                }

                foreach (Type classType in assembly.GetTypes())
                {
                    if (token.IsCancellationRequested)
                        return;

                    Progress.Report(id, Interlocked.Increment(ref current), total);

                    if (classType.IsDefined(typeof(DoNotInspectAttribute)))
                        continue;

                    if (classType.IsEnum)
                        container.enumTypes.Add(classType);
                    else
                    {
                        container.nonEnumTypes.Add(classType);

                        foreach (MemberInfo memberInfo in classType.GetMembers(bindingFlags))
                        {
                            try
                            {
                                if (memberInfo.IsDefined(typeof(DoNotInspectAttribute)))
                                    continue;
                            }
                            catch (BadImageFormatException)
                            {
                                continue;
                            }

                            container.memberInfos.Add(memberInfo);

                            switch (memberInfo.MemberType)
                            {
                                case MemberTypes.Field:
                                    FieldInfo fieldInfo = (FieldInfo)memberInfo;
                                    if (fieldInfo.CanBeSerializedByUnity())
                                        container.fieldInfosSerializableByUnity.Add((FieldInfo)memberInfo);
                                    else
                                        container.fieldInfosNonSerializableByUnity.Add((FieldInfo)memberInfo);
                                    break;
                                case MemberTypes.Property:
                                    container.propertyInfos.Add((PropertyInfo)memberInfo);
                                    break;
                                case MemberTypes.Method:
                                    MethodInfo methodInfo = (MethodInfo)memberInfo;
                                    container.methodInfos.Add(methodInfo);
                                    GetExecuteAttributes(container, methodInfo);
                                    break;
                            }
                        }
                    }
                }
            });

            if (token.IsCancellationRequested)
                goto end;

            int enumTypesCount = 0;
            int nonEnumTypesCount = 0;
            int memberInfosCount = 0;
            int fieldInfosNonSerializableByUnityCount = 0;
            int fieldInfosSerializableByUnityCount = 0;
            int propertyInfosCount = 0;
            int methodInfosCount = 0;
            for (int i = 0; i < containers.Length; i++)
            {
                if (token.IsCancellationRequested)
                    goto end;

                Container c = containers[i];

                enumTypesCount += c.enumTypes.Count;
                nonEnumTypesCount += c.nonEnumTypes.Count;
                memberInfosCount += c.memberInfos.Count;
                fieldInfosNonSerializableByUnityCount += c.fieldInfosNonSerializableByUnity.Count;
                fieldInfosSerializableByUnityCount += c.fieldInfosSerializableByUnity.Count;
                propertyInfosCount += c.propertyInfos.Count;
                methodInfosCount += c.methodInfos.Count;
            }

            bag.enumTypes.Capacity = enumTypesCount;
            bag.nonEnumTypes.Capacity = nonEnumTypesCount;
            bag.memberInfos.Capacity = memberInfosCount;
            bag.fieldInfosNonSerializableByUnity.Capacity = fieldInfosNonSerializableByUnityCount;
            bag.fieldInfosSerializableByUnity.Capacity = fieldInfosSerializableByUnityCount;
            bag.propertyInfos.Capacity = propertyInfosCount;
            bag.methodInfos.Capacity = methodInfosCount;

            for (int i = 0; i < containers.Length; i++)
            {
                if (token.IsCancellationRequested)
                    goto end;

                Container c = containers[i];

                foreach (KeyValuePair<int, Action<Type>> kvp in c.executeOnEachTypeLessEnums)
                    SubscribeConcurrent(bag.executeOnEachTypeLessEnums, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, Action<Type>> kvp in c.executeOnEachTypeEnum)
                    SubscribeConcurrent(bag.executeOnEachTypeEnum, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, Action<MemberInfo>> kvp in c.executeOnEachMemberOfTypes)
                    SubscribeConcurrent(bag.executeOnEachMemberOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, Action<FieldInfo>> kvp in c.executeOnEachSerializableByUnityFieldOfTypes)
                    SubscribeConcurrent(bag.executeOnEachSerializableByUnityFieldOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, Action<FieldInfo>> kvp in c.executeOnEachNonSerializableByUnityFieldOfTypes)
                    SubscribeConcurrent(bag.executeOnEachNonSerializableByUnityFieldOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, Action<PropertyInfo>> kvp in c.executeOnEachPropertyOfTypes)
                    SubscribeConcurrent(bag.executeOnEachPropertyOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, Action<MethodInfo>> kvp in c.executeOnEachMethodOfTypes)
                    SubscribeConcurrent(bag.executeOnEachMethodOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, Action> kvp in c.executeOnce)
                    SubscribeConcurrent(bag.executeOnce, kvp.Value, kvp.Key);

                bag.enumTypes.AddRange(c.enumTypes);
                bag.nonEnumTypes.AddRange(c.nonEnumTypes);
                bag.memberInfos.AddRange(c.memberInfos);
                bag.fieldInfosNonSerializableByUnity.AddRange(c.fieldInfosNonSerializableByUnity);
                bag.fieldInfosSerializableByUnity.AddRange(c.fieldInfosSerializableByUnity);
                bag.propertyInfos.AddRange(c.propertyInfos);
                bag.methodInfos.AddRange(c.methodInfos);
            }

        end:
            current = 0;
        }

        private void GetExecuteAttributes(Container container, MethodInfo methodInfo)
        {
            if (!methodInfo.IsStatic)
                return;

            foreach (BaseExecuteWhenScriptsReloads attribute in methodInfo.GetCustomAttributes<BaseExecuteWhenScriptsReloads>())
            {
                int loop = attribute.loop;
                if (attribute is ExecuteOnEachTypeWhenScriptsReloads executeOnEachTypeWhenScriptsReloads)
                {
                    ExecuteOnEachTypeWhenScriptsReloads.TypeFlags typeFlags = executeOnEachTypeWhenScriptsReloads.typeFilter;

                    if (TryGetDelegate(methodInfo, out Action<Type> action))
                    {
                        if ((typeFlags & ExecuteOnEachTypeWhenScriptsReloads.TypeFlags.IsEnum) != 0)
                            SubscribeConcurrent(container.executeOnEachTypeEnum, action, loop);
                        if ((typeFlags & ExecuteOnEachTypeWhenScriptsReloads.TypeFlags.IsNonEnum) != 0)
                            SubscribeConcurrent(container.executeOnEachTypeLessEnums, action, loop);
                    }
                }
                else if (attribute is ExecuteOnEachMemberOfEachTypeWhenScriptsReloads executeOnEachMemberOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<MemberInfo> action))
                        SubscribeConcurrent(container.executeOnEachMemberOfTypes, action, loop);
                }
                else if (attribute is ExecuteOnEachFieldOfEachTypeWhenScriptsReloads executeOnEachFieldOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<FieldInfo> action))
                    {
                        FieldSerialization fieldFags = executeOnEachFieldOfEachTypeWhenScriptsReloads.fieldFilter;
                        if ((fieldFags & FieldSerialization.SerializableByUnity) != 0)
                            SubscribeConcurrent(container.executeOnEachSerializableByUnityFieldOfTypes, action, loop);
                        if ((fieldFags & FieldSerialization.NotSerializableByUnity) != 0)
                            SubscribeConcurrent(container.executeOnEachNonSerializableByUnityFieldOfTypes, action, loop);
                    }
                }
                else if (attribute is ExecuteOnEachPropertyOfEachTypeWhenScriptsReloads executeOnEachPropertyOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<PropertyInfo> action))
                        SubscribeConcurrent(container.executeOnEachPropertyOfTypes, action, loop);
                }
                else if (attribute is ExecuteOnEachMethodOfEachTypeWhenScriptsReloads executeOnEachMethodOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<MethodInfo> action))
                        SubscribeConcurrent(container.executeOnEachMethodOfTypes, action, loop);
                }
                else if (attribute is ExecuteWhenScriptsReloads executeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action action))
                        SubscribeConcurrent(container.executeOnce, action, loop);
                }
            }
        }

        private static readonly string ATTRIBUTE_METHOD_ERROR = $"Method {{0}} in {{1}} does not follow the requirements of attribute {nameof(ExecuteOnEachTypeWhenScriptsReloads)}. It's signature must be {{2}}.";

        private static bool TryGetDelegate<T>(MethodInfo methodInfo, out Action<T> action)
        {
            action = (Action<T>)TryGetDelegate<Action<T>>(methodInfo);
            return action != null;
        }

        private static bool TryGetDelegate(MethodInfo methodInfo, out Action action)
        {
            action = (Action)TryGetDelegate<Action>(methodInfo);
            return action != null;
        }

        private static Delegate TryGetDelegate<T>(MethodInfo methodInfo)
        {
            try
            {
                /* At first sight `e => methodInfo.Invoke(null, Array.Empty<object>());` might seem faster
                   But actually, CreateDelegate does amortize if called through MulticastDelegate multiple times, like we are going to do.*/
                return methodInfo.CreateDelegate(typeof(T));
            }
            catch (ArgumentException e)
            {
                Error();

                void Error()
                {
                    Type[] genericArguments = typeof(T).GetGenericArguments();
                    string signature = genericArguments.Length == 0 ? "nothing" : string.Join(", ", genericArguments.Select(a => a.Name));
                    Debug.LogException(new ArgumentException(string.Format(ATTRIBUTE_METHOD_ERROR, methodInfo.Name, methodInfo.DeclaringType, signature), e));
                }
            }
            return null;
        }

        private void ExecuteCallbacks(int id)
        {
            int total = bag.executeOnce.Count;
            HashSet<int> keys = new HashSet<int>();

            keys.UnionWith(bag.executeOnEachTypeEnum.Keys);
            total += bag.executeOnEachTypeEnum.Count * bag.enumTypes.Count;
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachTypeLessEnums.Keys);
            total += bag.executeOnEachTypeLessEnums.Count * bag.nonEnumTypes.Count;
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachMemberOfTypes.Keys);
            total += bag.executeOnEachMemberOfTypes.Count * bag.memberInfos.Count;
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachSerializableByUnityFieldOfTypes.Keys);
            total += bag.executeOnEachSerializableByUnityFieldOfTypes.Count * bag.fieldInfosSerializableByUnity.Count;
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachNonSerializableByUnityFieldOfTypes.Keys);
            total += bag.executeOnEachNonSerializableByUnityFieldOfTypes.Count * bag.fieldInfosNonSerializableByUnity.Count;
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachPropertyOfTypes.Keys);
            total += bag.executeOnEachPropertyOfTypes.Count * bag.propertyInfos.Count;
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachMethodOfTypes.Keys);
            total += bag.executeOnEachMethodOfTypes.Count * bag.methodInfos.Count;
            if (token.IsCancellationRequested)
                goto end;

            List<int> orderedKeys = keys.ToList();
            if (token.IsCancellationRequested)
                goto end;

            orderedKeys.Sort();
            if (token.IsCancellationRequested)
                goto end;

            Progress.Report(id, 0, total);

            int loop = 0;
            Action[] actions = new Action[]
            {
                () => ExecuteLoop(bag.executeOnEachTypeEnum, bag.enumTypes),
                () => ExecuteLoop(bag.executeOnEachTypeLessEnums, bag.nonEnumTypes),
                () => ExecuteLoop(bag.executeOnEachMemberOfTypes, bag.memberInfos),
                () => ExecuteLoop(bag.executeOnEachSerializableByUnityFieldOfTypes, bag.fieldInfosSerializableByUnity),
                () => ExecuteLoop(bag.executeOnEachNonSerializableByUnityFieldOfTypes, bag.fieldInfosNonSerializableByUnity),
                () => ExecuteLoop(bag.executeOnEachPropertyOfTypes, bag.propertyInfos),
                () => ExecuteLoop(bag.executeOnEachMethodOfTypes, bag.methodInfos),
                () =>
                {
                    if (bag.executeOnce.TryGetValue(loop, out Action action))
                    {
                        action();
                        Progress.Report(id, Interlocked.Increment(ref current), total);
                    }
                }
            };

            current = 0;
            foreach (int loop_ in orderedKeys)
            {
                if (token.IsCancellationRequested)
                    goto end;

                loop = loop_;

                Parallel.Invoke(actions);
            }

        end:
            loop = 0;
            current = 0;

            void ExecuteLoop<T>(Dictionary<int, Action<T>> callbacks, List<T> values)
            {
                if (callbacks.TryGetValue(loop, out Action<T> action))
                {
                    if (token.IsCancellationRequested)
                        return;

                    foreach (T element in values)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        action(element);

                        Progress.Report(id, Interlocked.Increment(ref current), total);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SubscribeConcurrent<T>(Dictionary<int, T> dictionary, T action, int order)
            where T : Delegate
        {
            if (dictionary.TryGetValue(order, out T value))
                dictionary[order] = (T)Delegate.Combine(value, action);
            else
                dictionary.Add(order, action);
        }
    }
}