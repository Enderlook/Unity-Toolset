using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class DrawVectorRelativeToTransformTesting
    {
        private static readonly List<(FieldInfo field, DrawVectorRelativeToTransformAttribute attribute)> fields = new List<(FieldInfo field, DrawVectorRelativeToTransformAttribute attribute)>();

        [ExecuteWhenCheck(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void Reset() => fields.Clear();

        [ExecuteOnEachFieldOfEachTypeWhenCheck(FieldSerialization.SerializableByUnity, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (!fieldInfo.CheckIfShouldBeIgnored(typeof(DrawVectorRelativeToTransformAttribute))
                && fieldInfo.GetCustomAttribute<DrawVectorRelativeToTransformAttribute>() is DrawVectorRelativeToTransformAttribute attribute)
                fields.Add((fieldInfo, attribute));
        }

        [ExecuteWhenCheck(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            // TODO: This could be optimized by deduplicating similar checkings.
            foreach ((FieldInfo field, DrawVectorRelativeToTransformAttribute attribute) tuple in fields)
            {
                (MemberInfo member, Type type) tuple2 = GetMemberAndType(tuple.field.DeclaringType, tuple.field.Name);
                if (tuple2.type is null)
                    Debug.LogError($"No field, property (with Get method), or method with no mandatory parameters of name '{tuple.attribute.reference}' for attribute '{nameof(DrawVectorRelativeToTransformAttribute)}' in field '{tuple.field.Name}' of type '{tuple.field.ReflectedType.Name}' was found.");
                else if (tuple2.type != typeof(Vector2)
                    && tuple2.type != typeof(Vector2Int)
                    && tuple2.type != typeof(Vector3)
                    && tuple2.type != typeof(Vector3Int)
                    && tuple2.type != typeof(Vector4)
                    && tuple2.type != typeof(GameObject)
                    && typeof(Transform).IsAssignableFrom(tuple2.type)
                    && typeof(Component).IsAssignableFrom(tuple2.type))
                {
                    string member;
                    switch (tuple2.member.MemberType)
                    {
                        case MemberTypes.Field:
                            member = "Field";
                            break;
                        case MemberTypes.Property:
                            member = "Property";
                            break;
                        case MemberTypes.Method:
                            member = "Method";
                            break;
                        default:
                            System.Diagnostics.Debug.Fail("Impossibl state.");
                            member = null;
                            break;
                    }
                    Debug.LogError($"{member} named {tuple.field.Name} doesn't have a type assignable to: {typeof(Vector2)}, {typeof(Vector2Int)}, {typeof(Vector3)}, {typeof(Vector3Int)}, {typeof(Vector4)}, {typeof(GameObject)}, {typeof(Transform)} or {typeof(Component)}. Is of type {tuple2.type}.");
                }
            }
        }

        private static (MemberInfo member, Type type) GetMemberAndType(Type originalType, string name)
        {
            Type type = originalType;
        start:
            foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (memberInfo.Name != name)
                    continue;

                if (memberInfo is FieldInfo fieldInfo)
                    return (memberInfo, fieldInfo.FieldType);

                if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                    return (memberInfo, propertyInfo.PropertyType);

                if (memberInfo is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void) && methodInfo.HasNoMandatoryParameters())
                    return (memberInfo, methodInfo.ReturnType);
            }

            type = type.BaseType;
            if (type == null)
                return default;
            goto start;
        }
    }
}
