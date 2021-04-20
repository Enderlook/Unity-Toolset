using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <inheritdoc cref="IAccessors{T}"/>
    public readonly struct Accessors : IAccessors<object>, IEquatable<Accessors>
    {
        private readonly object source;
        private readonly MemberInfo memberInfo;
        private readonly int index;

        /// <summary>
        /// Creates a wrapper for accessing a field or property of an object.
        /// </summary>
        /// <param name="source">Object which contains the member to access.</param>
        /// <param name="memberInfo">Member to access.</param>
        internal Accessors(object source, MemberInfo memberInfo)
        {
            this.source = source;
            this.memberInfo = memberInfo;
            index = -1;
        }

        /// <summary>
        /// Creates a wrapper for accessing a field or property of an object.
        /// </summary>
        /// <param name="array">Array which contains the element to access.</param>
        /// <param name="index">Index to access.</param>
        internal Accessors(Array array, int index)
        {
            source = array;
            this.index = index;
            memberInfo = null;
        }

        /// <summary>
        /// Creates a wrapper for accessing a field or property of an object.
        /// </summary>
        /// <param name="source">Object which contains the member to access.</param>
        /// <param name="name">Name of the member to access.</param>
        public Accessors(object source, string name)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            this.source = source;
            memberInfo = null;
            index = -1;

            Type type = source.GetType();

            while (type != null)
            {
                memberInfo = type.GetField(name, AccessorsHelper.bindingFlags);
                if (!(memberInfo is null))
                    return;

                memberInfo = type.GetProperty(name, AccessorsHelper.bindingFlags | BindingFlags.IgnoreCase);
                if (!(memberInfo is null))
                    return;

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Creates a wrapper for accessing a field or property of an object.
        /// </summary>
        /// <param name="source">Object which contains the member to access.</param>
        /// <param name="name">Name of the member to access.</param>
        /// <param name="index">Index of the member to access.</param>
        public Accessors(object source, string name, int index)
        {
            this.source = source.GetValue(name);

            memberInfo = null;
            this.index = index;

            if (this.source is Array)
                return;

            if (!(this.source is IEnumerable))
                throw new ArgumentException($"Accessor with index is only supported for members of type {typeof(Array)} or {typeof(IEnumerable)}.");
        }

        /// <inheritdoc cref="IAccessors{T}.Set(T)"/>
        public void Set(object value)
        {
            if (memberInfo is FieldInfo field)
                field.SetValue(source, value);
            else if (memberInfo is PropertyInfo property)
                property.SetValue(source, value);
            else if (source is Array array)
                array.SetValue(value, index);
            else
                throw new NotSupportedException($"Set not supported on {nameof(IEnumerable)} types.");
        }

        /// <inheritdoc cref="IAccessors{T}.Get"/>
        public object Get()
        {
            if (memberInfo is FieldInfo field)
                return field.GetValue(source);
            else if (memberInfo is PropertyInfo property)
                return property.GetValue(source);
            else if (source is Array array)
                return array.GetValue(index);

            IEnumerator enumerator = ((IEnumerable)source).GetEnumerator();
            for (int i = 0; i <= index; i++)
                if (!enumerator.MoveNext())
                    throw new ArgumentOutOfRangeException($"The enumerable doesn't contains an element at index {index}.");
            return enumerator.Current;
        }

        /// <inheritdoc cref="IAccessors{T}.GetPropertyType"/>
        public Type GetPropertyType()
        {
            if (memberInfo is FieldInfo field)
                return field.FieldType;
            else if (memberInfo is PropertyInfo property)
                return property.PropertyType;
            else if (source is Array array)
                return array.GetType().GetElementType();
            else
            {
                Type type = source.GetType();
                bool hasIEnumerable = false;
                foreach (Type @interface in type.GetInterfaces())
                    if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        return @interface.GenericTypeArguments[0];
                    else if (@interface == typeof(IEnumerable))
                        hasIEnumerable = true;
                if (hasIEnumerable)
                    return typeof(object);
            }
            System.Diagnostics.Debug.Fail("Impossible state.");
            return null;
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(Accessors other) => source == other.source && memberInfo == other.memberInfo && index == other.index;

        /// <summary>
        /// Calculates the hashcode of this instance.
        /// </summary>
        /// <returns>Haschode of this instance.</returns>
        public override int GetHashCode() => HashCode.Combine(source, memberInfo, index);

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public override bool Equals(object obj) => obj is Accessors other && Equals(other);

        /// <summary>
        /// Determines if both instances are equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are equal.</returns>
        public static bool operator ==(Accessors left, Accessors right) => left.Equals(right);

        /// <summary>
        /// Determines if both instances are not equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are not equal.</returns>
        public static bool operator !=(Accessors left, Accessors right) => !(left == right);
    }
}