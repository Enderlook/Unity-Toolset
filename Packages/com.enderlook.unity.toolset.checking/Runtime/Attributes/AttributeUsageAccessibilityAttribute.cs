using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageAccessibilityAttribute : Attribute
    {
        internal readonly BindingFlags bindingFlags;

        /// <summary>
        /// Determines which <see cref="BindingFlags"/> must have the decorated.
        /// </summary>
        /// <param name="bindingFlags">Necessary binding flags.</param>
        public AttributeUsageAccessibilityAttribute(BindingFlags bindingFlags) => this.bindingFlags = bindingFlags;
    }
}