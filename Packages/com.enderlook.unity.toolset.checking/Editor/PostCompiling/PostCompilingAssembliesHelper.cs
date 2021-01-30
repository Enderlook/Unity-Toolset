using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling
{
    public static class PostCompilingAssembliesHelper
    {
        public static bool onlyCheckAssembliesFromPlayerAndEditor = true;
        public static bool OnlyCheckAssembliesFromPlayerAndEditor {
            get => onlyCheckAssembliesFromPlayerAndEditor;
            set {
                onlyCheckAssembliesFromPlayerAndEditor = value;
                ExecuteAnalysis();
            }
        }

#pragma warning disable CS0649
        // Type Less Enum
        private static readonly Dictionary<int, Action<Type>> executeOnEachTypeLessEnums = new Dictionary<int, Action<Type>>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed on each <see cref="Type"/> in the assemblies compiled by Unity which <see cref="Type.IsEnum"/> is <see langword="false"/>.<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteOnEachTypeWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeOnEachTypeLessEnums(Action<Type> action, int order) => SubscribeCallback(executeOnEachTypeLessEnums, action, order);

        // Type Enum
        private static readonly Dictionary<int, Action<Type>> executeOnEachTypeEnum = new Dictionary<int, Action<Type>>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed on each <see cref="Type"/> in the assemblies compiled by Unity which <see cref="Type.IsEnum"/> is <see langword="true"/>.<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteOnEachTypeWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeOnEachTypeEnum(Action<Type> action, int order) => SubscribeCallback(executeOnEachTypeEnum, action, order);

        // Member
        private static readonly Dictionary<int, Action<MemberInfo>> executeOnEachMemberOfTypes = new Dictionary<int, Action<MemberInfo>>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed on each member of each <see cref="Type"/> in the assemblies compiled by Unity.<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteOnEachTypeWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeOnEachMemberOfTypes(Action<MemberInfo> action, int order) => SubscribeCallback(executeOnEachMemberOfTypes, action, order);

        // Serializable By Unity Field
        private static readonly Dictionary<int, Action<FieldInfo>> executeOnEachSerializableByUnityFieldOfTypes = new Dictionary<int, Action<FieldInfo>>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed on each field of each <see cref="Type"/> in the assemblies compiled by Unity which can be serialized by Unity (<seealso cref="ReflectionHelper.CanBeSerializedByUnity(FieldInfo)"/>).<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteOnEachTypeWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeOnEachSerializableByUnityFieldOfTypes(Action<FieldInfo> action, int order) => SubscribeCallback(executeOnEachSerializableByUnityFieldOfTypes, action, order);

        // Non Serializable By Unity Field
        private static readonly Dictionary<int, Action<FieldInfo>> executeOnEachNonSerializableByUnityFieldOfTypes = new Dictionary<int, Action<FieldInfo>>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed on each member of each <see cref="Type"/> in the assemblies compiled by Unity which can be serialized by Unity (<seealso cref="ReflectionHelper.CanBeSerializedByUnity(FieldInfo)"/>).<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteOnEachTypeWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeOnEachNonSerializableByUnityFieldOfTypes(Action<FieldInfo> action, int order) => SubscribeCallback(executeOnEachNonSerializableByUnityFieldOfTypes, action, order);

        // Property
        private static readonly Dictionary<int, Action<PropertyInfo>> executeOnEachPropertyOfTypes = new Dictionary<int, Action<PropertyInfo>>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed on each property of each <see cref="Type"/> in the assemblies compiled by Unity.<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteOnEachTypeWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeOnEachPropertyOfTypes(Action<PropertyInfo> action, int order) => SubscribeCallback(executeOnEachPropertyOfTypes, action, order);

        // Method
        private static readonly Dictionary<int, Action<MethodInfo>> executeOnEachMethodOfTypes = new Dictionary<int, Action<MethodInfo>>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed on each method of each <see cref="Type"/> in the assemblies compiled by Unity.<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteOnEachTypeWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeOnEachMethodOfTypes(Action<MethodInfo> action, int order) => SubscribeCallback(executeOnEachMethodOfTypes, action, order);

        // Once
        private static readonly Dictionary<int, Action> executeOnce = new Dictionary<int, Action>();

        /// <summary>
        /// Subscribes <paramref name="action"/> to be executed once wen Unity ompiles assemblies.<br/>
        /// If possible, it's strongly advisable to use <see cref="ExecuteWhenScriptsReloads"/> attribute instead of this method.
        /// </summary>
        /// <param name="action">Action to subscribe.</param>
        /// <param name="order">Priority of this method to execute. After all other callbacks of lower order are executed on all targets this will be executed.</param>
        public static void SubscribeToExecuteOnce(Action action, int order) => SubscribeCallback(executeOnce, action, order);

        private static void SubscribeCallback<T>(Dictionary<int, Action<T>> dictionary, Action<T> action, int order)
        {
            if (dictionary.ContainsKey(order))
                dictionary[order] += action;
            else
                dictionary.Add(order, action);
        }

        private static void SubscribeCallback(Dictionary<int, Action> dictionary, Action action, int order)
        {
            if (dictionary.ContainsKey(order))
                dictionary[order] += action;
            else
                dictionary.Add(order, action);
        }

        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static readonly List<Type> enumTypes = new List<Type>();
        private static readonly List<Type> nonEnumTypes = new List<Type>();
        private static readonly List<MemberInfo> memberInfos = new List<MemberInfo>();
        private static readonly List<FieldInfo> fieldInfosNonSerializableByUnity = new List<FieldInfo>();
        private static readonly List<FieldInfo> fieldInfosSerializableByUnity = new List<FieldInfo>();
        private static readonly List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
        private static readonly List<MethodInfo> methodInfos = new List<MethodInfo>();
        
        [DidReloadScripts(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity Editor")]
        private static async void ExecuteAnalysis()
        {
            // When Unity is started the assemblies haven't loaded yet but this method is called, by adding this timer we reduce the chance of error
            // TODO: This is prone to race condition
            int millisecondsDelay = (10 - (int)EditorApplication.timeSinceStartup) * 1000;
            if (millisecondsDelay > 0)
                await Task.Delay(millisecondsDelay).ConfigureAwait(true);

            // Can't do unsafe work in non-main thread. And this is unsafe
            IEnumerable<Type> types = GetAllTypesThatShouldBeInspected();

            await Task.Run(() =>
            {
                ScanAssemblies(types);
                ExecuteCallbacks();
            });
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            if (onlyCheckAssembliesFromPlayerAndEditor)
                return AssembliesHelper.GetAllAssembliesOfPlayerAndEditorAssemblies();
            else
                return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Get all types from assemblies which doesn't have <see cref="DoNotInspectAttribute"/> either the type or the assembly.
        /// </summary>
        /// <returns>All types of Player and Editor assemblies, which matches criteria..</returns>
        private static IEnumerable<Type> GetAllTypesThatShouldBeInspected() =>
            GetAssemblies()
                .Where(e => !e.IsDefined(typeof(DoNotInspectAttribute)))
                .SelectMany(e => e.GetTypes()).Where(e => !e.IsDefined(typeof(DoNotInspectAttribute)));

        private static void ScanAssemblies(IEnumerable<Type> types)
        {
            // We don't filter by DoNotInspectAttribute because we did that before
            foreach (Type classType in types)
                if (classType.IsEnum)
                    enumTypes.Add(classType);
                else
                {
                    nonEnumTypes.Add(classType);

                    foreach (MemberInfo memberInfo in classType.GetMembers(bindingFlags)
                        // We don't check those member which doesn't want to be checked
                        .Where(e => !e.IsDefined(typeof(DoNotInspectAttribute))))
                    {
                        memberInfos.Add(memberInfo);

                        switch (memberInfo.MemberType)
                        {
                            case MemberTypes.Field:
                                FieldInfo fieldInfo = (FieldInfo)memberInfo;
                                if (fieldInfo.CanBeSerializedByUnity())
                                    fieldInfosSerializableByUnity.Add((FieldInfo)memberInfo);
                                else
                                    fieldInfosNonSerializableByUnity.Add((FieldInfo)memberInfo);
                                break;
                            case MemberTypes.Property:
                                propertyInfos.Add((PropertyInfo)memberInfo);
                                break;
                            case MemberTypes.Method:
                                MethodInfo methodInfo = (MethodInfo)memberInfo;
                                methodInfos.Add(methodInfo);
                                GetExecuteAttributes(methodInfo);
                                break;
                        }
                    }
                }
        }

        private static void GetExecuteAttributes(MethodInfo methodInfo)
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
                            SubscribeOnEachTypeEnum(action, loop);
                        if ((typeFlags & ExecuteOnEachTypeWhenScriptsReloads.TypeFlags.IsNonEnum) != 0)
                            SubscribeOnEachTypeLessEnums(action, loop);
                    }
                }
                if (attribute is ExecuteOnEachMemberOfEachTypeWhenScriptsReloads executeOnEachMemberOfEachTypeWhenScriptsReloads)
                    if (TryGetDelegate(methodInfo, out Action<MemberInfo> action))
                        SubscribeOnEachMemberOfTypes(action, loop);
                if (attribute is ExecuteOnEachFieldOfEachTypeWhenScriptsReloads executeOnEachFieldOfEachTypeWhenScriptsReloads)
                    if (TryGetDelegate(methodInfo, out Action<FieldInfo> action))
                    {
                        FieldSerialization fieldFags = executeOnEachFieldOfEachTypeWhenScriptsReloads.fieldFilter;
                        if ((fieldFags & FieldSerialization.SerializableByUnity) != 0)
                            SubscribeOnEachSerializableByUnityFieldOfTypes(action, loop);
                        if ((fieldFags & FieldSerialization.NotSerializableByUnity) != 0)
                            SubscribeOnEachNonSerializableByUnityFieldOfTypes(action, loop);
                    }
                if (attribute is ExecuteOnEachPropertyOfEachTypeWhenScriptsReloads executeOnEachPropertyOfEachTypeWhenScriptsReloads)
                    if (TryGetDelegate(methodInfo, out Action<PropertyInfo> action))
                        SubscribeOnEachPropertyOfTypes(action, loop);
                if (attribute is ExecuteOnEachMethodOfEachTypeWhenScriptsReloads executeOnEachMethodOfEachTypeWhenScriptsReloads)
                    if (TryGetDelegate(methodInfo, out Action<MethodInfo> action))
                        SubscribeOnEachMethodOfTypes(action, loop);
                if (attribute is ExecuteWhenScriptsReloads executeWhenScriptsReloads)
                    if (TryGetDelegate(methodInfo, out Action action))
                        SubscribeToExecuteOnce(action, loop);
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
                Type[] genericArguments = typeof(T).GetGenericArguments();
                string signature = genericArguments.Length == 0 ? "nothing" : string.Join(", ", genericArguments.Select(a => a.Name));
                Debug.LogException(new ArgumentException(string.Format(ATTRIBUTE_METHOD_ERROR, methodInfo.Name, methodInfo.DeclaringType, signature), e));
            }
            return null;
        }

        private static void ExecuteCallbacks()
        {
            foreach (int loop in GetKeySortedUnion(
                executeOnEachTypeEnum.Keys,
                executeOnEachTypeLessEnums.Keys,
                executeOnEachMemberOfTypes.Keys,
                executeOnEachSerializableByUnityFieldOfTypes.Keys,
                executeOnEachNonSerializableByUnityFieldOfTypes.Keys,
                executeOnEachPropertyOfTypes.Keys,
                executeOnEachMethodOfTypes.Keys
                ))
            {
                void ExecuteList<T>(Dictionary<int, Action<T>> callbacks, List<T> values) => ExecuteLoop(loop, callbacks, values);

                Task.WaitAll(new Task[] {
                    Task.Run(() => ExecuteList(executeOnEachTypeEnum, enumTypes)),
                    Task.Run(() => ExecuteList(executeOnEachTypeLessEnums, nonEnumTypes)),
                    Task.Run(() => ExecuteList(executeOnEachMemberOfTypes, memberInfos)),
                    Task.Run(() => ExecuteList(executeOnEachSerializableByUnityFieldOfTypes, fieldInfosSerializableByUnity)),
                    Task.Run(() => ExecuteList(executeOnEachNonSerializableByUnityFieldOfTypes, fieldInfosNonSerializableByUnity)),
                    Task.Run(() => ExecuteList(executeOnEachPropertyOfTypes, propertyInfos)),
                    Task.Run(() => ExecuteList(executeOnEachMethodOfTypes, methodInfos)),
                    Task.Run(() => {
                        if (executeOnce.TryGetValue(loop, out Action action))
                            action();
                    })
                });
            }
        }

        private static IEnumerable<int> GetKeySortedUnion(params IEnumerable<int>[] keys) => keys.SelectMany(e => e).Distinct().OrderByDescending(e => e).Reverse();

        private static void ExecuteLoop<T>(int loop, Dictionary<int, Action<T>> callbacks, List<T> values)
        {
            if (callbacks.TryGetValue(loop, out Action<T> action))
                values.ForEach(action);
        }
    }
}