using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the owner of this attribute should not be inspected by the post compiling checker.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DoNotInspectAttribute : Attribute { }
}