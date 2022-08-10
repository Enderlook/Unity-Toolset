using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the onwer of this attribute can only be used to decorate methods that matches an speicifed <see cref="BindingFlags"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageAccessibilityAttribute : Attribute
    {
#if UNITY_EDITOR
        internal readonly BindingFlags bindingFlags;
#endif

        /// <summary>
        /// Determines which <see cref="BindingFlags"/> must have the decorated.
        /// </summary>
        /// <param name="bindingFlags">Necessary binding flags.</param>
        public AttributeUsageAccessibilityAttribute(BindingFlags bindingFlags)
        {
#if UNITY_EDITOR
            this.bindingFlags = bindingFlags;
#endif
        }
    }
}