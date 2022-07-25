using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class RedirectToTesting
    {
        [ExecuteOnEachTypeWhenCheck(TypeFlags.IsNonEnum, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetTypes(Type type)
        {
            if (type.GetCustomAttribute<RedirectToAttribute>() is RedirectToAttribute attribute && !type.CheckIfShouldBeIgnored(typeof(RedirectToAttribute)))
            {
                FieldInfo fieldInfo = type.GetFieldExhaustive(attribute.RedirectFieldName, ExhaustiveBindingFlags.Instance);
                if (fieldInfo == null)
                    Debug.LogError($"Type '{type}' has attribute '{nameof(RedirectToAttribute)}', but doesn't have a field named '{attribute.RedirectFieldName}' as '{nameof(RedirectToAttribute.RedirectFieldName)}' requires nor its bases classes have it.");
                else if (!fieldInfo.CanBeSerializedByUnity() && !fieldInfo.CheckIfShouldBeIgnored(typeof(PropertyPopupAttribute)))
                {
                    bool r = fieldInfo.CanBeSerializedByUnity();
                    Debug.LogError($"Type '{type}' has attribute '{nameof(RedirectToAttribute)}' which uses the field '{fieldInfo.Name}' declared in '{fieldInfo.DeclaringType}' as '{nameof(RedirectToAttribute.RedirectFieldName)}', but it's not serializable by Unity.");
                }
            }
        }
    }
}
