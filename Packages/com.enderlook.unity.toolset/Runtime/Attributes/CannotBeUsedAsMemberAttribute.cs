using System;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Determines that this type can't be used as a serialized property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CannotBeUsedAsMemberAttribute : Attribute { }
}