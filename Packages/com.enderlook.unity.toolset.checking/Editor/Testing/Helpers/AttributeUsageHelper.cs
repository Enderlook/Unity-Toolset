using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using UnityEngine;

using static UnityEditor.Experimental.GraphView.Port;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Only works in Unity Editor.
    /// </summary>
    internal static class AttributeUsageHelper
    {
        /// <summary>
        /// Check if <paramref name="toCheckType"/> is in <paramref name="types"/> according to <paramref name="typeFlags"/> and <paramref name="isBlackList"/>.
        /// </summary>
        /// <param name="types"><see cref="Type"/>s target.</param>
        /// <param name="typeFlags">Additional rules to between <paramref name="types"/> and <paramref name="toCheckType"/>.</param>
        /// <param name="isBlackList">If <see langword="true"/> <paramref name="toCheckType"/> must not be related with <paramref name="types"/>.</param>
        /// <param name="supportEnumerable">If <see langword="true"/>, it will also check the element type of field of array o list types.</param>
        /// <param name="toCheckType"><see cref="Type"/> to be checked.</param>
        /// <returns>Whenever the criteria matches.</returns>
        /// <remarks>Only use in Unity Editor.</remarks>
        internal static bool CheckContains(Type[] types, TypeRelationship typeFlags, bool isBlackList, bool supportEnumerable, Type toCheckType)
        {
            if (types.Length == 0)
                return true;

            Type oldToCheckType = toCheckType;
            if (supportEnumerable)
            {
                if (oldToCheckType.IsArray)
                    toCheckType = oldToCheckType.GetElementType();
                else if (oldToCheckType.IsGenericType && oldToCheckType.GetGenericTypeDefinition() == typeof(List<>))
                    toCheckType = oldToCheckType.GetGenericArguments()[0];
            }

            bool contains = false;

            if ((typeFlags & TypeRelationship.IsEqual) != 0)
            {
                contains = types.Contains(toCheckType);
                if (contains)
                    goto end;
            }

            if ((typeFlags & TypeRelationship.IsAssignableTo) != 0)
            {
                foreach (Type type in types)
                {
                    if (toCheckType.IsAssignableFrom(type) && toCheckType != type)
                    {
                        contains = true;
                        goto end;
                    }
                }
            }
            else if ((typeFlags & TypeRelationship.IsSubclassOf) != 0)
            {
                foreach (Type type in types)
                {
                    if (toCheckType.IsSubclassOf(type) && toCheckType != type)
                    {
                        contains = true;
                        goto end;
                    }
                }
            }

            if ((typeFlags & TypeRelationship.IsAssignableFrom) != 0)
            {
                foreach (Type type in types)
                {
                    if (type.IsAssignableFrom(toCheckType) && toCheckType != type)
                    {
                        contains = true;
                        goto end;
                    }
                }
            }
            else
            {
                if ((typeFlags & TypeRelationship.IsSuperclassOf) != 0)
                {
                    foreach (Type type in types)
                    {
                        if (type.IsSubclassOf(toCheckType) && toCheckType != type)
                        {
                            contains = true;
                            goto end;
                        }
                    }
                }
            }

        end:
            return contains != isBlackList;
        }

        /// <summary>
        /// Append a list of the supported types.
        /// </summary>
        /// <param name="builder">Where text will be appended.</param>
        /// <param name="types"><see cref="Type"/>s target.</param>
        /// <param name="typeFlags">Additional rules to between <paramref name="types"/> and <paramref name="toCheckType"/>.</param>
        /// <param name="isBlackList">If <see langword="true"/> <paramref name="toCheckType"/> must not be related with <paramref name="types"/>.</param>
        /// <param name="supportEnumerable">If <see langword="true"/>, it will also check the element type of field of array o list types.</param>
        public static LogBuilder AppendSupportedTypes(LogBuilder builder, Type[] types, TypeRelationship typeFlags, bool isBlackList, bool supportEnumerable)
        {
            Debug.Assert(types.Length > 0);
            LogBuilder Is() => builder.Append(isBlackList ? " isn't " : " is ");

            bool match = false;
            if ((typeFlags & TypeRelationship.IsSubclassOf) != 0)
            {
                match = true;
                Is().Append("a subclass of");
            }
            if ((typeFlags & TypeRelationship.IsSuperclassOf) != 0)
            {
                if (match)
                    builder.Append(", or");
                match = true;
                Is().Append("a superclass of");
            }
            if ((typeFlags & TypeRelationship.IsSuperclassOf) != 0)
            {
                if (match)
                    builder.Append(", or");
                match = true;
                Is().Append("assignable to");
            }
            if ((typeFlags & TypeRelationship.IsAssignableTo) != 0)
            {
                if (match)
                    builder.Append(", or");
                match = true;
                Is().Append("assignable from");
            }
            if ((typeFlags & TypeRelationship.IsEqual) != 0)
            {
                if (match)
                    builder.Append(", or");
                Is().Append("equal to");
            }

            if (types.Length > 0)
                builder.Append(" any of the following types: ");
            else
                builder.Append(" ");

            foreach (Type type in types)
                builder.Append(type).Append(", ");
            builder.RemoveLast(", ".Length);

            if (supportEnumerable)
                builder.Append(". Or if they are arrays or lists, their element types matches previous criteria.");

            return builder;
        }

        public static int GetMaximumRequiredCapacity(Type[] types)
        {
            // This values were got by concatenating the sum of the largest possible paths of appended constants in AppendSupportedTypes method.
            int capacity = 235;
            capacity += (types.Length * 2) - 2;
            foreach (Type type in types)
                capacity += type.ToString().Length;
            return capacity;
        }
    }
}