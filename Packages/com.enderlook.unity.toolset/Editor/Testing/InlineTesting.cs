using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class InlineTesting
    {
        [ExecuteOnEachFieldOfEachTypeWhenCheck(FieldSerialization.SerializableByUnity, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttribute<InlineAttribute>() is InlineAttribute attribute && !fieldInfo.CheckIfShouldBeIgnored(typeof(InlineAttribute)))
            {
                Type type = fieldInfo.DeclaringType;
                if (type.IsPrimitive || (type.IsArrayOrList(out Type elementType) && elementType.IsPrimitive))
                    Debug.LogError($"Attribute {nameof(InlineAttribute)} on field '{fieldInfo.Name}' in type '{fieldInfo.DeclaringType}' can't be used on fields whose field type is primitive or is an array or lists of primitive types.");
            }
        }
    }
}