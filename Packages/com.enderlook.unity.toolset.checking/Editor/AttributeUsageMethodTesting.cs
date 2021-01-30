using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking
{
    internal static class AttributeUsageMethodTesting
    {
        private static Dictionary<Type, Action<MethodInfo, string>> checkers = new Dictionary<Type, Action<MethodInfo, string>>();

        [ExecuteOnEachTypeWhenScriptsReloads(ExecuteOnEachTypeWhenScriptsReloads.TypeFlags.IsNonEnum, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributesAndTypes(Type type)
        {
            if (type.IsSubclassOf(typeof(Attribute))
                && type.GetCustomAttribute<AttributeUsageMethodAttribute>(true) is AttributeUsageMethodAttribute attribute)
            {
                checkers.Add(type, new AttributeUsageMethodHelper(attribute).CheckAllowance);
            }
        }

        [ExecuteOnEachMethodOfEachTypeWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckMethods(MethodInfo methodInfo)
        {
            foreach (Attribute attribute in methodInfo.GetCustomAttributes())
            {
                Type type = attribute.GetType();
                if (checkers.TryGetValue(attribute.GetType(), out Action<MethodInfo, string> check)
                    && !methodInfo.CheckIfShouldBeIgnored(type))
                    check(methodInfo, $"method {methodInfo.Name} in {methodInfo.DeclaringType.Name} class");
            }
        }
    }
}