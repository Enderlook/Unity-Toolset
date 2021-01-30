using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking
{
    internal static class AttributeUsageAccessibilityTesting
    {
        private static Dictionary<Type, Action<MemberInfo, string>> checkers = new Dictionary<Type, Action<MemberInfo, string>>();

        [ExecuteOnEachTypeWhenScriptsReloads(ExecuteOnEachTypeWhenScriptsReloads.TypeFlags.IsEitherEnumNonEnum, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributesAndTypes(Type type)
        {
            if (type.IsSubclassOf(typeof(Attribute))
                && type.GetCustomAttribute<AttributeUsageAccessibilityAttribute>(true) is AttributeUsageAccessibilityAttribute attribute)
                    checkers.Add(type, attribute.CheckAllowance);
        }

        [ExecuteOnEachMemberOfEachTypeWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckMethods(MemberInfo memberInfo)
        {
            foreach (Attribute attribute in memberInfo.GetCustomAttributes())
            {
                Type type = attribute.GetType();
                if (checkers.TryGetValue(type, out Action<MemberInfo, string> check)
                    && !memberInfo.CheckIfShouldBeIgnored(type))
                    check(memberInfo, $"Member {memberInfo.Name} in {memberInfo.DeclaringType.Name} class");
            }
        }
    }
}