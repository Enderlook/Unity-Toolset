using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Utils
{
    public static partial class SerializedPropertyHelper
    {
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly Regex AssemblyNameRegex = new Regex("^(.*?),", RegexOptions.Compiled);
        private static readonly Regex ArrayRegex = new Regex(@"^(.*?)(?:\[])?$", RegexOptions.Compiled);
        private static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>();
        private static readonly Dictionary<Type, (MethodInfo GetCount, MethodInfo SetItems)> IListMethods = new Dictionary<Type, (MethodInfo GetCount, MethodInfo SetItems)>();
        private static ReadWriteLock IListItemsSetLock = new ReadWriteLock();

        private static BackgroundTask task;
        private static List<SerializedPropertyPathNode> nodes;
        private static Type[] oneType;
        private static object[] twoObjects;

        [DidReloadScripts]
        private static void Reset()
        {
            IListItemsSetLock.WriteBegin();
            {
                IListMethods.Clear();
            }
            IListItemsSetLock.WriteEnd();

            task = BackgroundTask.Enqueue(
#if UNITY_2020_1_OR_NEWER
                token => Progress.Start($"Initialize {typeof(SerializedPropertyHelper)}", "Enqueued process."),
                (id, token) =>
#else
                token =>
#endif
                {
                    if (token.IsCancellationRequested)
                        goto cancelled;
#if UNITY_2020_1_OR_NEWER
                    Progress.SetDescription(id, null);
#endif
                    Types.Clear();

                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
#if UNITY_2020_1_OR_NEWER
                    int total = 0;
                    foreach (Assembly assembly in assemblies)
                        total += assembly.GetTypes().Length;
                    Progress.Report(id, 0, total);

                    int current = 0;
#endif
                    foreach (Assembly assembly in assemblies)
                    {
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (token.IsCancellationRequested)
                                goto cancelled;

#if UNITY_2020_1_OR_NEWER
                            Progress.Report(id, current++, total);
#endif

                            if (type.IsValueType)
                                continue;

                            string assemblyName = AssemblyNameRegex.Match(type.Assembly.FullName).Groups[1].Value;
                            string namespaceParentAndTypeName = type.FullName.Replace('+', '/');
                            string name = $"{assemblyName} {namespaceParentAndTypeName}";
                            Types.Add(name, type);
                        }
                    }

#if UNITY_2020_1_OR_NEWER
                    Progress.Finish(id);
                    return;
                cancelled:;
                    Progress.Finish(id, Progress.Status.Canceled);
#else
                cancelled:;
#endif
                }
            );
        }

        /// <summary>
        /// Gets the property nodes hierarchy of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="nodes">Hierarchy traveled to get the property nodes.</param>
        /// <param name="count">Depth in the hierarchy to travel.</param>
        /// <returns>If <see langword="true"/> <paramref name="target"/> was found. Otherwise <paramref name="target"/> contains an undefined value.</returns>
        private static bool GetPropertyNodes<TCanThrow>(this SerializedProperty source, List<SerializedPropertyPathNode> nodes, int count)
        {
            // https://github.com/lordofduct/spacepuppy-unity-framework-4.0/blob/master/Framework/com.spacepuppy.core/Editor/src/EditorHelper.cs

            if (source == null)
            {
                if (Toggle.IsToggled<TCanThrow>()) Helper.ThrowArgumentNullException_Source();
                goto isFalse;
            }

            if (nodes is null)
            {
                if (Toggle.IsToggled<TCanThrow>()) ThrowNodesIsNull();
                goto isFalse;
            }

            nodes.Clear();

            int total = 1;
            ReadOnlySpan<char> propertyPath = source.propertyPath.AsSpan();
            for (int j = 0; j < propertyPath.Length; j++)
            {
                if (propertyPath[j] == '.')
                {
                    if (propertyPath.Slice(j).StartsWith(".Array.data[".AsSpan()))
                        j += ".Array.data[".Length;
                    else
                        total++;
                }
            }

            if (count > total)
            {
                if (Toggle.IsToggled<TCanThrow>()) ThrowCountMustBeLowerThanTotal();
                goto isFalse;
            }

            int remaining = total - count;
            object target = source.serializedObject.targetObject;
            SerializedPropertyPathNode node = new SerializedPropertyPathNode
            {
                Object = target,
                Index = -1,
                path = "source.serializedObject.targetObject"
            };

            if (remaining-- == 0)
            {
                nodes.Add(node);
                return true;
            }

            if (target == null)
            {
                if (Toggle.IsToggled<TCanThrow>()) ThrowTargetObjectIsNull();
                goto isFalse;
            }

            int i = -1;
            ReadOnlySpan<char> remainingPath = source.propertyPath.AsSpan();
            while (remainingPath.Length > 0)
            {
                i++;
                int remainingPathIndex = remainingPath.IndexOf('.');
                ReadOnlySpan<char> pathSection;
                int? hasIndex;
                if (remainingPathIndex == -1)
                {
                    pathSection = remainingPath;
                    remainingPath = default;
                    hasIndex = default;
                }
                else if (remainingPath.Slice(remainingPathIndex).StartsWith(".Array.data[".AsSpan()))
                {
                    int start = remainingPathIndex + ".Array.data[".Length;
                    int endIndex = remainingPath.IndexOf(']');
                    ReadOnlySpan<char> indexText = remainingPath.Slice(start, endIndex - start);
                    // TODO: In .Net Standard 2.1 the .ToString() can be avoided.
                    hasIndex = int.Parse(indexText.ToString());
                    pathSection = remainingPath.Slice(0, remainingPath.IndexOf('.'));
                    endIndex = remainingPath.Length == ++endIndex ? endIndex : endIndex + 1;
                    remainingPath = remainingPath.Slice(endIndex);
                }
                else
                {
                    pathSection = remainingPath.Slice(0, remainingPathIndex);
                    remainingPath = remainingPath.Slice(remainingPathIndex + 1);
                    hasIndex = default;
                }

                string elementName = pathSection.ToString();

                if (hasIndex is int index)
                {
                    if (!SetObj(elementName, out target, out MemberInfo memberInfo)) goto isFalse;

                    node.MemberInfo = memberInfo;
                    nodes.Add(node);
                    node.Object = target;
                    node.path = elementName;

                    if (target is null)
                    {
                        if (Toggle.IsToggled<TCanThrow>()) ThrowArrayDataIsNull();
                        goto isFalse;
                    }

                    void ThrowArrayDataIsNull()
                        => throw new ArgumentException($"source.serializedObject.targetObject.{GetPathSections(i)}.{elementName}.Array.data");

                    if (target is IList list)
                    {
                        if (list.Count > index)
                        {
                            target = list[index];
                            goto next;
                        }
                        else if (Toggle.IsToggled<TCanThrow>()) ThrowIndexMustBeLowerThanArraySize();
                        else goto isFalse;
                    }

                    void ThrowIndexMustBeLowerThanArraySize()
                    {
                        string subPath = GetPathSections(i);
                        string subPathPlusOne = GetPathSections(i + 1);
                        throw new ArgumentException($"source.serializedObject.targetObject.{subPathPlusOne}", $"Index {index} at 'source.serializedObject.targetObject.{subPathPlusOne}' must be lower than 'source.serializedObject.targetObject.{subPath}{(string.IsNullOrEmpty(subPath) ? "" : ".")}{elementName}.Array.arraySize' ({list.Count})");
                    }

                    if (target is IEnumerable enumerable)
                    {
                        IEnumerator enumerator = enumerable.GetEnumerator();

                        for (int j = 0; j <= index; j++)
                        {
                            if (!enumerator.MoveNext())
                            {
                                if (Toggle.IsToggled<TCanThrow>()) ThrowEnumerableExhausted();
                                nodes.Clear();
                                return false;
                            }

                            void ThrowEnumerableExhausted()
                            {
                                string subPath = GetPathSections(i);
                                string subPathPlusOne = GetPathSections(i + 1);
                                throw new ArgumentException($"source.serializedObject.targetObject.{subPathPlusOne}", $"Index {index} at 'source.serializedObject.targetObject.{subPathPlusOne}' must be lower than 'source.serializedObject.targetObject.{subPath}{(string.IsNullOrEmpty(subPath) ? "" : ".")}{elementName}.Array.arraySize' ({j})");
                            }
                        }

                        target = enumerator.Current;
                        goto next;
                    }
                    else
                        Debug.Assert(false, "Impossible state.");

                    next:;
                    node.Index = index;
                    node.MemberInfo = null;
                    nodes.Add(node);
                    node.Object = target;
                    node.Index = -1;
                    node.path = null;
                }
                else if (SetObj(elementName, out target, out MemberInfo memberInfo))
                {
                    node.MemberInfo = memberInfo;
                    nodes.Add(node);
                    node.Object = target;
                    node.path = elementName;
                }
                else
                    goto isFalse;

                if (remaining-- == 0)
                {
                    node.MemberInfo = null;
                    nodes.Add(node);
                    return true;
                }

                if (target == null)
                {
                    if (Toggle.IsToggled<TCanThrow>()) ThrowTargetNull();
                    goto isFalse;

                    void ThrowTargetNull()
                        => throw new ArgumentException($"source.serializedObject.targetObject.{GetPathSections(i + 1)}");
                }

                bool SetObj(string name, out object target__, out MemberInfo memberInfo)
                {
                    Type type = target.GetType();

                    while (true)
                    {
                        FieldInfo fieldInfo = type.GetField(name, bindingFlags);
                        if (!(fieldInfo is null))
                        {
                            target__ = fieldInfo.GetValue(target);
                            memberInfo = fieldInfo;
                            break;
                        }

                        PropertyInfo propertyInfo = type.GetProperty(name, bindingFlags | BindingFlags.IgnoreCase);
                        if (!(propertyInfo is null))
                        {
                            target__ = propertyInfo.GetValue(target, null);
                            memberInfo = propertyInfo;
                            break;
                        }

                        type = type.BaseType;

                        if (type is null)
                        {
                            if (Toggle.IsToggled<TCanThrow>()) ThrowMemberNotFound();
                            target__ = default;
                            memberInfo = default;
                            return false;
                        }
                    }

                    return true;

                    void ThrowMemberNotFound()
                    {
                        string subPath = GetPathSections(i);
                        throw new InvalidOperationException($"From path 'source.serializedObject.targetObject.{source.propertyPath}', member '{name}' (at 'source.serializedObject.targetObject.{subPath}{(string.IsNullOrEmpty(subPath) ? "" : ".")}{name}') was not found.");
                    }
                }
            }

            Debug.Assert(false, "Impossible state.");
        isFalse:
            nodes.Clear();
            return false;

            string GetPathSections(int take)
            {
                if (take == 0)
                    return default;

                ReadOnlySpan<char> propertyPath_ = source.propertyPath.AsSpan();
                int i = 0;
                for (int j = 0; j < propertyPath_.Length; j++)
                {
                    if (propertyPath_[j] == '.')
                    {
                        if (propertyPath_.Slice(j).StartsWith(".Array.data[".AsSpan()))
                            j += ".Array.data[".Length;
                        else if (++i == take)
                            return propertyPath_.Slice(0, j).ToString();
                    }
                }

                return source.propertyPath;
            }

            void ThrowNodesIsNull()
                => throw new ArgumentNullException(nameof(nodes));

            void ThrowCountMustBeLowerThanTotal()
                => throw new ArgumentOutOfRangeException(nameof(count), $"Path of '{source.displayName}' property ('{source.propertyPath}') contains {total} sections, but count was {count}, which is not a lower number.");

            void ThrowTargetObjectIsNull()
                => throw new ArgumentException("source.serializedObject.targetObject");
        }

        /// <summary>
        /// Gets the target object of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="last">At which depth from last to first should return.</param>
        /// <returns>Value of the <paramref name="source"/> as <see cref="object"/>.</returns>
        public static object GetTargetObject(this SerializedProperty source, int last = 0)
        {
            List<SerializedPropertyPathNode> nodes_ = Interlocked.Exchange(ref nodes, null) ?? new List<SerializedPropertyPathNode>();
            source.GetPropertyNodes<Toggle.Yes>(nodes_, last);
            object @object = nodes_[nodes_.Count - 1].Object;
            nodes_.Clear();
            nodes = nodes_;
            return @object;
        }

        /// <summary>
        /// Try get the target object of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="target">Value of the <paramref name="source"/> as <see cref="object"/> if returns <see langword="true"/>.</param>
        /// <param name="last">At which depth from last to first should return.</param>
        /// <returns>If <see langword="true"/>, <paramref name="target"/> was got successfully.</returns>
        public static bool TryGetTargetObject(this SerializedProperty source, out object target, int last = 0)
        {
            List<SerializedPropertyPathNode> nodes_ = Interlocked.Exchange(ref nodes, null) ?? new List<SerializedPropertyPathNode>();
            if (source.GetPropertyNodes<Toggle.No>(nodes_, last))
            {
                target = nodes_[nodes_.Count - 1].Object;
                nodes_.Clear();
                nodes = nodes_;
                return true;
            }
            target = default;
            nodes_.Clear();
            nodes = nodes_;
            return false;
        }

        /// <summary>
        /// Get the value of <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of value in the property.</typeparam>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <returns>Value of target object.</returns>
        public static T GetValue<T>(this SerializedProperty source)
        {
            // Insert JIT optimizable fast paths to expected values.
            // Code is splitted in sub methods to help JIT heuristics for constant propagation.

            if (!typeof(T).IsValueType)
                return FromReferenceType();
            if (typeof(T).IsPrimitive)
                return FromPrimitiveType();
            if (typeof(Enum).IsAssignableFrom(typeof(T)) && source.propertyType == SerializedPropertyType.Enum)
                return FromEnum();
            return FromValueType();

            T FromReferenceType()
            {
                switch (source.propertyType)
                {
                    case SerializedPropertyType.AnimationCurve:
                        return (T)(object)source.animationCurveValue;
                    case SerializedPropertyType.String:
                        return (T)(object)source.stringValue;
                    case SerializedPropertyType.Enum:
                        return FromEnum();
                    case SerializedPropertyType.ObjectReference:
                        return (T)(object)source.objectReferenceValue;
                    case SerializedPropertyType.ExposedReference:
                        return (T)(object)source.exposedReferenceValue;
#if UNITY_2019_3_OR_NEWER
                    case SerializedPropertyType.ManagedReference:
#if UNITY_2021_2_OR_NEWER
                        return source.managedReferenceValue;
#endif
#endif
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Generic:
                        return (T)source.GetTargetObject();
                }
                return Fallback();
            }

            T FromPrimitiveType()
            {
                switch (Unsafe.SizeOf<T>())
                {
                    case 1:
                        return From1Byte();
                    case 2:
                        return From2Bytes();
                    case 4:
                        return From4Bytes();
                    case 8:
                        return From8Bytes();
                    default:
                        return Fallback();
                }

                T From1Byte()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(bool) && propertyType == SerializedPropertyType.Boolean)
                        return (T)(object)source.boolValue;
                    if (typeof(T) == typeof(byte) && propertyType == SerializedPropertyType.Integer && source.type == "byte")
                        return (T)(object)(byte)source.intValue;
                    if (typeof(T) == typeof(sbyte) && propertyType == SerializedPropertyType.Integer && source.type == "sbyte")
                        return (T)(object)(sbyte)source.intValue;
                    return Fallback();
                }

                T From2Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(short) && propertyType == SerializedPropertyType.Integer && source.type == "short")
                        return (T)(object)(short)source.intValue;
                    if (typeof(T) == typeof(ushort) && propertyType == SerializedPropertyType.Integer && source.type == "ushort")
                        return (T)(object)(ushort)source.intValue;
                    if (typeof(T) == typeof(char) && propertyType == SerializedPropertyType.Character)
                        return (T)(object)(char)source.intValue;
                    return Fallback();
                }

                T From4Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(int))
                    {
                        switch (propertyType)
                        {
                            case SerializedPropertyType.ArraySize:
                                return (T)(object)source.arraySize;
                            case SerializedPropertyType.Integer when source.type == "int":
                                return (T)(object)source.intValue;
                            case SerializedPropertyType.FixedBufferSize:
                                return (T)(object)source.fixedBufferSize;
                            default:
                                goto fallback;
                        }
                    }
                    if (typeof(T) == typeof(uint) && propertyType == SerializedPropertyType.Integer && source.type == "uint")
                        return (T)(object)(uint)source.longValue;
                    if (typeof(T) == typeof(float) && propertyType == SerializedPropertyType.Float && source.type == "float")
                        return (T)(object)source.floatValue;
                    fallback:
                    return Fallback();
                }

                T From8Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(long) && propertyType == SerializedPropertyType.Integer && source.type == "long")
                        return (T)(object)source.longValue;
                    if (typeof(T) == typeof(ulong) && propertyType == SerializedPropertyType.Integer && source.type == "ulong")
                        return (T)(object)(ulong)source.longValue;
                    if (typeof(T) == typeof(double) && propertyType == SerializedPropertyType.Float && source.type == "double")
                        return (T)(object)source.doubleValue;
                    return Fallback();
                }
            }

            T FromValueType()
            {
                switch (Unsafe.SizeOf<T>())
                {
                    case 4:
                        return From4Bytes();
                    case 8:
                        return From8Bytes();
                    case 12:
                        return From12Bytes();
                    case 16:
                        return From16Bytes();
                    case 24:
                        return From24Bytes();
                    default:
                        return Fallback();
                }

                T From4Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(LayerMask) && propertyType == SerializedPropertyType.LayerMask)
                        return (T)(object)(LayerMask)source.intValue;
                    return Fallback();
                }

                T From8Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Vector2) && propertyType == SerializedPropertyType.Vector2)
                        return (T)(object)source.vector2Value;
                    if (typeof(T) == typeof(Vector2Int) && propertyType == SerializedPropertyType.Vector2Int)
                        return (T)(object)source.vector2IntValue;
                    return Fallback();
                }

                T From12Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Vector3) && propertyType == SerializedPropertyType.Vector3)
                        return (T)(object)source.vector3Value;
                    if (typeof(T) == typeof(Vector3Int) && propertyType == SerializedPropertyType.Vector3Int)
                        return (T)(object)source.vector3IntValue;
                    return Fallback();
                }

                T From16Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Vector4) && propertyType == SerializedPropertyType.Vector4)
                        return (T)(object)source.vector4Value;
                    if (typeof(T) == typeof(Color) && propertyType == SerializedPropertyType.Color)
                        return (T)(object)source.colorValue;
                    if (typeof(T) == typeof(Quaternion) && propertyType == SerializedPropertyType.Quaternion)
                        return (T)(object)source.quaternionValue;
                    if (typeof(T) == typeof(Rect) && propertyType == SerializedPropertyType.Rect)
                        return (T)(object)source.rectValue;
                    if (typeof(T) == typeof(RectInt) && propertyType == SerializedPropertyType.RectInt)
                        return (T)(object)source.rectIntValue;
                    if (typeof(T) == typeof(decimal) && propertyType == SerializedPropertyType.Float && source.type == "decimal")
                        return (T)(object)(decimal)source.doubleValue;
#if UNITY_2021_1_OR_NEWER
                    if (typeof(T) == typeof(Hash128) && propertyType == SerializedPropertyType.Hash128)
                        return (Hash128)(object)source.hash128Value;
#endif
                    return Fallback();
                }

                T From24Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Bounds) && propertyType == SerializedPropertyType.Bounds)
                        return (T)(object)source.boundsValue;
                    if (typeof(T) == typeof(BoundsInt) && propertyType == SerializedPropertyType.BoundsInt)
                        return (T)(object)source.boundsIntValue;
                    return Fallback();
                }
            }

            T FromEnum()
            {
                if (typeof(T) == typeof(Enum))
                    goto fallback;

                Type underlyingType = Enum.GetUnderlyingType(typeof(T));
                if (underlyingType == typeof(int))
                    return (T)Enum.ToObject(typeof(T), source.intValue);
                if (underlyingType == typeof(short))
                    return (T)Enum.ToObject(typeof(T), (short)source.intValue);
                if (underlyingType == typeof(long))
                    return (T)Enum.ToObject(typeof(T), source.longValue);
                if (underlyingType == typeof(byte))
                    return (T)Enum.ToObject(typeof(T), (byte)source.intValue);
                if (underlyingType == typeof(uint))
                    return (T)Enum.ToObject(typeof(T), (uint)source.longValue);
                if (underlyingType == typeof(ushort))
                    return (T)Enum.ToObject(typeof(T), (ushort)source.intValue);
                if (underlyingType == typeof(ulong))
                    return (T)Enum.ToObject(typeof(T), (ulong)source.longValue);
                if (underlyingType == typeof(sbyte))
                    return (T)Enum.ToObject(typeof(T), (sbyte)source.intValue);
                if (underlyingType == typeof(char))
                    return (T)Enum.ToObject(typeof(T), (char)source.intValue);
                if (underlyingType == typeof(float))
                    return (T)Enum.ToObject(typeof(T), (float)source.floatValue);
                if (underlyingType == typeof(double))
                    return (T)Enum.ToObject(typeof(T), (double)source.doubleValue);

                fallback:
                return (T)GetTargetObject(source);
            }

            T Fallback()
            {
                switch (source.propertyType)
                {
                    case SerializedPropertyType.AnimationCurve:
                        return (T)(object)source.animationCurveValue;
                    case SerializedPropertyType.ArraySize:
                        return (T)(object)source.arraySize;
                    case SerializedPropertyType.FixedBufferSize:
                        return (T)(object)source.fixedBufferSize;
                    case SerializedPropertyType.Integer:
                        // Technically, int and long are the only valid types,
                        // but we put the others to be sure.
                        switch (source.type)
                        {
                            case "long":
                                return (T)(object)source.longValue;
                            case "int":
                                return (T)(object)source.intValue;
                            case "short":
                                return (T)(object)(short)source.intValue;
                            case "byte":
                                return (T)(object)(byte)source.intValue;
                            case "ulong":
                                return (T)(object)(ulong)source.intValue;
                            case "uint":
                                return (T)(object)(uint)source.longValue;
                            case "ushort":
                                return (T)(object)(ushort)source.intValue;
                            case "sbyte":
                                return (T)(object)(sbyte)source.intValue;
                            default:
                                goto fallback;
                        }
                    case SerializedPropertyType.Boolean:
                        return (T)(object)source.boolValue;
                    case SerializedPropertyType.Bounds:
                        return (T)(object)source.boundsValue;
                    case SerializedPropertyType.BoundsInt:
                        return (T)(object)source.boundsIntValue;
                    case SerializedPropertyType.Character:
                        return (T)(object)(char)source.intValue;
                    case SerializedPropertyType.Color:
                        return (T)(object)source.colorValue;
                    case SerializedPropertyType.Float:
                        // Technically, float and double are the only valid types,
                        // but we put the others to be sure.
                        switch (source.type)
                        {
                            case "float":
                                return (T)(object)source.floatValue;
                            case "double":
                                return (T)(object)source.doubleValue;
                            case "decimal":
                                return (T)(object)(decimal)source.doubleValue;
                            default:
                                goto fallback;
                        }
                    case SerializedPropertyType.LayerMask:
                        return (T)(object)(LayerMask)source.intValue;
                    case SerializedPropertyType.Quaternion:
                        return (T)(object)source.quaternionValue;
                    case SerializedPropertyType.Rect:
                        return (T)(object)source.rectValue;
                    case SerializedPropertyType.RectInt:
                        return (T)(object)source.rectIntValue;
                    case SerializedPropertyType.String:
                        return (T)(object)source.stringValue;
                    case SerializedPropertyType.Vector2:
                        return (T)(object)source.vector2Value;
                    case SerializedPropertyType.Vector2Int:
                        return (T)(object)source.vector2IntValue;
                    case SerializedPropertyType.Vector3:
                        return (T)(object)source.vector3Value;
                    case SerializedPropertyType.Vector3Int:
                        return (T)(object)source.vector3IntValue;
                    case SerializedPropertyType.Vector4:
                        return (T)(object)source.vector4Value;
                    case SerializedPropertyType.Enum:
                        return FromEnum();
                    case SerializedPropertyType.ObjectReference:
                        return (T)(object)source.objectReferenceValue;
                    case SerializedPropertyType.ExposedReference:
                        return (T)(object)source.exposedReferenceValue;
#if UNITY_2019_3_OR_NEWER
                    case SerializedPropertyType.ManagedReference:
#if UNITY_2021_2_OR_NEWER
                        return source.managedReferenceValue;
#endif
#endif
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Generic:
                    default:
                    fallback:
                        return (T)GetTargetObject(source);
                }
            }
        }

        /// <summary>
        /// Gets the parent target object of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <returns>Value of the <paramref name="source"/> as <see cref="object"/>.</returns>
        public static object GetParentTargetObject(this SerializedProperty source)
            => source.GetTargetObject(1);

        /// <summary>
        /// Gets the parent target object of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="target">Value of the <paramref name="source"/> as <see cref="object"/> if returns <see langword="true"/>.</param>
        /// <returns>If <see langword="true"/>, <paramref name="target"/> was got successfully.</returns>
        public static bool TryGetParentTargetObject(this SerializedProperty source, out object target)
            => source.TryGetTargetObject(out target, 1);

        /// <summary>
        /// Sets the target object of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="newTarget">New target value.</param>
        /// <param name="notThrow">If <see langword="true"/> it will not throw if there is an error.</param>
        /// <returns>If <see langword="true"/> the value was set successfully. On <see langword="false"/>, undefined behaviour, the value may or may not have been defined.</returns>
        private static bool SetTargetObject<T>(this SerializedProperty source, T newTarget, bool notThrow)
        {
            List<SerializedPropertyPathNode> nodes_ = Interlocked.Exchange(ref nodes, null) ?? new List<SerializedPropertyPathNode>();
            if (source.GetPropertyNodes<Toggle.Yes>(nodes_, 0))
            {
                int i = nodes_.Count - 2;
                SerializedPropertyPathNode node = nodes_[i];
                if (!Set<T, Toggle.Yes>(node, newTarget))
                    goto error;

                while (i > 0 && node.Object.GetType().IsValueType)
                {
                    node = nodes_[i];
                    SerializedPropertyPathNode previousNode = nodes_[--i];
                    if (!Set<object, Toggle.No>(previousNode, node.Object))
                        goto error;
                    node = previousNode;
                }

                bool Set<U, TFirstCall>(SerializedPropertyPathNode previousNode, U newValue_)
                {
                    switch (previousNode.MemberInfo)
                    {
                        case FieldInfo fieldInfo:
                            fieldInfo.SetValue(previousNode.Object, newValue_);
                            break;
                        case PropertyInfo propertyInfo:
                            propertyInfo.SetValue(previousNode.Object, newValue_);
                            break;
                        case null:
                            {
                                // TODO: We could create Toggle.Yes and Toggle.No types.
                                if (Toggle.IsToggled<TFirstCall>())
                                {
                                    if (previousNode.Object is IList<U> list)
                                    {
                                        if (previousNode.Index < list.Count)
                                        {
                                            list[previousNode.Index] = newValue_;
                                            break;
                                        }
                                        goto indexError;
                                    }
                                }

                                // Both Array and List<T> implement IList.
                                if (previousNode.Object is IList list_)
                                {
                                    if (previousNode.Index < list_.Count)
                                    {
                                        list_[previousNode.Index] = newValue_;
                                        break;
                                    }
                                    goto indexError;
                                }

                                Type[] typeArray = Interlocked.Exchange(ref oneType, null) ?? new Type[1];
                                Type previousNodeObjectType = previousNode.Object.GetType();
                                Type newValueTypeOriginal = newValue_.GetType();

                                Type newValueType = newValueTypeOriginal;
                                while (!(newValueType is null))
                                {
                                    switch (TrySet(newValueType))
                                    {
                                        case 1:
                                            break;
                                        case -1:
                                            goto indexError;
                                    }
                                    newValueType = newValueType.BaseType;
                                }

                                foreach (Type newValueInterfaceType in newValueTypeOriginal.GetInterfaces())
                                {
                                    switch (TrySet(newValueType))
                                    {
                                        case 1:
                                            break;
                                        case -1:
                                            goto indexError;
                                    }
                                }

                                typeArray[0] = null;
                                oneType = typeArray;

                                int TrySet(Type type_)
                                {
                                    typeArray[0] = type_;
                                    Type concreteType = typeof(IList<>).MakeGenericType(typeArray);
                                    if (concreteType.IsAssignableFrom(previousNodeObjectType))
                                    {
                                        typeArray[0] = null;
                                        oneType = typeArray;

                                        (MethodInfo GetCount, MethodInfo SetItems) methodInfos;
                                        bool found;
                                        IListItemsSetLock.ReadBegin();
                                        {
                                            found = IListMethods.TryGetValue(concreteType, out methodInfos);
                                        }
                                        IListItemsSetLock.ReadEnd();
                                        if (!found)
                                        {
                                            IListItemsSetLock.WriteBegin();
                                            {
                                                if (!IListMethods.TryGetValue(concreteType, out methodInfos))
                                                {
                                                    MethodInfo setItem = concreteType.GetMethod("set_Item");
                                                    MethodInfo getCount = concreteType.GetMethod("get_Count");
                                                    IListMethods.Add(concreteType, methodInfos = (getCount, setItem));
                                                }
                                            }
                                            IListItemsSetLock.WriteEnd();
                                        }

                                        if (previousNode.Index < (int)methodInfos.GetCount.Invoke(previousNode.Object, null))
                                        {
                                            object[] objectsArray = Interlocked.Exchange(ref twoObjects, null) ?? new object[2];
                                            objectsArray[1] = newValue_;
                                            objectsArray[0] = previousNode.Index;
                                            methodInfos.SetItems.Invoke(previousNode.Object, objectsArray);
                                            objectsArray[1] = null;
                                            objectsArray[0] = null;
                                            twoObjects = objectsArray;
                                            return 1;
                                        }
                                        return -1;
                                    }
                                    return 0;
                                }

                                if (notThrow) goto error_;
                                ThrowTypeNotSupported();

                                void ThrowTypeNotSupported() => throw new NotSupportedException($"Error while assigning value to '{string.Join(".", nodes_.Take(i + 1).Select(e => e.path ?? $"Array.data[{e.Index}]"))}' from path '{string.Join(".", nodes_.Select(e => e.path ?? $"Array.data[{e.Index}]"))}'. Can only mutate fields, properties or types that implement {nameof(IList)} or IList<T>, where T is any type that is assignable from the value to assign.");

                            indexError:
                                if (notThrow) goto error_;
                                ThrowIndexMustBeLowerThanArraySize();

                                void ThrowIndexMustBeLowerThanArraySize()
                                    => throw new NotSupportedException($"Error while assigning value to '{string.Join(".", nodes_.Take(i + 1).Select(e => e.path ?? $"Array.data[{e.Index}]"))}' from path '{string.Join(".", nodes_.Select(e => e.path ?? $"Array.data[{e.Index}]"))}'. Index {previousNode.Index} must be lwoer than '{string.Join(".", nodes_.Take(i).Select(e => e.path ?? $"Array.data[{e.Index}]"))}.Array.arraySize' ({((IList)previousNode.Object).Count}).");

                                break;
                            }
                        default:
                            Debug.Assert(false, "Impossible state.");
                            break;
                    }
                    nodes_.Clear();
                    nodes = nodes_;
                    return true;
                error_:
                    nodes_.Clear();
                    nodes = nodes_;
                    return false;
                }
            }
        error:
            nodes_.Clear();
            nodes = nodes_;
            return false;
        }

        /// <summary>
        /// Sets the target object of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="newTarget">New target value.</param>
        public static void SetTargetObject(this SerializedProperty source, object newTarget)
            => source.SetTargetObject(newTarget, false);

        /// <summary>
        /// Sets the target object of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="newTarget">New target value.</param>
        /// <returns>If <see langword="true"/> the value was set successfully. On <see langword="false"/>, undefined behaviour, the value may or may not have been defined.</returns>
        public static bool TrySetTargetObject(this SerializedProperty source, object newTarget)
            => source.SetTargetObject(newTarget, true);

        /// <summary>
        /// Sets the value of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="newValue">New target value.</param>
        public static void SetValue<T>(this SerializedProperty source, T newValue)
        {
            // Insert JIT optimizable fast paths to expected values.
            // Code is splitted in sub methods to help JIT heuristics for constant propagation.

            if (!typeof(T).IsValueType)
                FromReferenceType();
            else if (typeof(T).IsPrimitive)
                FromPrimitiveType();
            else if (typeof(Enum).IsAssignableFrom(typeof(T)) && source.propertyType == SerializedPropertyType.Enum)
                FromEnum();
            else
                FromValueType();

            void FromReferenceType()
            {
                switch (source.propertyType)
                {
                    case SerializedPropertyType.AnimationCurve:
                        source.animationCurveValue = (AnimationCurve)(object)newValue;
                        break;
                    case SerializedPropertyType.String:
                        source.stringValue = (string)(object)newValue;
                        break;
                    case SerializedPropertyType.Enum:
                        FromEnum();
                        break;
                    case SerializedPropertyType.ObjectReference:
                        source.objectReferenceValue = (UnityObject)(object)newValue;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        source.exposedReferenceValue = (UnityObject)(object)newValue;
                        break;
#if UNITY_2019_3_OR_NEWER
                    case SerializedPropertyType.ManagedReference:
                        source.managedReferenceValue = newValue;
                        break;
#endif
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Generic:
                        source.SetTargetObject(newValue, false);
                        break;
                    default:
                        Fallback();
                        break;
                }
            }

            void FromPrimitiveType()
            {
                switch (Unsafe.SizeOf<T>())
                {
                    case 1:
                        From1Byte();
                        break;
                    case 2:
                        From2Bytes();
                        break;
                    case 4:
                        From4Bytes();
                        break;
                    case 8:
                        From8Bytes();
                        break;
                    default:
                        Fallback();
                        break;
                }

                void From1Byte()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(bool) && propertyType == SerializedPropertyType.Boolean)
                        source.boolValue = (bool)(object)newValue;
                    else if (typeof(T) == typeof(byte) && propertyType == SerializedPropertyType.Integer && source.type == "byte")
                        source.intValue = (byte)(object)newValue;
                    else if (typeof(T) == typeof(sbyte) && propertyType == SerializedPropertyType.Integer && source.type == "sbyte")
                        source.intValue = (sbyte)(object)newValue;
                    else
                        Fallback();
                }

                void From2Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(short) && propertyType == SerializedPropertyType.Integer && source.type == "short")
                        source.intValue = (short)(object)newValue;
                    else if (typeof(T) == typeof(ushort) && propertyType == SerializedPropertyType.Integer && source.type == "ushort")
                        source.intValue = (ushort)(object)newValue;
                    else if (typeof(T) == typeof(char) && propertyType == SerializedPropertyType.Character)
                        source.intValue = (char)(object)newValue;
                    else
                        Fallback();
                }

                void From4Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(int))
                    {
                        switch (propertyType)
                        {
                            case SerializedPropertyType.ArraySize:
                                source.arraySize = (int)(object)newValue;
                                break;
                            case SerializedPropertyType.Integer when source.type == "int":
                                source.intValue = (int)(object)newValue;
                                break;
                            default:
                                goto fallback;
                        }
                    }
                    else if (typeof(T) == typeof(uint) && propertyType == SerializedPropertyType.Integer && source.type == "uint")
                        source.longValue = (uint)(object)newValue;
                    else if (typeof(T) == typeof(float) && propertyType == SerializedPropertyType.Float && source.type == "float")
                        source.floatValue = (float)(object)newValue;
                    else
                        goto fallback;
                    end:
                    return;
                fallback:
                    Fallback();
                    goto end;
                }

                void From8Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(long) && propertyType == SerializedPropertyType.Integer && source.type == "long")
                        source.longValue = (long)(object)newValue;
                    else if (typeof(T) == typeof(ulong) && propertyType == SerializedPropertyType.Integer && source.type == "ulong")
                        source.longValue = (long)(ulong)(object)newValue;
                    else if (typeof(T) == typeof(double) && propertyType == SerializedPropertyType.Float && source.type == "double")
                        source.doubleValue = (double)(object)newValue;
                    else
                        Fallback();
                }
            }

            void FromValueType()
            {
                switch (Unsafe.SizeOf<T>())
                {
                    case 4:
                        From4Bytes();
                        break;
                    case 8:
                        From8Bytes();
                        break;
                    case 12:
                        From12Bytes();
                        break;
                    case 16:
                        From16Bytes();
                        break;
                    case 24:
                        From24Bytes();
                        break;
                    default:
                        Fallback();
                        break;
                }

                void From4Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(LayerMask) && propertyType == SerializedPropertyType.LayerMask)
                        source.intValue = (LayerMask)(object)newValue;
                    else
                        Fallback();
                }

                void From8Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Vector2) && propertyType == SerializedPropertyType.Vector2)
                        source.vector2Value = (Vector2)(object)newValue;
                    else if (typeof(T) == typeof(Vector2Int) && propertyType == SerializedPropertyType.Vector2Int)
                        source.vector2IntValue = (Vector2Int)(object)newValue;
                    else Fallback();
                }

                void From12Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Vector3) && propertyType == SerializedPropertyType.Vector3)
                        source.vector3Value = (Vector3)(object)newValue;
                    else if (typeof(T) == typeof(Vector3Int) && propertyType == SerializedPropertyType.Vector3Int)
                        source.vector3IntValue = (Vector3Int)(object)newValue;
                    else
                        Fallback();
                }

                void From16Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Vector4) && propertyType == SerializedPropertyType.Vector4)
                        source.vector4Value = (Vector4)(object)newValue;
                    else if (typeof(T) == typeof(Color) && propertyType == SerializedPropertyType.Color)
                        source.colorValue = (Color)(object)newValue;
                    else if (typeof(T) == typeof(Quaternion) && propertyType == SerializedPropertyType.Quaternion)
                        source.quaternionValue = (Quaternion)(object)newValue;
                    else if (typeof(T) == typeof(Rect) && propertyType == SerializedPropertyType.Rect)
                        source.rectValue = (Rect)(object)newValue;
                    else if (typeof(T) == typeof(RectInt) && propertyType == SerializedPropertyType.RectInt)
                        source.rectIntValue = (RectInt)(object)newValue;
                    else if (typeof(T) == typeof(decimal) && propertyType == SerializedPropertyType.Float && source.type == "decimal")
                        source.doubleValue = (double)(decimal)(object)newValue;
#if UNITY_2021_1_OR_NEWER
                    else if (typeof(T) == typeof(Hash128) && propertyType == SerializedPropertyType.Hash128)
                       source.hash128Value = (Hash128)(object)newValue;
#endif
                    else
                        Fallback();
                }

                void From24Bytes()
                {
                    SerializedPropertyType propertyType = source.propertyType;
                    if (typeof(T) == typeof(Bounds) && propertyType == SerializedPropertyType.Bounds)
                        source.boundsValue = (Bounds)(object)newValue;
                    else if (typeof(T) == typeof(BoundsInt) && propertyType == SerializedPropertyType.BoundsInt)
                        source.boundsIntValue = (BoundsInt)(object)newValue;
                    else
                        Fallback();
                }
            }

            void FromEnum()
            {
                Type underlyingType;
                if (typeof(T) == typeof(Enum))
                {
                    if (!typeof(T).IsValueType && !((object)newValue is null))
                        underlyingType = newValue.GetType();
                    else
                    {
                        source.intValue = default;
                        return;
                    }
                }
                else
                    underlyingType = Enum.GetUnderlyingType(typeof(T));

                if (underlyingType == typeof(int))
                    source.intValue = (int)(object)newValue;
                else if (underlyingType == typeof(short))
                    source.intValue = (short)(object)newValue;
                else if (underlyingType == typeof(long))
                    source.longValue = (long)(object)newValue;
                else if (underlyingType == typeof(byte))
                    source.intValue = (byte)(object)newValue;
                else if (underlyingType == typeof(uint))
                    source.longValue = (uint)(object)newValue;
                else if (underlyingType == typeof(ushort))
                    source.intValue = (ushort)(object)newValue;
                else if (underlyingType == typeof(ulong))
                    source.longValue = (long)(ulong)(object)newValue;
                else if (underlyingType == typeof(sbyte))
                    source.intValue = (sbyte)(object)newValue;
                else if (underlyingType == typeof(char))
                    source.intValue = (char)(object)newValue;
                else if (underlyingType == typeof(float))
                    source.floatValue = (char)(object)newValue;
                else if (underlyingType == typeof(double))
                    source.doubleValue = (char)(object)newValue;

                source.SetTargetObject(newValue, false);
            }

            void Fallback()
            {
                switch (source.propertyType)
                {
                    case SerializedPropertyType.AnimationCurve:
                        source.animationCurveValue = (AnimationCurve)(object)newValue;
                        break;
                    case SerializedPropertyType.ArraySize:
                        source.arraySize = (int)(object)newValue;
                        break;
                    case SerializedPropertyType.FixedBufferSize:
                        Throw();
                        void Throw() => throw new InvalidOperationException("Can't mutate size of a Fixed Buffer.");
                        break;
                    case SerializedPropertyType.Integer:
                        // Technically, int and long are the only valid types,
                        // but we put the others to be sure.
                        switch (source.type)
                        {
                            case "long":
                                source.longValue = (long)(object)newValue;
                                break;
                            case "int":
                                source.intValue = (int)(object)newValue;
                                break;
                            case "short":
                                source.intValue = (short)(object)newValue;
                                break;
                            case "byte":
                                source.intValue = (byte)(object)newValue;
                                break;
                            case "ulong":
                                source.longValue = (long)(object)newValue;
                                break;
                            case "uint":
                                source.longValue = (uint)(object)newValue;
                                break;
                            case "ushort":
                                source.intValue = (ushort)(object)newValue;
                                break;
                            case "sbyte":
                                source.intValue = (sbyte)(object)newValue;
                                break;
                            default:
                                goto fallback;
                        }
                        break;
                    case SerializedPropertyType.Boolean:
                        source.boolValue = (bool)(object)newValue;
                        break;
                    case SerializedPropertyType.Bounds:
                        source.boundsValue = (Bounds)(object)newValue;
                        break;
                    case SerializedPropertyType.BoundsInt:
                        source.boundsIntValue = (BoundsInt)(object)newValue;
                        break;
                    case SerializedPropertyType.Character:
                        source.intValue = (char)(object)newValue;
                        break;
                    case SerializedPropertyType.Color:
                        source.colorValue = (Color)(object)newValue;
                        break;
                    case SerializedPropertyType.Float:
                        // Technically, float and double are the only valid types,
                        // but we put the others to be sure.
                        switch (source.type)
                        {
                            case "float":
                                source.floatValue = (float)(object)newValue;
                                break;
                            case "double":
                                source.doubleValue = (double)(object)newValue;
                                break;
                            case "decimal":
                                source.doubleValue = (double)(decimal)(object)newValue;
                                break;
                            default:
                                goto fallback;
                        }
                        break;
                    case SerializedPropertyType.LayerMask:
                        source.intValue = (LayerMask)(object)newValue;
                        break;
                    case SerializedPropertyType.Quaternion:
                        source.quaternionValue = (Quaternion)(object)newValue;
                        break;
                    case SerializedPropertyType.Rect:
                        source.rectValue = (Rect)(object)newValue;
                        break;
                    case SerializedPropertyType.RectInt:
                        source.rectIntValue = (RectInt)(object)newValue;
                        break;
                    case SerializedPropertyType.String:
                        source.stringValue = (string)(object)newValue;
                        break;
                    case SerializedPropertyType.Vector2:
                        source.vector2Value = (Vector2)(object)newValue;
                        break;
                    case SerializedPropertyType.Vector2Int:
                        source.vector2IntValue = (Vector2Int)(object)newValue;
                        break;
                    case SerializedPropertyType.Vector3:
                        source.vector3Value = (Vector3)(object)newValue;
                        break;
                    case SerializedPropertyType.Vector3Int:
                        source.vector3IntValue = (Vector3Int)(object)newValue;
                        break;
                    case SerializedPropertyType.Vector4:
                        source.vector4Value = (Vector4)(object)newValue;
                        break;
                    case SerializedPropertyType.Enum:
                        FromEnum();
                        break;
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Generic:
                        goto fallback;
                    case SerializedPropertyType.ObjectReference:
                        source.objectReferenceValue = (UnityObject)(object)newValue;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        source.exposedReferenceValue = (UnityObject)(object)newValue;
                        break;
                    default:
                    fallback:
                        source.SetTargetObject(newValue, false);
                        break;
                }
            }
        }

        /// <summary>
        /// Get the <see cref="MemberInfo"/> of <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose <see cref="MemberInfo"/> will be get.</param>
        /// <returns><see cref="MemberInfo"/> of <paramref name="source"/>.</returns>
        public static MemberInfo GetMemberInfo(this SerializedProperty source)
        {
            List<SerializedPropertyPathNode> nodes_ = Interlocked.Exchange(ref nodes, null) ?? new List<SerializedPropertyPathNode>();
            source.GetPropertyNodes<Toggle.Yes>(nodes_, 0);

            int i = nodes_.Count - 2;
            SerializedPropertyPathNode node = nodes_[i];
            while (node.MemberInfo is null)
                node = nodes_[--i];
            nodes_.Clear();
            nodes = nodes_;

            return node.MemberInfo;
        }

        /// <summary>
        /// Try get the <see cref="MemberInfo"/> of <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose <see cref="FieldInfo"/> will be get.</param>
        /// <param name="memberInfo"><see cref="FieldInfo"/> of <paramref name="source"/> if returns <see langword="true"/>.</param>
        /// <returns>If <see langword="true"/>, <paramref name="target"/> was got successfully.</returns>
        public static bool TryGetMemberInfo(this SerializedProperty source, out MemberInfo memberInfo)
        {
            List<SerializedPropertyPathNode> nodes_ = Interlocked.Exchange(ref nodes, null) ?? new List<SerializedPropertyPathNode>();
            if (!source.GetPropertyNodes<Toggle.No>(nodes_, 0))
            {
                nodes_.Clear();
                nodes = nodes_;
                memberInfo = null;
                return false;
            }

            int i = nodes_.Count - 2;
            SerializedPropertyPathNode node = nodes_[i];
            while (node.MemberInfo is null)
                node = nodes_[--i];
            nodes_.Clear();
            nodes = nodes_;

            memberInfo = node.MemberInfo;
            return true;
        }

        /// <summary>
        /// Get the <see cref="Type"/> of the property <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose <see cref="Type"/> will be get.</param>
        /// <returns><see cref="Type"/> of <paramref name="source"/>.</returns>
        public static Type GetPropertyType(this SerializedProperty source)
        {
            // TODO: Some checks may be redundant or unnecesary.
            string typeName = default;
            Type defaultType = default;
            Type type;
            switch (source.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    type = typeof(AnimationCurve);
                    goto done;
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.FixedBufferSize:
                    type = typeof(int);
                    goto done;
                case SerializedPropertyType.Integer:
                    // Technically, int and long are the only valid types,
                    // but we put the others to be sure.
                    switch (source.type)
                    {
                        case "long":
                            type = typeof(long);
                            goto done;
                        case "int":
                            type = typeof(int);
                            goto done;
                        case "short":
                            type = typeof(short);
                            goto done;
                        case "byte":
                            type = typeof(byte);
                            goto done;
                        case "ulong":
                            type = typeof(ulong);
                            goto done;
                        case "uint":
                            type = typeof(uint);
                            goto done;
                        case "ushort":
                            type = typeof(ushort);
                            goto done;
                        case "sbyte":
                            type = typeof(sbyte);
                            goto done;
                        default:
                            defaultType = typeof(int);
                            goto fallback;
                    }
                case SerializedPropertyType.Boolean:
                    type = typeof(bool);
                    goto done;
                case SerializedPropertyType.Bounds:
                    type = typeof(Bounds);
                    goto done;
                case SerializedPropertyType.BoundsInt:
                    type = typeof(BoundsInt);
                    goto done;
                case SerializedPropertyType.Character:
                    type = typeof(char);
                    goto done;
                case SerializedPropertyType.Color:
                    type = typeof(Color);
                    goto done;
                case SerializedPropertyType.Float:
                    // Technically, float and double are the only valid types,
                    // but we put the others to be sure.
                    switch (source.type)
                    {
                        case "float":
                            type = typeof(float);
                            goto done;
                        case "double":
                            type = typeof(double);
                            goto done;
                        case "decimal":
                            type = typeof(decimal);
                            goto done;
                        default:
                            goto fallback;
                    }
                case SerializedPropertyType.Gradient:
                    type = typeof(Gradient);
                    goto done;
                case SerializedPropertyType.LayerMask:
                    type = typeof(LayerMask);
                    goto done;
                case SerializedPropertyType.Quaternion:
                    type = typeof(Quaternion);
                    goto done;
                case SerializedPropertyType.Rect:
                    type = typeof(Rect);
                    goto done;
                case SerializedPropertyType.RectInt:
                    type = typeof(RectInt);
                    goto done;
                case SerializedPropertyType.String:
                    type = typeof(string);
                    goto done;
                case SerializedPropertyType.Vector2:
                    type = typeof(Vector2);
                    goto done;
                case SerializedPropertyType.Vector2Int:
                    type = typeof(Vector2Int);
                    goto done;
                case SerializedPropertyType.Vector3:
                    type = typeof(Vector3);
                    goto done;
                case SerializedPropertyType.Vector3Int:
                    type = typeof(Vector3Int);
                    goto done;
                case SerializedPropertyType.Vector4:
                    type = typeof(Vector4);
                    goto done;
#if UNITY_2021_1_OR_NEWER
                case SerializedPropertyType.Hash128:
                    type = typeof(Hash128);
                    goto done;
#endif
                case SerializedPropertyType.Generic:
                    // We don't use source.type in order to extract the type since it doesn't specify to which assembly the type belongs,
                    // which means that it would produce wrong results if two assemblies uses the same type name.
                    defaultType = typeof(object);
                    goto fallback;
                case SerializedPropertyType.Enum:
                    defaultType = typeof(Enum);
                    goto fallback;
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    defaultType = typeof(UnityObject);
                    goto fallback;
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    typeName = source.managedReferenceFieldTypename;
                    defaultType = typeof(object);
                    task.EnsureExecute();
                    if (Types.TryGetValue(typeName, out type))
                        goto done;
                    goto fallback;
#endif
                default:
                    defaultType = typeof(object);
                    goto fallback;
            }

        done:
            if (source.isArray)
            {
                defaultType = type.MakeArrayType();
                goto fallback;
            }
            else
                goto end;

            fallback:
            type = Fallback();

        end:
            return type;

            Type Fallback()
            {
                List<SerializedPropertyPathNode> nodes_ = Interlocked.Exchange(ref nodes, null) ?? new List<SerializedPropertyPathNode>();
                if (!(defaultType is null ? source.GetPropertyNodes<Toggle.Yes>(nodes_, 0) : source.GetPropertyNodes<Toggle.No>(nodes_, 0)))
                {
                    nodes_.Clear();
                    nodes = nodes_;
                    return defaultType;
                }

                SerializedPropertyPathNode node = nodes_[nodes_.Count - 2];
                Type type_ = null;
                switch (node.MemberInfo)
                {
                    case FieldInfo fieldInfo:
                        type_ = fieldInfo.FieldType;
                        break;
                    case PropertyInfo propertyInfo:
                        type_ = propertyInfo.PropertyType;
                        break;
                    case null:
                        Type type__ = node.Object.GetType();
                        if (!type__.IsArrayOrList(out type_))
                        {
                            foreach (Type @interface in type__.GetInterfaces())
                            {
                                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                {
                                    type_ = @interface.GenericTypeArguments[0];
                                    goto outerBreak;
                                }
                            }

                            if (typeof(IEnumerable).IsAssignableFrom(type__))
                            {
                                type_ = typeof(object);
                                break;
                            }

                            Debug.Assert(false, "Impossible state.");
                        outerBreak:;
                        }
                        break;
                    default:
                        Debug.Assert(false, "Impossible state.");
                        break;
                }

                nodes_.Clear();
                nodes = nodes_;
                return type_;
            }
        }
    }
}
