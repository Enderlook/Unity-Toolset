using System;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// Represent the accessors of a field or property.
    /// </summary>
    public interface IAccessors<T>
    {
        /// <summary>
        /// Get the value.
        /// </summary>
        /// <returns>Value of the accessor.</returns>
        T Get();

        /// <summary>
        /// Set a value.
        /// </summary>
        /// <param name="value">value to set.</param>
        void Set(T value);

        /// <summary>
        /// Returns the type of objects this accessor use.
        /// </summary>
        /// <returns>Type of objects this accessor use.</returns>
        Type GetPropertyType();
    }
}