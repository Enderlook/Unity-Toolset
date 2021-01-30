using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking
{
    // TODO: This should be internal
    public static class DoNotCheckAttributeTesting
    {
        private static string MUST_INHERIT_FROM_ATTRIBUTE = "Attribute " + nameof(DoNotCheckAttribute) + " can only have types that inherit from " + nameof(Attribute) + " in the field " + nameof(DoNotCheckAttribute.ignoreTypes) + ". The type {0} is not subclass of " + nameof(Attribute) + ". Found in {1}.";

        private static void CheckAttributes(DoNotCheckAttribute attribute, string foundIn)
        {
            foreach (Type type in attribute?.ignoreTypes ?? Enumerable.Empty<Type>())
                if (!type.IsSubclassOf(typeof(Attribute)))
                    Debug.LogError(string.Format(MUST_INHERIT_FROM_ATTRIBUTE, type, foundIn));
        }

        [ExecuteOnEachTypeWhenScriptsReloads(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributeFromMember(Type type) => CheckAttributes(type.GetCustomAttribute<DoNotCheckAttribute>(), $"type {type.Name}");

        [ExecuteOnEachMemberOfEachTypeWhenScriptsReloads(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributeFromMember(MemberInfo memberInfo) => CheckAttributes(memberInfo.GetCustomAttribute<DoNotCheckAttribute>(), $"member {memberInfo.MemberType} {memberInfo.Name} in class {memberInfo.ReflectedType}");

        /// <summary>
        /// Check if this <paramref name="memberInfo"/> should be ignored when checking if has <paramref name="typeThatMayBeIgnored"/>.
        /// </summary>
        /// <param name="memberInfo">Member to check.</param>
        /// <param name="typeThatMayBeIgnored">Type of attribute that it's being checked.</param>
        /// <returns>Whenever it should be ignored or not.</returns>
        public static bool CheckIfShouldBeIgnored(this MemberInfo memberInfo, Type typeThatMayBeIgnored) => memberInfo.GetAttributeTypesThatShouldBeIgnored().Contains(typeThatMayBeIgnored);

        /// <summary>
        /// Check if this <paramref name="type"/> should be ignored when checking if has <paramref name="typeThatMayBeIgnored"/>.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="typeThatMayBeIgnored">Type of attribute that it's being checked.</param>
        /// <returns>Whenever it should be ignored or not.</returns>
        public static bool CheckIfShouldBeIgnored(this Type type, Type typeThatMayBeIgnored) => type.GetAttributeTypesThatShouldBeIgnored().Contains(typeThatMayBeIgnored);

        /// <summary>
        /// Get attribute types that should be ignored.
        /// </summary>
        /// <param name="memberInfo">Member to check.</param>
        /// <returns>Attribute types that should be ignored.</returns>
        public static IEnumerable<Type> GetAttributeTypesThatShouldBeIgnored(this MemberInfo memberInfo)
        {
            DoNotCheckAttribute attribute = memberInfo.GetCustomAttribute<DoNotCheckAttribute>();
            return attribute?.ignoreTypes ?? Enumerable.Empty<Type>();
        }

        /// <summary>
        /// Get attribute types that should be ignored.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>Attribute types that should be ignored.</returns>
        public static IEnumerable<Type> GetAttributeTypesThatShouldBeIgnored(this Type type)
        {
            DoNotCheckAttribute attribute = type.GetCustomAttribute<DoNotCheckAttribute>();
            return attribute?.ignoreTypes ?? Enumerable.Empty<Type>();
        }
    }
}