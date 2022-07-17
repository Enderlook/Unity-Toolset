
using System;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// Binding flags for exhaustive searchs.
    /// </summary>
    [Flags]
    public enum ExhaustiveBindingFlags
    {
        /// <summary>
        /// Include instance members.
        /// </summary>
        Instance = 1 << 1,

        /// <summary>
        /// Include static members.
        /// </summary>
        Static = 1 << 2
    }
}