using System;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <inheritdoc cref="IAccessors{T}"/>
    public struct PropertyAccessor<T> : IAccessors<T>, IEquatable<PropertyAccessor<T>>
    {
        private readonly PropertyAccessor accessors;

        /// <summary>
        /// Wrap an accessor in a type safe way.
        /// </summary>
        /// <param name="accessors">Accessor to wrap.</param>
        public PropertyAccessor(PropertyAccessor accessors)
        {
            this.accessors = accessors;
            Type type = accessors.GetValueType();
            if (type != typeof(T))
                throw new ArgumentException($"The {nameof(accessors)} isn't of type {typeof(T)}, but {type}.");
        }

        /// <inheritdoc cref="IAccessors{T}.Get"/>
        public T Get() => (T)accessors.Get();

        /// <inheritdoc cref="IAccessors{T}.Set(object)"/>
        public void Set(T value) => accessors.Set(value);

        /// <inheritdoc cref="IAccessors{T}.GetValueType"/>
        public Type GetValueType() => accessors.GetValueType();

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(PropertyAccessor<T> other) => accessors.Equals(other.accessors);

        /// <summary>
        /// Calculates the hashcode of this instance.
        /// </summary>
        /// <returns>Haschode of this instance.</returns>
        public override int GetHashCode() => accessors.GetHashCode();

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public override bool Equals(object obj) => obj is PropertyAccessor<T> other && Equals(other);

        /// <summary>
        /// Determines if both instances are equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are equal.</returns>
        public static bool operator ==(PropertyAccessor<T> left, PropertyAccessor<T> right) => left.Equals(right);

        /// <summary>
        /// Determines if both instances are not equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are not equal.</returns>
        public static bool operator !=(PropertyAccessor<T> left, PropertyAccessor<T> right) => !(left == right);
    }
}
