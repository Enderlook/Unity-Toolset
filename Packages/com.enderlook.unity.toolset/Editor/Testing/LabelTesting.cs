using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class LabelTesting
    {
        private static readonly Dictionary<Type, List<(FieldInfo field, LabelAttribute attribute)>> typesAndAttributes = new Dictionary<Type, List<(FieldInfo field, LabelAttribute attribute)>>();

        [ExecuteWhenScriptsReloads(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void Reset() => typesAndAttributes.Clear();

        [ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization.SerializableByUnity, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttribute<LabelAttribute>() is LabelAttribute attribute && !fieldInfo.CheckIfShouldBeIgnored(typeof(LabelAttribute)))
            {
                Type type = fieldInfo.DeclaringType;
                if (!typesAndAttributes.TryGetValue(type, out List<(FieldInfo field, LabelAttribute attribute)> list))
                    typesAndAttributes.Add(type, list = new List<(FieldInfo field, LabelAttribute attribute)>());
                list.Add((fieldInfo, attribute));
            }
        }

        [ExecuteWhenScriptsReloads(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            HashSet<string> stringSet = new HashSet<string>();
            HashSet<string> guiContentSet = new HashSet<string>();

            foreach (KeyValuePair<Type, List<(FieldInfo field, LabelAttribute attribute)>> classToCheck in typesAndAttributes)
            {
                stringSet.Clear();
                stringSet.UnionWith(classToCheck.Key.FieldsPropertiesAndMethodsWithReturnTypeOf<string>());

                guiContentSet.Clear();
                guiContentSet.UnionWith(classToCheck.Key.FieldsPropertiesAndMethodsWithReturnTypeOf<GUIContent>());

                for (int i = 0; i < classToCheck.Value.Count; i++)
                {
                    (FieldInfo field, LabelAttribute attribute) = classToCheck.Value[i];

                    if (attribute.TooltipMode == LabelMode.ByReference && attribute.Tooltip is string tooltip)
                    {
                        if (!stringSet.Contains(tooltip))
                            Debug.LogError($"Type '{classToCheck.Key}' does not have a field, property (with Get method) or method (with only optional or params parameters) that returns '{typeof(string)}' named '{tooltip}' necessary for the parameter 'tooltip' of attribute '{nameof(LabelAttribute)}' on field named '{field.Name}'.");
                        if (attribute.DisplayNameMode == LabelMode.ByReference
                            && attribute.DisplayNameOrGuiContent is string displayName
                            && !stringSet.Contains(displayName))
                            Debug.LogError($"Type '{classToCheck.Key}' does not have a field, property (with Get method) or method (with only optional or params parameters) that returns '{typeof(string)}' named '{displayName}' necessary for the parameter 'displayName' of attribute '{nameof(LabelAttribute)}' on field named '{field.Name}'.");
                    }
                    else
                    {
                        if (attribute.DisplayNameMode == LabelMode.ByReference && attribute.DisplayNameOrGuiContent is string displayNameOrGuiContent)
                        {
                            if (!stringSet.Contains(displayNameOrGuiContent) && !guiContentSet.Contains(displayNameOrGuiContent))
                                Debug.LogError($"Type '{classToCheck.Key}' does not have a field, property (with Get method) or method (with only optional or params parameters) that returns '{typeof(string)}' or '{typeof(GUIContent)}' named '{displayNameOrGuiContent}' necessary for the parameter 'displayName' of attribute '{nameof(LabelAttribute)}' on field named '{field.Name}'.");
                        }
                    }
                }
            }
        }
    }
}