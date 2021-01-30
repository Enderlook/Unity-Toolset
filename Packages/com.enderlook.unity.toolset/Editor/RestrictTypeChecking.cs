using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;

using System;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset
{
    public static class RestrictTypeChecking
    {
        /// <summary>
        /// Check if the given type restriction is possible.
        /// </summary>
        /// <param name="attribute">Attribute which produces the restriction.</param>
        /// <param name="fieldType">Field to restrict.</param>
        /// <param name="errorMessage">Message error, if return is <see langword="false"/>.</param>
        /// <returns>Whenever its allowed or there is an error.</returns>
        public static bool CheckRestrictionFeasibility(this RestrictTypeAttribute attribute, Type fieldType, out string errorMessage)
        {
            Type oldFieldType = fieldType;
            // Get the element type from arrays or lists
            if (!fieldType.TryGetElementTypeOfArrayOrList(out fieldType))
                fieldType = oldFieldType;

            // Check if that element type inherit from Unity object
            if (!typeof(UnityObject).IsAssignableFrom(fieldType))
            {
                errorMessage = $"Must inherit from {typeof(UnityObject)}, or be an array or list with that type as elements. Is {fieldType}.";
                return false;
            }

            // Check that restrictions are feasibles
            foreach (Type type in attribute.restriction)
            {
                if (type.IsClass)
                {
                    // Restriction type must inherit from Unity object
                    if (!typeof(UnityObject).IsAssignableFrom(type))
                    {
                        errorMessage = $"Attribute {nameof(RestrictTypeAttribute)} has a wrong {nameof(Type)} restriction. Class types must inherit from {typeof(UnityObject)}. One of its restrictions was {type}.";
                        return false;
                    }
                    // Restriction type must be casteable to field type
                    else if (!fieldType.IsAssignableFrom(type))
                    {
                        errorMessage = $"Attribute {nameof(RestrictTypeAttribute)} has a wrong {nameof(Type)} restriction. Restrictions class types must be casteable to the field element type ({fieldType}). One of its restrictions was {type}.";
                        return false;
                    }
                }
                // Structs and primitives are not allowed.
                else if (type.IsValueType || type.IsPrimitive || type.IsPointer)
                {
                    errorMessage = $"Attribute {nameof(RestrictTypeAttribute)} has a wrong {nameof(Type)} restriction. Restrictions can't be value types, primitives nor pointers. One of its restrictions was {type}.";
                    return false;
                }
                // Check for everything else, just to be sure
                else if (!type.IsInterface)
                {
                    errorMessage = $"Attribute {nameof(RestrictTypeAttribute)} has a wrong {nameof(Type)} restriction. Restrictions can only be classes derived from {typeof(UnityObject)} or interfaces. One of its restrictions was {type}.";
                    return false;
                }
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Check if the given type is allowed by <paramref name="attribute"/> restrictions.
        /// </summary>
        /// <param name="attribute">Attribute which produces the restriction.</param>
        /// <param name="resultType"><see cref="Type"/> to be checked</param>
        /// <param name="errorMessage">Message error, if return is <see langword="false"/>.</param>
        /// <returns>Whenever its allowed or there is an error.</returns>
        public static bool CheckIfTypeIsAllowed(this RestrictTypeAttribute attribute, Type resultType, out string errorMessage)
        {
            foreach (Type type in attribute.restriction)
            {
                if (!type.IsAssignableFrom(resultType))
                {
                    errorMessage = $"Require values than can be casted to {type}. {resultType} can't.";
                    return false;
                }
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Check if the given type is allowed by <paramref name="attribute"/> restrictions.
        /// </summary>
        /// <param name="attribute">Attribute which produces the restriction.</param>
        /// <param name="resultType"><see cref="Type"/> to be checked</param>
        /// <returns>Whenever its allowed or there is an error.</returns>
        public static bool CheckIfTypeIsAllowed(this RestrictTypeAttribute attribute, Type resultType)
        {
            foreach (Type type in attribute.restriction)
                if (!type.IsAssignableFrom(resultType))
                    return false;
            return true;
        }
    }
}