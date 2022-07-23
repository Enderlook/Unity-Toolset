﻿using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class PropertyPopupTesting
    {
        private static readonly Dictionary<Type, PropertyPopupAttribute> typesAndAttributes = new Dictionary<Type, PropertyPopupAttribute>();

        private static readonly Dictionary<Type, List<FieldInfo>> typesAndFieldAttributes = new Dictionary<Type, List<FieldInfo>>();

        [ExecuteWhenCheckAttribute(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void Reset()
        {
            typesAndAttributes.Clear();
            typesAndFieldAttributes.Clear();
        }

        [ExecuteOnEachTypeWhenCheckAttribute(TypeFlags.IsNonEnum, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetTypes(Type type)
        {
            if (type.GetCustomAttribute<PropertyPopupAttribute>() is PropertyPopupAttribute attribute && !type.CheckIfShouldBeIgnored(typeof(PropertyPopupAttribute)))
            {
                typesAndAttributes.Add(type, attribute);
                FieldInfo fieldInfo = type.GetFieldExhaustive(attribute.ModeName, ExhaustiveBindingFlags.Instance);
                if (fieldInfo == null)
                    Debug.LogError($"Type '{type}' has attribute '{nameof(PropertyPopupAttribute)}', but doesn't have a field named '{attribute.ModeName}' as '{nameof(PropertyPopupAttribute.ModeName)}' requires nor its bases classes have it.");
                else if (!fieldInfo.CanBeSerializedByUnity() && !fieldInfo.CheckIfShouldBeIgnored(typeof(PropertyPopupAttribute)))
                    Debug.LogError($"Type '{type}' has attribute '{nameof(PropertyPopupAttribute)}' which uses the field '{fieldInfo.Name}' declared in '{fieldInfo.DeclaringType}' as mode field, but it's no serializable by Unity.");
            }
        }

        [ExecuteOnEachFieldOfEachTypeWhenCheckAttribute(FieldSerialization.SerializableByUnity, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttribute<PropertyPopupOptionAttribute>() is PropertyPopupOptionAttribute && !fieldInfo.CheckIfShouldBeIgnored(typeof(PropertyPopupOptionAttribute)))
            {
                Type type = fieldInfo.DeclaringType;
                if (!typesAndFieldAttributes.TryGetValue(type, out List<FieldInfo> list))
                    typesAndFieldAttributes.Add(type, list = new List<FieldInfo>());
                list.Add(fieldInfo);
            }
        }

        [ExecuteWhenCheckAttribute(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            foreach (KeyValuePair<Type, List<FieldInfo>> kv in typesAndFieldAttributes)
            {
                Type type = kv.Key;
                while (type != null)
                {
                    if (typesAndAttributes.ContainsKey(type))
                        goto next;
                    else
                        type = type.BaseType;
                }
                foreach (FieldInfo field in kv.Value)
                    Debug.LogError($"Type '{kv.Key}' nor its base classes have attribute '{nameof(PropertyPopupAttribute)}', but its field named '{field.Name}' has the attribute '{nameof(PropertyPopupOptionAttribute)}'.");

                next:
                foreach (FieldInfo fieldInfo in kv.Value)
                    if (!fieldInfo.CanBeSerializedByUnity())
                        Debug.LogError($"Type '{kv.Key}' has a field named '{fieldInfo.Name}' with the attribute '{nameof(PropertyPopupAttribute)}', but it's not serializable by unity.");
            }
        }
    }
}
