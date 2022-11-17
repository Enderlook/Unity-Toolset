using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking
{
    internal static class AttributeUsageFieldMustBeSerializableByUnityTesting
    {
        private static readonly Dictionary<Type, bool> types = new Dictionary<Type, bool>();

        [ExecuteWhenCheck(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void Reset() => types.Clear();

        [ExecuteOnEachTypeWhenCheck(TypeFlags.IsNonEnum, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributesAndTypes(Type type)
        {
            if (type.IsSubclassOf(typeof(Attribute))
                && type.GetCustomAttribute<AttributeUsageFieldMustBeSerializableByUnityAttribute>(true) is AttributeUsageFieldMustBeSerializableByUnityAttribute attribute)
                types.Add(type, attribute.notSupportEnumerableFields);
        }

        [ExecuteOnEachFieldOfEachTypeWhenCheck(FieldSerialization.AnyField, 2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckFields(FieldInfo fieldInfo)
        {
            foreach (Attribute attribute in fieldInfo.GetCustomAttributes())
            {
                Type attributeType = attribute.GetType();
                if (types.TryGetValue(attributeType, out bool notSupportEnumerableFields)
                    && (!fieldInfo.CanBeSerializedByUnity() || (notSupportEnumerableFields && fieldInfo.FieldType.IsArrayOrList()))
                    && !fieldInfo.CheckIfShouldBeIgnored(attributeType))
                    Debug.LogError($"The attribute '{attribute.GetType().Name}' can only be used in fields that can be serialized by Unity{(notSupportEnumerableFields ? "and are not arrays nor lists" : "")}, but field '{fieldInfo.Name}' from class '{fieldInfo.DeclaringType.Name}' (type '{fieldInfo.FieldType}') don't match criteria.");
            }
        }
    }
}