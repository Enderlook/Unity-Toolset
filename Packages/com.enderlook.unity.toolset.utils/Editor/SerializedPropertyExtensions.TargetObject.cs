using Enderlook.Reflection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    public static partial class SerializedPropertyExtensions
    {
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly List<SerializedPropertyPathNode> nodes = new List<SerializedPropertyPathNode>();
        private static readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

        /// <summary>
        /// Gets the property nodes hierarchy of <paramref name="source"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="nodes">Hierarchy traveled to get the property nodes.</param>
        /// <param name="count">Depth in the hierarchy to travel.</param>
        /// <param name="throwIfError">Whenever it should throw if there is an error.</param>
        /// <returns>If <see langword="true"/> <paramref name="target"/> was found. Otherwise <paramref name="target"/> contains an undefined value.</returns>
        private static bool GetPropertyNodes(this SerializedProperty source, List<SerializedPropertyPathNode> nodes, int count, bool throwIfError)
        {
            if (source == null)
            {
                if (throwIfError) ThrowSourceIsNull();
                goto isFalse;
            }

            if (nodes is null)
            {
                if (throwIfError) ThrowNodesIsNull();
                goto isFalse;
            }

            nodes.Clear();

            string path = source.propertyPath.Replace(".Array.data[", "[");
            string[] pathSections = path.Split(Helper.DOT_SEPARATOR);
            int total = pathSections.Length;

            if (count > total)
            {
                if (throwIfError) ThrowCountMustBeLowerThanTotal();
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
                if (throwIfError) ThrowTargetObjectIsNull();
                goto isFalse;
            }

            for (int i = 0; i < pathSections.Length; i++)
            {
                string element = pathSections[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = int.Parse(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                    if (!SetObj(elementName, out target, out MemberInfo memberInfo)) goto isFalse;

                    node.MemberInfo = memberInfo;
                    nodes.Add(node);
                    node.Object = target;
                    node.path = elementName;

                    if (target is null)
                    {
                        if (throwIfError) ThrowArrayDataIsNull();
                        goto isFalse;
                    }

                    void ThrowArrayDataIsNull()
                        => throw new ArgumentException($"source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[")}.{elementName}.Array.data");

                    if (target is IList list)
                    {
                        if (list.Count > index)
                        {
                            target = list[index];
                            goto next;
                        }
                        else if (throwIfError) ThrowIndexMustBeLowerThanArraySize();
                        else goto isFalse;
                    }

                    void ThrowIndexMustBeLowerThanArraySize()
                    {
                        string subPath = string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[");
                        string subPathPlusOne = string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[");
                        throw new ArgumentException($"source.serializedObject.targetObject.{subPathPlusOne}", $"Index {index} at 'source.serializedObject.targetObject.{subPathPlusOne}' must be lower than 'source.serializedObject.targetObject.{subPath}{(string.IsNullOrEmpty(subPath) ? "" : ".")}{elementName}.Array.arraySize' ({list.Count})");
                    }

                    if (target is IEnumerable enumerable)
                    {
                        IEnumerator enumerator = enumerable.GetEnumerator();

                        for (int j = 0; j <= index; j++)
                        {
                            if (!enumerator.MoveNext())
                            {
                                if (throwIfError) ThrowEnumerableExhausted();
                                nodes.Clear();
                                return false;
                            }

                            void ThrowEnumerableExhausted()
                            {
                                string subPath = string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[");
                                string subPathPlusOne = string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[");
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
                else if (SetObj(element, out target, out MemberInfo memberInfo))
                {
                    node.MemberInfo = memberInfo;
                    nodes.Add(node);
                    node.Object = target;
                    node.path = element;
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
                    if (throwIfError) ThrowTargetNull();
                    goto isFalse;

                    void ThrowTargetNull()
                        => throw new ArgumentException($"source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[")}");
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

                        PropertyInfo propertyInfo = type.GetProperty(name, bindingFlags);
                        if (!(propertyInfo is null))
                        {
                            target__ = propertyInfo.GetValue(target, null);
                            memberInfo = propertyInfo;
                            break;
                        }

                        type = type.BaseType;

                        if (type is null)
                        {
                            if (throwIfError) ThrowMemberNotFound();
                            target__ = default;
                            memberInfo = default;
                            return false;
                        }
                    }

                    return true;

                    void ThrowMemberNotFound()
                    {
                        string subPath = string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[");
                        throw new InvalidOperationException($"From path 'source.serializedObject.targetObject.{source.propertyPath}', member '{name}' (at 'source.serializedObject.targetObject.{subPath}{(string.IsNullOrEmpty(subPath) ? "" : ".")}{name}') was not found.");
                    }
                }
            }

            Debug.Assert(false, "Impossible state.");
            isFalse:
            nodes.Clear();
            return false;

            void ThrowSourceIsNull()
                => throw new ArgumentNullException(nameof(source));

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
            source.GetPropertyNodes(nodes, last, false);
            object @object = nodes[nodes.Count - 1].Object;
            nodes.Clear();
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
            if (source.GetPropertyNodes(nodes, last, true))
            {
                target = nodes[nodes.Count - 1].Object;
                nodes.Clear();
                return true;
            }
            target = default;
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
            // Insert JIT optimizable fast paths to expected values:

            SerializedPropertyType propertyType = source.propertyType;

            if (!typeof(T).IsValueType)
            {
                switch (propertyType)
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
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Generic:
                        return (T)source.GetTargetObject();
                }
            }

            if (typeof(T) == typeof(bool) && propertyType == SerializedPropertyType.Boolean)
                return (T)(object)source.boolValue;
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
                }
            }
            if (typeof(T) == typeof(long) && propertyType == SerializedPropertyType.Integer && source.type == "long")
                return (T)(object)source.longValue;
            if (typeof(T) == typeof(short) && propertyType == SerializedPropertyType.Integer && source.type == "short")
                return (T)(object)(short)source.intValue;
            if (typeof(T) == typeof(byte) && propertyType == SerializedPropertyType.Integer && source.type == "byte")
                return (T)(object)(byte)source.intValue;
            if (typeof(T) == typeof(uint) && propertyType == SerializedPropertyType.Integer && source.type == "uint")
                return (T)(object)(uint)source.longValue;
            if (typeof(T) == typeof(ulong) && propertyType == SerializedPropertyType.Integer && source.type == "ulong")
                return (T)(object)(ulong)source.longValue;
            if (typeof(T) == typeof(ushort) && propertyType == SerializedPropertyType.Integer && source.type == "ushort")
                return (T)(object)(ushort)source.intValue;
            if (typeof(T) == typeof(sbyte) && propertyType == SerializedPropertyType.Integer && source.type == "sbyte")
                return (T)(object)(sbyte)source.intValue;
            if (typeof(T) == typeof(float) && propertyType == SerializedPropertyType.Float && source.type == "float")
                return (T)(object)source.floatValue;
            if (typeof(T) == typeof(double) && propertyType == SerializedPropertyType.Float && source.type == "double")
                return (T)(object)source.doubleValue;
            if (typeof(T) == typeof(char) && propertyType == SerializedPropertyType.Character)
                return (T)(object)(char)source.intValue;
            if (typeof(T) == typeof(LayerMask) && propertyType == SerializedPropertyType.LayerMask)
                return (T)(object)(LayerMask)source.intValue;
            if (typeof(T) == typeof(Bounds) && propertyType == SerializedPropertyType.Bounds)
                return (T)(object)source.boundsValue;
            if (typeof(T) == typeof(BoundsInt) && propertyType == SerializedPropertyType.BoundsInt)
                return (T)(object)source.boundsIntValue;
            if (typeof(T) == typeof(Color) && propertyType == SerializedPropertyType.Color)
                return (T)(object)source.colorValue;
            if (typeof(T) == typeof(Quaternion) && propertyType == SerializedPropertyType.Quaternion)
                return (T)(object)source.quaternionValue;
            if (typeof(T) == typeof(Rect) && propertyType == SerializedPropertyType.Rect)
                return (T)(object)source.rectValue;
            if (typeof(T) == typeof(RectInt) && propertyType == SerializedPropertyType.RectInt)
                return (T)(object)source.rectIntValue;
            if (typeof(T) == typeof(Vector2) && propertyType == SerializedPropertyType.Vector2)
                return (T)(object)source.vector2Value;
            if (typeof(T) == typeof(Vector2Int) && propertyType == SerializedPropertyType.Vector2Int)
                return (T)(object)source.vector2IntValue;
            if (typeof(T) == typeof(Vector3) && propertyType == SerializedPropertyType.Vector3)
                return (T)(object)source.vector3Value;
            if (typeof(T) == typeof(Vector3Int) && propertyType == SerializedPropertyType.Vector3Int)
                return (T)(object)source.vector3IntValue;
            if (typeof(T) == typeof(Vector4) && propertyType == SerializedPropertyType.Vector4)
                return (T)(object)source.vector4Value;
            if (typeof(T).IsValueType && typeof(T) == typeof(Enum) && propertyType == SerializedPropertyType.Enum)
                return FromEnum();

            // Fallback to slower method since the type is not expected.
            return SlowPath();

            T FromEnum()
            {
                if (typeof(T) == typeof(Enum))
                    return (T)GetTargetObject(source);

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
                return (T)Enum.ToObject(typeof(T), GetTargetObject(source));
            }

            T SlowPath()
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
                    {
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
                        switch (source.type)
                        {
                            case "float":
                                return (T)(object)source.floatValue;
                            case "double":
                                return (T)(object)source.doubleValue;
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
            if (source.GetPropertyNodes(nodes, 0, true))
            {
                int i = nodes.Count - 2;
                SerializedPropertyPathNode node = nodes[i];
                if (!Set(node, newTarget))
                    goto error;

                while (i > 0 && node.Object.GetType().IsValueType)
                {
                    node = nodes[i];
                    SerializedPropertyPathNode previousNode = nodes[--i];
                    if (!Set(previousNode, node.Object))
                        goto error;
                    node = previousNode;
                }

                bool Set<U>(SerializedPropertyPathNode previousNode, U newValue_)
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
                            if (previousNode.Object is U[] array)
                            {
                                if (previousNode.Index < array.Length)
                                {
                                    array[previousNode.Index] = newValue_;
                                    break;
                                }
                                goto indexError;
                            }

                            // Check if type implement IList<U>, this is only useful for the first call of this method.
                            if (previousNode.Object is IList<U> list)
                            {
                                if (previousNode.Index < list.Count)
                                {
                                    list[previousNode.Index] = newValue_;
                                    break;
                                }
                                goto indexError;
                            }

                            if (previousNode.Object is IList list_)
                            {
                                if (previousNode.Index < list_.Count)
                                {
                                    list_[previousNode.Index] = newValue_;
                                    break;
                                }
                                goto indexError;
                            }

                            // TODO: Support for IList<> based of runtime type of newValue_ may be required.

                            if (notThrow) goto error_;
                            ThrowTypeNotSupported();

                            void ThrowTypeNotSupported() => throw new NotSupportedException($"Error while assigning value to '{string.Join(".", nodes.Take(i + 1).Select(e => e.path ?? $"Array.data[{e.Index}]"))}' from path '{string.Join(".", nodes.Select(e => e.path ?? $"Array.data[{e.Index}]"))}'. Can only mutate fields, properties or types that implement {nameof(IList)}.");

                            indexError:
                            if (notThrow) goto error_;
                            ThrowIndexMustBeLowerThanArraySize();

                            void ThrowIndexMustBeLowerThanArraySize()
                                => throw new NotSupportedException($"Error while assigning value to '{string.Join(".", nodes.Take(i + 1).Select(e => e.path ?? $"Array.data[{e.Index}]"))}' from path '{string.Join(".", nodes.Select(e => e.path ?? $"Array.data[{e.Index}]"))}'. Index {previousNode.Index} must be lwoer than '{string.Join(".", nodes.Take(i).Select(e => e.path ?? $"Array.data[{e.Index}]"))}.Array.arraySize' ({((IList)previousNode.Object).Count}).");

                            break;
                        }
                        default:
                            Debug.Assert(false, "Impossible state.");
                            break;
                    }
                    return true;
                    error_:
                    return false;
                }
            }
            error:
            nodes.Clear();
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
            // Insert JIT optimizable fast paths to expected values:

            SerializedPropertyType propertyType = source.propertyType;

            if (!typeof(T).IsValueType)
            {
                switch (propertyType)
                {
                    case SerializedPropertyType.AnimationCurve:
                        source.animationCurveValue = (AnimationCurve)(object)newValue;
                        return;
                    case SerializedPropertyType.String:
                        source.stringValue = (string)(object)newValue;
                        return;
                    case SerializedPropertyType.Enum:
                        FromEnum();
                        return;
                    case SerializedPropertyType.ObjectReference:
                        source.objectReferenceValue = (UnityEngine.Object)(object)newValue;
                        return;
                    case SerializedPropertyType.ExposedReference:
                        source.exposedReferenceValue = (UnityEngine.Object)(object)newValue;
                        return;
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Generic:
                        source.SetTargetObject(newValue, false);
                        return;
                    default:
                        goto slow;
                }
            }
            else if (typeof(T) == typeof(bool) && propertyType == SerializedPropertyType.Boolean)
                source.boolValue = (bool)(object)newValue;
            else if (typeof(T) == typeof(int))
            {
                switch (propertyType)
                {
                    case SerializedPropertyType.ArraySize:
                        source.arraySize = (int)(object)newValue;
                        return;
                    case SerializedPropertyType.Integer when source.type == "int":
                        source.intValue = (int)(object)newValue;
                        return;
                    default:
                        goto slow;
                }
            }
            else if (typeof(T) == typeof(long) && propertyType == SerializedPropertyType.Integer && source.type == "long")
                source.longValue = (long)(object)newValue;
            else if (typeof(T) == typeof(short) && propertyType == SerializedPropertyType.Integer && source.type == "short")
                source.intValue = (short)(object)newValue;
            else if (typeof(T) == typeof(byte) && propertyType == SerializedPropertyType.Integer && source.type == "byte")
                source.intValue = (byte)(object)newValue;
            else if (typeof(T) == typeof(uint) && propertyType == SerializedPropertyType.Integer && source.type == "uint")
                source.longValue = (uint)(object)newValue;
            else if (typeof(T) == typeof(ulong) && propertyType == SerializedPropertyType.Integer && source.type == "ulong")
                source.longValue = (long)(ulong)(object)newValue;
            else if (typeof(T) == typeof(ushort) && propertyType == SerializedPropertyType.Integer && source.type == "ushort")
                source.intValue = (ushort)(object)newValue;
            else if (typeof(T) == typeof(sbyte) && propertyType == SerializedPropertyType.Integer && source.type == "sbyte")
                source.intValue = (sbyte)(object)newValue;
            else if (typeof(T) == typeof(float) && propertyType == SerializedPropertyType.Float && source.type == "float")
                source.floatValue = (float)(object)newValue;
            else if (typeof(T) == typeof(double) && propertyType == SerializedPropertyType.Float && source.type == "double")
                source.doubleValue = (double)(object)newValue;
            else if (typeof(T) == typeof(char) && propertyType == SerializedPropertyType.Character)
                source.intValue = (char)(object)newValue;
            else if (typeof(T) == typeof(LayerMask) && propertyType == SerializedPropertyType.LayerMask)
                source.intValue = (LayerMask)(object)newValue;
            else if (typeof(T) == typeof(Bounds) && propertyType == SerializedPropertyType.Bounds)
                source.boundsValue = (Bounds)(object)newValue;
            else if (typeof(T) == typeof(BoundsInt) && propertyType == SerializedPropertyType.BoundsInt)
                source.boundsIntValue = (BoundsInt)(object)newValue;
            else if (typeof(T) == typeof(Color) && propertyType == SerializedPropertyType.Color)
                source.colorValue = (Color)(object)newValue;
            else if (typeof(T) == typeof(Quaternion) && propertyType == SerializedPropertyType.Quaternion)
                source.quaternionValue = (Quaternion)(object)newValue;
            else if (typeof(T) == typeof(Rect) && propertyType == SerializedPropertyType.Rect)
                source.rectValue = (Rect)(object)newValue;
            else if (typeof(T) == typeof(RectInt) && propertyType == SerializedPropertyType.RectInt)
                source.rectIntValue = (RectInt)(object)newValue;
            else if (typeof(T) == typeof(Vector2) && propertyType == SerializedPropertyType.Vector2)
                source.vector2Value = (Vector2)(object)newValue;
            else if (typeof(T) == typeof(Vector2Int) && propertyType == SerializedPropertyType.Vector2Int)
                source.vector2IntValue = (Vector2Int)(object)newValue;
            else if (typeof(T) == typeof(Vector3) && propertyType == SerializedPropertyType.Vector3)
                source.vector3Value = (Vector3)(object)newValue;
            else if (typeof(T) == typeof(Vector3Int) && propertyType == SerializedPropertyType.Vector3Int)
                source.vector3IntValue = (Vector3Int)(object)newValue;
            else if (typeof(T) == typeof(Vector4) && propertyType == SerializedPropertyType.Vector4)
                source.vector4Value = (Vector4)(object)newValue;
            else if (typeof(T).IsValueType && typeof(T) == typeof(Enum) && propertyType == SerializedPropertyType.Enum)
                FromEnum();
            else
                goto slow;

            return;
            slow:
            // Fallback to slower method since the type is not expected.
            SlowPath();

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

                source.SetTargetObject(newValue, false);
            }

            void SlowPath()
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
                        throw new InvalidOperationException("Can't mutate size of a Fixed Buffer.");
                    case SerializedPropertyType.Integer:
                    {
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
                    }
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
                        switch (source.type)
                        {
                            case "float":
                                source.floatValue = (float)(object)newValue;
                                break;
                            case "double":
                                source.doubleValue = (double)(object)newValue;
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
                        source.objectReferenceValue = (UnityEngine.Object)(object)newValue;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        source.exposedReferenceValue = (UnityEngine.Object)(object)newValue;
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
        /// <param name="source"><see cref="SerializedProperty"/> whose <see cref="FieldInfo"/> will be get.</param>
        /// <returns><see cref="FieldInfo"/> of <paramref name="source"/>.</returns>
        public static MemberInfo GetMemberInfo(this SerializedProperty source)
        {
            source.GetPropertyNodes(nodes, 0, true);

            int i = nodes.Count - 2;
            SerializedPropertyPathNode node = nodes[i];
            while (node.MemberInfo is null)
                node = nodes[--i];
            nodes.Clear();

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
            if (!source.GetPropertyNodes(nodes, 0, false))
            {
                memberInfo = null;
                return false;
            }

            int i = nodes.Count - 2;
            SerializedPropertyPathNode node = nodes[i];
            while (node.MemberInfo is null)
                node = nodes[--i];
            nodes.Clear();

            memberInfo= node.MemberInfo;
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
                {
                    typeName = source.type;
                    switch (typeName)
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
                            goto find;
                    }
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
                {
                    typeName = source.type;
                    switch (typeName)
                    {
                        case "float":
                            type = typeof(float);
                            goto done;
                        case "double":
                            type = typeof(double);
                            goto done;
                        default:
                            defaultType = typeof(float);
                            goto find;
                    }
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
                case SerializedPropertyType.Generic:
                {
                    typeName = source.type;
                    defaultType = typeof(object);
                    goto find;
                }
                case SerializedPropertyType.Enum:
                    defaultType = typeof(Enum);
                    goto fallback;
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                    defaultType = typeof(UnityEngine.Object);
                    goto fallback;
                default:
                    defaultType = typeof(object);
                    goto fallback;
            }

            find:
            {
                if (types.TryGetValue(typeName, out Type type_))
                    type = type_;
                else
                    type = FindType();
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

            Type FindType()
            {
                bool isArray = typeName.EndsWith("[]");
                if (isArray)
                    typeName = typeName.Substring(0, typeName.Length - "[]".Length);

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type type_ in assembly.GetTypes())
                    {
                        if (type_.Name == typeName)
                        {
                            Type type__ = type_;
                            if (isArray)
                                type__ = type__.MakeArrayType();
                            types.Add(source.type, type__);
                            return type__;
                        }
                    }
                }

                return Fallback();
            }

            Type Fallback()
            {
                if (!source.GetPropertyNodes(nodes, 0, !(defaultType is null)))
                    return defaultType;

                SerializedPropertyPathNode node = nodes[nodes.Count - 2];
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
                        if (type__.IsArrayOrList())
                            type_ = type__.GetElementTypeOfArrayOrList();
                        else
                        {
                            type_ = null;
                            bool found = false;
                            foreach (Type @interface in type__.GetInterfaces())
                            {
                                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                {
                                    type_ = @interface.GenericTypeArguments[0];
                                    found = true;
                                    break;
                                }
                                else if (@interface == typeof(IEnumerable))
                                {
                                    type_ = typeof(object);
                                    found = true;
                                }
                            }
                            if (found is true)
                                break;
                            Debug.Assert(false, "Impossible state.");
                        }
                        break;
                    default:
                        Debug.Assert(false, "Impossible state.");
                        break;
                }

                nodes.Clear();
                return type_;
            }
        }
    }
}
