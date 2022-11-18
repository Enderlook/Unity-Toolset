using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determine relation of types to look for.
    /// </summary>
    [Flags]
    public enum TypeRelationship
    {
        /// <summary>
        /// Must be exact type.<br/>
        /// <c>this == other</c>.
        /// </summary>
        IsEqual = 1 << 0,

        /// <summary>
        /// Must be subclass of but not exact type.<br/>
        /// <c>this != other && this.IsSubclassOf(other)</c>.
        /// </summary>
        IsSubclassOf = 1 << 1,

        /// <summary>
        /// Must be superclass of but not exact type.<br/>
        /// <c>this != other && other.IsSubclassOf(this)</c>.
        /// </summary>
        IsSuperclassOf = 1 << 2,

        /// <summary>
        /// Must be assignable from it but not exact type.<br/>
        /// <c>this != other && this.IsAssignableFrom(other)</c>.
        /// </summary>
        IsAssignableFrom = 1 << 3 | IsSuperclassOf,

        /// <summary>
        /// Must be assignable to but not exact type.<br/>
        /// <c>this != other && other.IsAssignableFrom(this)</c>.
        /// </summary>
        IsAssignableTo = 1 << 4 | IsSubclassOf,

        /// <summary>
        /// Must be subclass of.<br/>
        /// <c>this.IsSubclassOf(other)</c>.
        /// </summary>
        IsEqualOrSubclassOf = IsEqual | IsSubclassOf,

        /// <summary>
        /// Must be superclass of.<br/>
        /// <c>other.IsSubclassOf(this)</c>.
        /// </summary>
        IsEqualOrSuperclassOf = IsEqual | IsSuperclassOf,

        /// <summary>
        /// Must be assignable from.<br/>
        /// <c>this.IsAssignableFrom(other)</c>.
        /// </summary>
        IsEqualOrAssignableFrom = IsEqual | IsAssignableFrom,

        /// <summary>
        /// Must be assignable to.<br/>
        /// <c>other.IsAssignableFrom(this)</c>.
        /// </summary>
        IsEqualOrAssignableTo = IsEqual | IsAssignableTo,
    };
}