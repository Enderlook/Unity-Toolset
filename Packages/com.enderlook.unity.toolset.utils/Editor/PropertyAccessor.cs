using Enderlook.Reflection;

using System;
using System.Reflection;

using UnityEditor;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <inheritdoc cref="IAccessors{T}"/>
    public struct PropertyAccessor : IAccessors<object>, IEquatable<PropertyAccessor>
    {
        private readonly SerializedProperty property;
        private readonly FieldInfo fieldInfo;

        /// <summary>
        /// Creates a wrapper for accessing a serialized property.
        /// </summary>
        /// <param name="property">Property which contains the value to access.</param>
        /// <param name="fieldInfo">Member to access.</param>
        public PropertyAccessor(SerializedProperty property, FieldInfo fieldInfo)
        {
            this.property = property;
            this.fieldInfo = fieldInfo;
        }

        /// <inheritdoc cref="IAccessors{T}.Get"/>
        public object Get() => property.objectReferenceValue;

        /// <inheritdoc cref="IAccessors{T}.GetValueType"/>
        public Type GetValueType()
        {
            if (fieldInfo.FieldType.TryGetElementTypeOfArrayOrList(out Type type))
                return type;
            else
                return fieldInfo.GetType();
        }

        /// <inheritdoc cref="IAccessors{T}.Set(T)"/>
        public void Set(object value)
        {
            UnityObject targetObject = property.serializedObject.targetObject;
            if (fieldInfo.FieldType == targetObject.GetType())
                fieldInfo.SetValue(targetObject, value);
            else
            {
                FieldInfo fieldInfo2 = targetObject
                        .GetType()
                        .GetField(property.name, AccessorsHelper.bindingFlags);
                if (fieldInfo2 != null)
                    fieldInfo2.SetValue(targetObject, value);
                else
                    property.objectReferenceValue = (UnityObject)value;
            }
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(PropertyAccessor other) => property == other.property && fieldInfo == other.fieldInfo;

        /// <summary>
        /// Calculates the hashcode of this instance.
        /// </summary>
        /// <returns>Haschode of this instance.</returns>
        public override int GetHashCode() => HashCode.Combine(property, fieldInfo);

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public override bool Equals(object obj) => obj is PropertyAccessor other && Equals(other);

        /// <summary>
        /// Determines if both instances are equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are equal.</returns>
        public static bool operator ==(PropertyAccessor left, PropertyAccessor right) => left.Equals(right);

        /// <summary>
        /// Determines if both instances are not equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are not equal.</returns>
        public static bool operator !=(PropertyAccessor left, PropertyAccessor right) => !(left == right);
    }
}
