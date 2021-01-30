using System;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CannotBeUsedAsMemberAttribute : Attribute { }
}