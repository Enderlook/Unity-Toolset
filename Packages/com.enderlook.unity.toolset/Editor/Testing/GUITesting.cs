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
    internal static class GUITesting
    {
        private static readonly Dictionary<Type, List<(FieldInfo field, GUIAttribute attribute)>> typesAndAttributes = new Dictionary<Type, List<(FieldInfo field, GUIAttribute attribute)>>();

        [ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization.SerializableByUnity, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttribute<GUIAttribute>() is GUIAttribute attribute && !fieldInfo.CheckIfShouldBeIgnored(typeof(GUIAttribute)))
            {
                Type type = fieldInfo.DeclaringType;
                if (!typesAndAttributes.TryGetValue(type, out List<(FieldInfo field, GUIAttribute attribute)> list))
                    typesAndAttributes.Add(type, (list = new List<(FieldInfo field, GUIAttribute attribute)>()));
                list.Add((fieldInfo, attribute));
            }
        }

        [ExecuteWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            Stack<HashSet<string>> stack = new Stack<HashSet<string>>();
            Dictionary<string, HashSet<string>> strings = new Dictionary<string, HashSet<string>>();

            foreach (KeyValuePair<Type, List<(FieldInfo field, GUIAttribute attribute)>> classToCheck in typesAndAttributes)
            {
                for (int i = 0; i < classToCheck.Value.Count; i++)
                {
                    (FieldInfo field, GUIAttribute attribute) = classToCheck.Value[i];

                    string name = attribute.name;
                    if (attribute.nameMode == GUIAttribute.Mode.Reference && name != null)
                    {
                        if (!strings.TryGetValue(name, out HashSet<string> hashSet))
                        {
                            if (!stack.TryPop(out hashSet))
                                hashSet = new HashSet<string>();
                            strings.Add(name, hashSet);
                        }
                        hashSet.Add(field.Name);
                    }

                    string tooltip = attribute.tooltip;
                    if (attribute.tooltipMode == GUIAttribute.Mode.Reference && tooltip != null)
                    {
                        if (!strings.TryGetValue(tooltip, out HashSet<string> hashSet))
                        {
                            if (!stack.TryPop(out hashSet))
                                hashSet = new HashSet<string>();
                            strings.Add(tooltip, hashSet);
                        }
                        hashSet.Add(field.Name);
                    }
                }

                foreach (string member in classToCheck.Key.FieldsPropertiesAndMethodsWithReturnTypeOf<string>())
                    strings.Remove(member);

                foreach (KeyValuePair<string, HashSet<string>> kvp in strings)
                    foreach (string name in kvp.Value)
                        Debug.LogError($"Type {classToCheck.Key} does not have a field, property (with Get method) or method (with only optional or params pameters and with a return type other than void) of type {typeof(string)} named '{kvp.Key}' necessary for attribute {nameof(GUIAttribute)} on field named {name}.");

                foreach (HashSet<string> hashset in strings.Values)
                {
                    hashset.Clear();
                    stack.Push(hashset);
                }
                strings.Clear();

                for (int i = 0; i < classToCheck.Value.Count; i++)
                {
                    (FieldInfo field, GUIAttribute attribute) = classToCheck.Value[i];

                    string name = attribute.guiContentOrReferenceName;
                    if (name != null)
                    {
                        if (!strings.TryGetValue(name, out HashSet<string> hashSet))
                        {
                            if (!stack.TryPop(out hashSet))
                                hashSet = new HashSet<string>();
                            strings.Add(name, hashSet);
                        }
                        hashSet.Add(field.Name);
                    }
                }

                foreach (string member in classToCheck.Key.FieldsPropertiesAndMethodsWithReturnTypeOf<GUIContent>())
                    strings.Remove(member);

                foreach (KeyValuePair<string, HashSet<string>> kvp in strings)
                    foreach (string name in kvp.Value)
                        Debug.LogError($"Type {classToCheck.Key} does not have a field, property (with Get method) or method (with only optional or params pameters and with a return type other than void) of type {typeof(GUIContent)} named '{kvp.Key}' necessary for attribute {nameof(GUIAttribute)} on field named {name}.");

                foreach (HashSet<string> hashset in strings.Values)
                {
                    hashset.Clear();
                    stack.Push(hashset);
                }
                strings.Clear();
            }
        }
    }
}