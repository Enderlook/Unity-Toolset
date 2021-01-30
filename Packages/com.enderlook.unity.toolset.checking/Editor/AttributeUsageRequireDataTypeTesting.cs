using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking
{
    internal static class AttributeUsageRequireDataTypeTesting
    {
        private static Dictionary<Type, (AttributeTargets targets, Action<Type, string> checker)> checkers = new Dictionary<Type, (AttributeTargets targets, Action<Type, string> checker)>();

        [ExecuteOnEachTypeWhenScriptsReloads(ExecuteOnEachTypeWhenScriptsReloads.TypeFlags.IsNonEnum, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributesAndTypes(Type type)
        {
            if (type.IsSubclassOf(typeof(Attribute)) && type.GetCustomAttribute<AttributeUsageRequireDataTypeAttribute>(true) is AttributeUsageRequireDataTypeAttribute attribute)
            {
                AttributeUsageAttribute attributeUsageAttribute = type.GetCustomAttribute<AttributeUsageAttribute>();
                checkers.Add(type, (attributeUsageAttribute?.ValidOn ?? AttributeTargets.All, (checkType, checkName) => new AttributeUsageRequireDataTypeHelper(attribute).CheckAllowance(checkType, checkName, type.Name)));
            }
        }

        private static void CheckSomething(IEnumerable<Attribute> attributes, HashSet<Type> toIgnore, Type type, AttributeTargets checkIf, string message)
        {
            foreach (Attribute attribute in attributes)
            {
                Type attributeType = attribute.GetType();
                if (!toIgnore.Contains(attributeType)
                    && checkers.TryGetValue(attributeType, out (AttributeTargets targets, Action<Type, string> checker) value)
                    && (value.targets & checkIf) != 0
                    ) // Check if has the proper flag
                    value.checker(type, message);
            }
        }

        private static void CheckSomething(MemberInfo memberInfo, Type type, string memberType, AttributeTargets checkIf) => CheckSomething(memberInfo.GetCustomAttributes(), new HashSet<Type>(memberInfo.GetAttributeTypesThatShouldBeIgnored()), type, checkIf, $"{memberType} {memberInfo.Name} in {memberInfo.DeclaringType.Name} class");

        [ExecuteOnEachTypeWhenScriptsReloads(ExecuteOnEachTypeWhenScriptsReloads.TypeFlags.IsNonEnum, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckClasses(Type type)
        {
            HashSet<Type> toIgnore = new HashSet<Type>(type.GetAttributeTypesThatShouldBeIgnored());
            if (!toIgnore.Contains(typeof(AttributeUsageMethodAttribute)))
                CheckSomething(type.GetCustomAttributes(), toIgnore, type, AttributeTargets.Class, $"Class {type.Name}");
        }

        [ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization.EitherSerializableOrNotByUnity, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckFields(FieldInfo fieldInfo) => CheckSomething(fieldInfo, fieldInfo.FieldType, "Field", AttributeTargets.Field);

        [ExecuteOnEachPropertyOfEachTypeWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckProperties(PropertyInfo propertyInfo) => CheckSomething(propertyInfo, propertyInfo.PropertyType, "Property", AttributeTargets.Property);

        [ExecuteOnEachMethodOfEachTypeWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckMethodReturns(MethodInfo methodInfo) => CheckSomething(methodInfo, methodInfo.ReturnType, "Method return", AttributeTargets.Method);
    }
}
