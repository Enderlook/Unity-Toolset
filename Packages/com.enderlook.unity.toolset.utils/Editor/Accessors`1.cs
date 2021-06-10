using System;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <inheritdoc cref="IAccessors{T}"/>
    public readonly struct Accessors<T> : IAccessors<T>, IEquatable<Accessors<T>>
    {
        private readonly Accessors accessors;

        /// <summary>
        /// Wrap an accessor in a type safe way.
        /// </summary>
        /// <param name="accessors">Accessor to wrap.</param>
        public Accessors(Accessors accessors)
        {
            this.accessors = accessors;
            Type type = accessors.GetPropertyType();
            if (!typeof(T).IsAssignableFrom(type))
                ThrowTypeIsNotAssignableException(type);
        }

        /// <summary>
        /// Creates a wrapper for accessing a field or property of an object.
        /// </summary>
        /// <param name="source">Object which contains the member to access.</param>
        /// <param name="name">Name of the member to access.</param>
        public Accessors(object source, string name)
        {
            accessors = new Accessors(source, name);
            Type type = accessors.GetPropertyType();
            if (!typeof(T).IsAssignableFrom(type))
                ThrowTypeIsNotAssignableException(type);
        }

        /// <summary>
        /// Creates a wrapper for accessing a field or property of an object.
        /// </summary>
        /// <param name="source">Object which contains the member to access.</param>
        /// <param name="name">Name of the member to access.</param>
        /// <param name="index">Index of the member to access.</param>
        public Accessors(object source, string name, int index)
        {
            accessors = new Accessors(source, name, index);
            Type type = accessors.GetPropertyType();
            if (!typeof(T).IsAssignableFrom(type))
                ThrowTypeIsNotAssignableException(type);
        }

        /// <inheritdoc cref="IAccessors{T}.Get"/>
        public T Get() => (T)accessors.Get();

        /// <inheritdoc cref="IAccessors{T}.Set(object)"/>
        public void Set(T value) => accessors.Set(value);

        /// <inheritdoc cref="IAccessors{T}.GetPropertyType"/>
        public Type GetPropertyType() => accessors.GetPropertyType();

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(Accessors<T> other) => accessors.Equals(other.accessors);

        /// <summary>
        /// Calculates the hashcode of this instance.
        /// </summary>
        /// <returns>Haschode of this instance.</returns>
        public override int GetHashCode() => accessors.GetHashCode();

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public override bool Equals(object obj) => obj is Accessors<T> other && Equals(other);

        /// <summary>
        /// Determines if both instances are equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are equal.</returns>
        public static bool operator ==(Accessors<T> left, Accessors<T> right) => left.Equals(right);

        /// <summary>
        /// Determines if both instances are not equal.
        /// </summary>
        /// <param name="left">Instance to check.</param>
        /// <param name="right">Instance to check.</param>
        /// <returns>Whenever both instances are not equal.</returns>
        public static bool operator !=(Accessors<T> left, Accessors<T> right) => !(left == right);

        private static void ThrowTypeIsNotAssignableException(Type type) => throw new ArgumentException($"The accessors isn't assignable to type {typeof(T)}, but {type}.");
    }
}