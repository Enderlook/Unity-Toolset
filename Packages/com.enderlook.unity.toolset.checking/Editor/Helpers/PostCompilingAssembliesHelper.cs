using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
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
        private const string EDITOR_CONFIGURATION_NAME = "Enderlook/Toolset/Checking/Mode";
        private const string MENU_NAME = "Enderlook/Toolset/Checking/";
        private const string MENU_NAME_REFRESH = MENU_NAME + "Refresh";
        private const string MENU_NAME_DISABLED = MENU_NAME + "Disabled";
        private const string MENU_NAME_UNITY = MENU_NAME + "Unity Compilation Pipeline";
        private const string MENU_NAME_ALL = MENU_NAME + "Entire AppDomain";
        private static int checkMode;

        private const int CHECK_DISABLED = 0;
        private const int CHECK_ENABLED_UNITY = 1;
        private const int CHECK_ENABLED_ALL = 2;

        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private readonly Container bag = new Container();

#if UNITY_2020_1_OR_NEWER
        private readonly int progressId;
#endif
        private readonly CancellationToken token;

#if UNITY_2020_1_OR_NEWER
        private int current;
#endif

        static PostCompilingAssembliesHelper()
        {
            checkMode = EditorPrefs.GetInt(MENU_NAME, 1);
            EditorApplication.delayCall += () => SetFeature(checkMode, false);
        }

        [MenuItem(MENU_NAME_ALL, priority = 0)]
        private static void CheckEnableAll() => SetFeature(CHECK_ENABLED_ALL, true);

        [MenuItem(MENU_NAME_UNITY, priority = 1)]
        private static void CheckEnableUnity() => SetFeature(CHECK_ENABLED_UNITY, true);

        [MenuItem(MENU_NAME_DISABLED, priority = 2)]
        private static void CheckDisabled() => SetFeature(CHECK_DISABLED, true);

        [MenuItem(MENU_NAME_REFRESH, priority = 3)]
        private static void Refresh() => ExecuteAnalysis();

        [MenuItem(MENU_NAME_REFRESH, true, priority = 3)]
        private static bool CanRefresh() => checkMode != CHECK_DISABLED;

        private static void SetFeature(int mode, bool execute)
        {
            int oldValue = checkMode;
            checkMode = mode;
            Menu.SetChecked(MENU_NAME_ALL, mode == CHECK_ENABLED_ALL);
            Menu.SetChecked(MENU_NAME_UNITY, mode == CHECK_ENABLED_UNITY);
            Menu.SetChecked(MENU_NAME_DISABLED, mode == CHECK_DISABLED);
            EditorPrefs.SetInt(EDITOR_CONFIGURATION_NAME, mode);
            if (execute && oldValue != mode)
                ExecuteAnalysis();
        }

#if UNITY_2020_1_OR_NEWER
        public PostCompilingAssembliesHelper(int progressId, CancellationToken token)
        {
            this.progressId = progressId;
#else
        public PostCompilingAssembliesHelper(CancellationToken token)
        {
#endif
            this.token = token;
        }

        private class Container
        {
            // TODO: Check if lazy initialization improves perfomance.

            // object is Action<Type>
            public Dictionary<int, object> executeOnEachTypeLessEnums = new Dictionary<int, object>();
            // object is Action<Type>
            public Dictionary<int, object> executeOnEachTypeEnum = new Dictionary<int, object>();
            // object is Action<MemberInfo>
            public Dictionary<int, object> executeOnEachMemberOfTypes = new Dictionary<int, object>();
            // object is Action<FieldInfo>
            public Dictionary<int, object> executeOnEachSerializableByUnityFieldOfTypes = new Dictionary<int, object>();
            // object is Action<FieldInfo>
            public Dictionary<int, object> executeOnEachNonSerializableByUnityFieldOfTypes = new Dictionary<int, object>();
            // object is Action<PropertyInfo>
            public Dictionary<int, object> executeOnEachPropertyOfTypes = new Dictionary<int, object>();
            // object is Action<MethodInfo>
            public Dictionary<int, object> executeOnEachMethodOfTypes = new Dictionary<int, object>();
            // object is Action
            public Dictionary<int, object> executeOnce = new Dictionary<int, object>();

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
            if (checkMode == CHECK_DISABLED)
                return;

            // When Unity is started the assemblies haven't loaded yet but this method is called, by adding this timer we reduce the chance of error
            // TODO: This is prone to race condition
            int millisecondsDelay = (10 - (int)EditorApplication.timeSinceStartup) * 1000;
            if (millisecondsDelay > 0)
                await Task.Delay(millisecondsDelay).ConfigureAwait(true);

            // Unsafe code must be executed in main thread.
            Assembly[] assemblies = checkMode == CHECK_ENABLED_ALL ? AppDomain.CurrentDomain.GetAssemblies() : AssembliesHelper.GetAllAssembliesOfPlayerAndEditorAssemblies().ToArray();

            BackgroundTask.Enqueue(
#if UNITY_2020_1_OR_NEWER
                _ => Progress.Start("Execute Post Compiling Checkings", "Enqueued process..."),
                (id, token) =>
#else
                token =>
#endif
                {
                    if (token.IsCancellationRequested)
                        goto cancelled;

#if UNITY_2020_1_OR_NEWER
                    Progress.SetDescription(id, "Ensure that Enderlook attributes are being used correctly.");
                    PostCompilingAssembliesHelper self = new PostCompilingAssembliesHelper(id, token);
                    int scanId = Progress.Start("Process Assemblies", parentId: id);
                    int analysisId = Progress.Start("Execute Analysis", parentId: id);
#else
                    PostCompilingAssembliesHelper self = new PostCompilingAssembliesHelper(token);
#endif

#if UNITY_2020_1_OR_NEWER
                    self.ScanAssemblies(scanId, assemblies);
#else
                    self.ScanAssemblies(assemblies);
#endif

                    if (token.IsCancellationRequested)
                    {
#if UNITY_2020_1_OR_NEWER
                        Progress.Finish(scanId, Progress.Status.Canceled);
#endif
                        goto cancelled2;
                    }

#if UNITY_2020_1_OR_NEWER
                    Progress.Finish(scanId);
                    Progress.Report(id, .5f);
#endif

#if UNITY_2020_1_OR_NEWER
                    self.ExecuteCallbacks(analysisId);
#else
                    self.ExecuteCallbacks();
#endif

                    if (token.IsCancellationRequested)
                        goto cancelled2;

#if UNITY_2020_1_OR_NEWER
                    Progress.Finish(analysisId);
                    Progress.Finish(id);
                    return;
                cancelled2:
                    Progress.Finish(analysisId, Progress.Status.Canceled);
                cancelled:;
                    Progress.Finish(id, Progress.Status.Canceled);
#else
                cancelled2:
                cancelled:;
#endif
                }
            );
        }

#if UNITY_2020_1_OR_NEWER
        private void ScanAssemblies(int id, Assembly[] assemblies)
#else
        private void ScanAssemblies(Assembly[] assemblies)
#endif
        {
#if UNITY_2020_1_OR_NEWER
            int total = 0;
            foreach (Assembly assembly in assemblies)
            {
                if (token.IsCancellationRequested)
                    goto end;

                total += assembly.GetTypes().Length;
            }
            Progress.Report(id, 0, 2);

            int currentId = Progress.Start("Scan Assemblies", parentId: id);
            Progress.Report(currentId, 0, total);
#endif

            // Sharing collections and using Interlocked.Exchange as synchronization per collection decreases perfomances by 23%.
            // Instead, allocating a collection per assembly and merge them later increases perfomance by 40%.
            Container[] containers = new Container[assemblies.Length];

#if UNITY_2020_1_OR_NEWER
            current = 0;
#endif
            Parallel.For(0, assemblies.Length, i =>
            {
                if (token.IsCancellationRequested)
                    return;

                Container container = new Container();
                containers[i] = container;

                Assembly assembly = assemblies[i];

                if (assembly.IsDefined(typeof(DoNotInspectAttribute)))
                {
#if UNITY_2020_1_OR_NEWER
                    Progress.Report(currentId, Interlocked.Add(ref current, assembly.GetTypes().Length), total);
#endif
                    return;
                }

                foreach (Type classType in assembly.GetTypes())
                {
                    if (token.IsCancellationRequested)
                        return;

#if UNITY_2020_1_OR_NEWER
                    Progress.Report(currentId, Interlocked.Increment(ref current), total);
#endif

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

#if UNITY_2020_1_OR_NEWER
            Progress.Finish(currentId, token.IsCancellationRequested ? Progress.Status.Canceled : Progress.Status.Succeeded);
            Progress.Report(id, 1, 2);
#endif

            if (token.IsCancellationRequested)
                goto end;

#if UNITY_2020_1_OR_NEWER
            currentId = Progress.Start("Merge Scans", parentId: id);
            Progress.Report(currentId, 0, containers.Length);
#endif

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

            // NOTE: Each nested loop could be executed in parallel from other nested loops.
            for (int i = 0; i < containers.Length; i++)
            {
                if (token.IsCancellationRequested)
                    goto end;

                Container c = containers[i];

                foreach (KeyValuePair<int, object> kvp in c.executeOnEachTypeLessEnums)
                    SubscribeConcurrent<Action<Type>>(bag.executeOnEachTypeLessEnums, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, object> kvp in c.executeOnEachTypeEnum)
                    SubscribeConcurrent<Action<Type>>(bag.executeOnEachTypeEnum, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, object> kvp in c.executeOnEachMemberOfTypes)
                    SubscribeConcurrent<Action<MemberInfo>>(bag.executeOnEachMemberOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, object> kvp in c.executeOnEachSerializableByUnityFieldOfTypes)
                    SubscribeConcurrent<Action<FieldInfo>>(bag.executeOnEachSerializableByUnityFieldOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, object> kvp in c.executeOnEachNonSerializableByUnityFieldOfTypes)
                    SubscribeConcurrent<Action<FieldInfo>>(bag.executeOnEachNonSerializableByUnityFieldOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, object> kvp in c.executeOnEachPropertyOfTypes)
                    SubscribeConcurrent<Action<PropertyInfo>>(bag.executeOnEachPropertyOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, object> kvp in c.executeOnEachMethodOfTypes)
                    SubscribeConcurrent<Action<MethodInfo>>(bag.executeOnEachMethodOfTypes, kvp.Value, kvp.Key);
                foreach (KeyValuePair<int, object> kvp in c.executeOnce)
                    SubscribeConcurrent<Action>(bag.executeOnce, kvp.Value, kvp.Key);

                bag.enumTypes.AddRange(c.enumTypes);
                bag.nonEnumTypes.AddRange(c.nonEnumTypes);
                bag.memberInfos.AddRange(c.memberInfos);
                bag.fieldInfosNonSerializableByUnity.AddRange(c.fieldInfosNonSerializableByUnity);
                bag.fieldInfosSerializableByUnity.AddRange(c.fieldInfosSerializableByUnity);
                bag.propertyInfos.AddRange(c.propertyInfos);
                bag.methodInfos.AddRange(c.methodInfos);

#if UNITY_2020_1_OR_NEWER
                Progress.Report(currentId, i, containers.Length);
#endif
            }

            Dictionary<int, object> dict = new Dictionary<int, object>(bag.executeOnEachTypeLessEnums.Count);
            Replace<Action<Type>>(ref dict, ref bag.executeOnEachTypeLessEnums);
            Replace<Action<Type>>(ref dict, ref bag.executeOnEachTypeEnum);
            Replace<Action<MemberInfo>>(ref dict, ref bag.executeOnEachMemberOfTypes);
            Replace<Action<FieldInfo>>(ref dict, ref bag.executeOnEachSerializableByUnityFieldOfTypes);
            Replace<Action<FieldInfo>>(ref dict, ref bag.executeOnEachNonSerializableByUnityFieldOfTypes);
            Replace<Action<PropertyInfo>>(ref dict, ref bag.executeOnEachPropertyOfTypes);
            Replace<Action<MethodInfo>>(ref dict, ref bag.executeOnEachMethodOfTypes);
            Replace<Action>(ref dict, ref bag.executeOnce);

            void Replace<T>(ref Dictionary<int, object> dict, ref Dictionary<int, object> source)
                where T : Delegate
            {
                // TODO: On .Net Standard 2.1 we can use .EnsureCapacity() to avoid reallocations.
                foreach (KeyValuePair<int, object> kvp in source)
                {
                    Debug.Assert(kvp.Value is List<T>);
                    dict[kvp.Key] = Delegate.Combine(Unsafe.As<List<T>>(kvp.Value).ToArray());
                }
                (dict, source) = (source, dict);
                dict.Clear();
            }

#if UNITY_2020_1_OR_NEWER
            Progress.Finish(currentId, token.IsCancellationRequested ? Progress.Status.Canceled : Progress.Status.Succeeded);
            Progress.Report(id, 2, 2);
#endif

        end:;
#if UNITY_2020_1_OR_NEWER
            current = 0;
#endif
        }

        private void GetExecuteAttributes(Container container, MethodInfo methodInfo)
        {
            if (!methodInfo.IsStatic)
                return;

            foreach (BaseExecuteWhenCheckAttribute attribute in methodInfo.GetCustomAttributes<BaseExecuteWhenCheckAttribute>())
            {
                int order = attribute.order;
                if (attribute is ExecuteOnEachTypeWhenCheckAttribute executeOnEachTypeWhenScriptsReloads)
                {
                    TypeFlags typeFlags = executeOnEachTypeWhenScriptsReloads.typeFilter;

                    if (TryGetDelegate(methodInfo, out Action<Type> action))
                    {
                        if ((typeFlags & TypeFlags.IsEnum) != 0)
                            SubscribeConcurrent(container.executeOnEachTypeEnum, action, order);
                        if ((typeFlags & TypeFlags.IsNonEnum) != 0)
                            SubscribeConcurrent(container.executeOnEachTypeLessEnums, action, order);
                    }
                }
                else if (attribute is ExecuteOnEachMemberOfEachTypeWhenCheckAttribute executeOnEachMemberOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<MemberInfo> action))
                        SubscribeConcurrent(container.executeOnEachMemberOfTypes, action, order);
                }
                else if (attribute is ExecuteOnEachFieldOfEachTypeWhenCheckAttribute executeOnEachFieldOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<FieldInfo> action))
                    {
                        FieldSerialization fieldFags = executeOnEachFieldOfEachTypeWhenScriptsReloads.fieldFilter;
                        if ((fieldFags & FieldSerialization.SerializableByUnity) != 0)
                            SubscribeConcurrent(container.executeOnEachSerializableByUnityFieldOfTypes, action, order);
                        if ((fieldFags & FieldSerialization.NotSerializableByUnity) != 0)
                            SubscribeConcurrent(container.executeOnEachNonSerializableByUnityFieldOfTypes, action, order);
                    }
                }
                else if (attribute is ExecuteOnEachPropertyOfEachTypeWhenCheckAttribute executeOnEachPropertyOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<PropertyInfo> action))
                        SubscribeConcurrent(container.executeOnEachPropertyOfTypes, action, order);
                }
                else if (attribute is ExecuteOnEachMethodOfEachTypeWhenCheckAttribute executeOnEachMethodOfEachTypeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action<MethodInfo> action))
                        SubscribeConcurrent(container.executeOnEachMethodOfTypes, action, order);
                }
                else if (attribute is ExecuteWhenCheckAttribute executeWhenScriptsReloads)
                {
                    if (TryGetDelegate(methodInfo, out Action action))
                        SubscribeConcurrent(container.executeOnce, action, order);
                }
            }
        }

        private static readonly string ATTRIBUTE_METHOD_ERROR = $"Method {{0}} in {{1}} does not follow the requirements of attribute {nameof(ExecuteOnEachTypeWhenCheckAttribute)}. It's signature must be {{2}}.";

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

#if UNITY_2020_1_OR_NEWER
        private void ExecuteCallbacks(int id)
#else
        private void ExecuteCallbacks()
#endif
        {
#if UNITY_2020_1_OR_NEWER
            int total = bag.executeOnce.Count;
#endif
            HashSet<int> keys = new HashSet<int>();

            keys.UnionWith(bag.executeOnEachTypeEnum.Keys);
#if UNITY_2020_1_OR_NEWER
            total += bag.executeOnEachTypeEnum.Count * bag.enumTypes.Count;
#endif
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachTypeLessEnums.Keys);
#if UNITY_2020_1_OR_NEWER
            total += bag.executeOnEachTypeLessEnums.Count * bag.nonEnumTypes.Count;
#endif
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachMemberOfTypes.Keys);
#if UNITY_2020_1_OR_NEWER
            total += bag.executeOnEachMemberOfTypes.Count * bag.memberInfos.Count;
#endif
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachSerializableByUnityFieldOfTypes.Keys);
#if UNITY_2020_1_OR_NEWER
            total += bag.executeOnEachSerializableByUnityFieldOfTypes.Count * bag.fieldInfosSerializableByUnity.Count;
#endif
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachNonSerializableByUnityFieldOfTypes.Keys);
#if UNITY_2020_1_OR_NEWER
            total += bag.executeOnEachNonSerializableByUnityFieldOfTypes.Count * bag.fieldInfosNonSerializableByUnity.Count;
#endif
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachPropertyOfTypes.Keys);
#if UNITY_2020_1_OR_NEWER
            total += bag.executeOnEachPropertyOfTypes.Count * bag.propertyInfos.Count;
#endif
            if (token.IsCancellationRequested)
                goto end;

            keys.UnionWith(bag.executeOnEachMethodOfTypes.Keys);
#if UNITY_2020_1_OR_NEWER
            total += bag.executeOnEachMethodOfTypes.Count * bag.methodInfos.Count;
#endif
            if (token.IsCancellationRequested)
                goto end;

            List<int> orderedKeys = keys.ToList();
            if (token.IsCancellationRequested)
                goto end;

            orderedKeys.Sort();
            if (token.IsCancellationRequested)
                goto end;

#if UNITY_2020_1_OR_NEWER
            Progress.Report(id, 0, total);
            current = 0;
#endif
            int order = 0;
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
                    if (bag.executeOnce.TryGetValue(order, out object action))
                    {
                        Debug.Assert(action is Action);
                        Unsafe.As<Action>(action)();
#if UNITY_2020_1_OR_NEWER
                        Progress.Report(id, Interlocked.Increment(ref current), total);
#endif
                    }
                }
            };

            foreach (int loop in orderedKeys)
            {
                if (token.IsCancellationRequested)
                    goto end;

                order = loop;

                Parallel.Invoke(actions);
            }

        end:
            order = 0;
#if UNITY_2020_1_OR_NEWER
            Progress.Finish(id, token.IsCancellationRequested ? Progress.Status.Canceled : Progress.Status.Succeeded);
            current = 0;
#endif

            void ExecuteLoop<T>(Dictionary<int, object> callbacks, List<T> values)
            {
                if (callbacks.TryGetValue(order, out object action))
                {
                    if (token.IsCancellationRequested)
                        return;

                    Debug.Assert(action is Action<T>);
                    foreach (T element in values)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        Unsafe.As<Action<T>>(action)(element);

#if UNITY_2020_1_OR_NEWER
                        Progress.Report(id, Interlocked.Increment(ref current), total);
#endif
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SubscribeConcurrent<T>(Dictionary<int, object> dictionary, T action, int order)
            where T : Delegate
        {
            if (dictionary.TryGetValue(order, out object list))
            {
                Debug.Assert(list is List<T>);
                Unsafe.As<List<T>>(list).Add(action);
            }
            else
                dictionary.Add(order, new List<T>() { action });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SubscribeConcurrent<T>(Dictionary<int, object> dictionary, object action, int order)
            where T : Delegate
        {
            Debug.Assert(action is List<T>);
            if (dictionary.TryGetValue(order, out object list))
            {
                Debug.Assert(list is List<T>);
                Unsafe.As<List<T>>(list).AddRange(Unsafe.As<List<T>>(action));
            }
            else
                dictionary.Add(order, action);
        }
    }
}