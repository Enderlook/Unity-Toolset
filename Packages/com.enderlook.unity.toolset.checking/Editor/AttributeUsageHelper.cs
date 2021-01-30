using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Only works in Unity Editor.
    /// </summary>
    internal static class AttributeUsageHelper
    {
        /// <summary>
        /// Produces a <see cref="HashSet{T}"/> with <paramref name="types"/>.
        /// </summary>
        /// <param name="types">Array of <see cref="Type"/> to use.</param>
        /// <param name="includeEnumerableTypes">If <see langword="true"/>, it will also check for array o list versions of types.<br/>
        /// Useful because Unity <see cref="PropertyDrawer"/> are draw on each element of an array or list <see cref="SerializedProperty"/></param>
        /// <returns><see cref="HashSet{T}"/> with all types to check.</returns>
        /// <remarks>Only use in Unity Editor.</remarks>
        internal static HashSet<Type> GetHashsetTypes(Type[] types, bool includeEnumerableTypes = false)
        {
            if (includeEnumerableTypes)
            {
                HashSet<Type> hashSet = new HashSet<Type>();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    hashSet.Add(type);
                    hashSet.Add(typeof(List<>).MakeGenericType(type));
                    hashSet.Add(type.MakeArrayType());
                }
                return hashSet;
            }
            else
                return new HashSet<Type>(types);
        }

        /// <summary>
        /// Produce a <see cref="string"/> with all elements of <paramref name="types"/> and include specific text from <paramref name="checkingFlags"/>.
        /// </summary>
        /// <param name="types">Elements to include in the result.</param>
        /// <param name="checkingFlags">Additional phrases.</param>
        /// <param name="isBlackList">Whenever the result forbid instead of require the <paramref name="types"/>.</param>
        /// <returns>A <see cref="string"/> with all elements.</returns>
        /// <remarks>Only use in Unity Editor.</remarks>
        internal static string GetTextTypes(IEnumerable<Type> types, TypeCasting checkingFlags, bool isBlackList = false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .Append(isBlackList ? "doesn't" : "only")
                .Append(" accept types of ")
                .Append(string.Join(", ", types.Select(e => e.Name)));
            if ((checkingFlags & TypeCasting.CheckSubclassTypes) != 0)
                stringBuilder.Append(", their subclasses");
            if ((checkingFlags & TypeCasting.CheckSuperclassTypes) != 0)
                stringBuilder.Append(", their superclasses");
            if ((checkingFlags & TypeCasting.CheckSuperclassTypes) != 0)
                stringBuilder.Append(", types assignable to them");
            if ((checkingFlags & TypeCasting.CheckCanBeAssignedTypes) != 0)
                stringBuilder.Append(", types assignable from them");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Check if <paramref name="toCheckType"/> is in <paramref name="types"/> according to <paramref name="typeFlags"/> and <paramref name="isBlackList"/>.
        /// If not found, it will log an exception in Unity.
        /// </summary>
        /// <param name="attributeCheckerName">Name of the attribute checker.</param>
        /// <param name="types"><see cref="Type"/>s target.</param>
        /// <param name="typeFlags">Additional rules to between <paramref name="types"/> and <paramref name="toCheckType"/>.</param>
        /// <param name="isBlackList">If <see langword="true"/> <paramref name="toCheckType"/> must not be related with <paramref name="types"/>.</param>
        /// <param name="allowedTypes">String version of <paramref name="types"/>.</param>
        /// <param name="toCheckType"><see cref="Type"/> to be checked.</param>
        /// <param name="attributeName">Name of the current attribute which is being checked.</param>
        /// <param name="toCheckName">Name of what is <paramref name="toCheckType"/> or where it was taken from (e.g: <c><see cref="System.Reflection.FieldInfo"/>.Name</c>.</param>
        /// <remarks>Only use in Unity Editor.</remarks>
        internal static void CheckContains(string attributeCheckerName, HashSet<Type> types, TypeCasting typeFlags, bool isBlackList, string allowedTypes, Type toCheckType, string attributeName, string toCheckName)
        {
            bool contains = types.Contains(toCheckType);

            if (!contains)
            {
                void Check(Func<Type, Type, bool> test)
                {
                    foreach (Type type in types)
                    {
                        bool result = test(toCheckType, type);
                        if (result)
                        {
                            contains = true;
                            break;
                        }
                    }
                }

                // Check if checkingFlags has the following flags
                // We could use checkingFlags.HasFlag(flag), but it's ~10 times slower
                if ((typeFlags & TypeCasting.CheckSubclassTypes) != 0)
                    Check((f, t) => f.IsSubclassOf(t));
                if ((typeFlags & TypeCasting.CheckSuperclassTypes) != 0 && !contains)
                    Check((f, t) => t.IsSubclassOf(f));
                if ((typeFlags & TypeCasting.CheckCanBeAssignedTypes) != 0 && !contains)
                    Check((f, t) => f.IsAssignableFrom(t));
                if ((typeFlags & TypeCasting.CheckIsAssignableTypes) != 0 && !contains)
                    Check((f, t) => t.IsAssignableFrom(f));
            }

            if (contains == isBlackList)
                Debug.LogError($"According to {attributeCheckerName}, {attributeName} {allowedTypes}. {toCheckName} is {toCheckType.Name} type.");
        }
    }
}