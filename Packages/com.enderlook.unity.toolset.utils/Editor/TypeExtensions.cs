using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// Helpers methods for reflection usage about <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly Type[] unityDefaultNonPrimitiveSerializables = new Type[]
        {
            typeof(Vector2), typeof(Vector3), typeof(Vector4),
            typeof(Rect), typeof(Quaternion), typeof(Matrix4x4),
            typeof(Color), typeof(Color32), typeof(LayerMask),
            typeof(AnimationCurve), typeof(Gradient), typeof(RectOffset), typeof(GUIStyle)
        };

        private static readonly Type[] validEnumTypes = new Type[]
        {
            // Can't be larger than 32-bits.
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(float),
            typeof(char)
        };

        /// <summary>
        /// Check if the given type can be serialized by Unity.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>Whenever the field can be serialized by Unity of not.</returns>
        public static bool CanBeSerializedByUnity(this Type type)
        {
            if (type.IsArray)
            {
                if (type.GetArrayRank() > 1)
                    return false;
                type = type.GetElementType();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                type = type.GetGenericArguments()[0];

            if (type.IsAbstract || type.IsGenericType || type.IsInterface)
                return false;

            if (type.IsSubclassOf(typeof(UnityObject)))
                return true;

            if (type.IsPrimitive || type.IsValueType || unityDefaultNonPrimitiveSerializables.Contains(type))
                return true;

            if (type.IsEnum)
                return validEnumTypes.Contains(Enum.GetUnderlyingType(type));

            if (type.IsDefined(typeof(SerializableAttribute)) || type.IsDefined(typeof(SerializeReference)))
                return true;

            return false;
        }

        /// <summary>
        /// Check if the given field can be serialized by Unity.
        /// </summary>
        /// <param name="fieldInfo">Field to check.</param>
        /// <returns>Whenever the field can be serialized by Unity of not.</returns>
        public static bool CanBeSerializedByUnity(this FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPublic || fieldInfo.IsDefined(typeof(SerializeField)))
            {
                if (fieldInfo.IsStatic || fieldInfo.IsInitOnly || fieldInfo.IsLiteral || fieldInfo.IsDefined(typeof(NonSerializedAttribute)))
                    return false;

                return fieldInfo.FieldType.CanBeSerializedByUnity();
            }
            return false;
        }

        /// <summary>
        /// Check if the given type can be serialized by Unity.
        /// </summary>
        /// <param name="typeInfo">Typeinfo of type to check.</param>
        /// <returns>Whenever the field can be serialized by Unity of not.</returns>
        public static bool CanBeSerializedByUnity(this TypeInfo typeInfo) => typeInfo.GetType().CanBeSerializedByUnity();
    }
}