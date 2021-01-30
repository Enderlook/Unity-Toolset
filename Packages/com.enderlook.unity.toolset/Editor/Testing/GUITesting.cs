using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class GUITesting
    {
        private static readonly Dictionary<Type, List<GUIAttribute>> typesAndAttributes = new Dictionary<Type, List<GUIAttribute>>();

        [ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization.SerializableByUnity, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (fieldInfo.CheckIfShouldBeIgnored(typeof(GUIAttribute)))
                return;
            if (fieldInfo.GetCustomAttribute<GUIAttribute>() is GUIAttribute attribute)
            {
                Type type = fieldInfo.DeclaringType;
                if (typesAndAttributes.TryGetValue(type, out List<GUIAttribute> list))
                    list.Add(attribute);
                else
                    typesAndAttributes.Add(type, new List<GUIAttribute>() { attribute });
            }
        }

        [ExecuteWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            foreach (KeyValuePair<Type, List<GUIAttribute>> classToCheck in typesAndAttributes)
            {
                Type classType = classToCheck.Key;
                List<GUIAttribute> attributes = classToCheck.Value;

                HashSet<string> strings = new HashSet<string>(
                    attributes
                        .Where(e => e.nameMode == GUIAttribute.Mode.Reference)
                        .Select(e => e.name)
                        .Concat(
                            attributes
                                .Where(e => e.tooltipMode == GUIAttribute.Mode.Reference)
                                .Select(e => e.tooltip)
                        )
                        .Where(e => e != null)
                    );

                HashSet<string> members = new HashSet<string>(classType.FieldsPropertiesAndMethodsWithReturnTypeOf<string>());

                strings.ExceptWith(members);

                foreach (string field in strings)
                    Debug.LogError($"Type {classType} does not have a field, property (with Get Method) or method (with only optional or params pameters and with a return type other than void) of type {typeof(string)} named {field} necessary for attribute {nameof(GUIAttribute)}.");

                HashSet<string> stringOrGUIContent = new HashSet<string>(
                    attributes
                        .Select(e => e.guiContentOrReferenceName)
                        .Where(e => e != null)
                    );

                members.UnionWith(classType.FieldsPropertiesAndMethodsWithReturnTypeOf<GUIContent>());

                stringOrGUIContent.ExceptWith(members);
            }
        }
    }
}