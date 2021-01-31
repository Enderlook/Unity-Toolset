using Enderlook.Reflection;
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
    internal static class ShowfIfTesting
    {
        private static readonly Dictionary<Type, List<ShowIfAttribute>> typesAndAttributes = new Dictionary<Type, List<ShowIfAttribute>>();

        [ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization.SerializableByUnity, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (fieldInfo.CheckIfShouldBeIgnored(typeof(ShowIfAttribute)))
                return;
            if (fieldInfo.GetCustomAttribute<ShowIfAttribute>() is ShowIfAttribute attribute)
            {
                Type type = fieldInfo.DeclaringType;
                if (typesAndAttributes.TryGetValue(type, out List<ShowIfAttribute> list))
                    list.Add(attribute);
                else
                    typesAndAttributes.Add(type, new List<ShowIfAttribute>() { attribute });
            }
        }

        [ExecuteWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            foreach (KeyValuePair<Type, List<ShowIfAttribute>> classToCheck in typesAndAttributes)
            {
                Type classType = classToCheck.Key;
                HashSet<string> confirmFields = new HashSet<string>(classToCheck.Value.Select(e => e.nameOfConditional));

                foreach ((string memberName, Type memberType) in classToCheck.Value.Select(e => (e.nameOfConditional, e.memberType)).Distinct())
                    if (classType.GetFirstMemberInfoInMatchReturn(memberName, memberType, true) is null)
                        Debug.LogError($"{classType} does not have a field, property (with Get Method) or method (without mandatory parameters and with return type) of type {memberType} named {memberName} necessary for attribute {nameof(ShowIfAttribute)}.");
            }
        }
    }
}
