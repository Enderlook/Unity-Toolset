using System;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Rules that should be match by the type.
    /// </summary>
    [Flags]
    public enum TypeFlags
    {
        /// <summary>
        /// Only execute on types which <see cref="Type.IsEnum"/> is <see langword="true"/>.
        /// </summary>
        IsEnum = 1,

        /// <summary>
        /// Only execute on types which <see cref="Type.IsEnum"/> is <see langword="false"/>.
        /// </summary>
        IsNonEnum = 1 << 2,

        /// <summary>
        /// Execute on types regardless <see cref="Type.IsEnum"/>.
        /// </summary>
        IsEitherEnumNonEnum = IsEnum | IsNonEnum,
    }
}