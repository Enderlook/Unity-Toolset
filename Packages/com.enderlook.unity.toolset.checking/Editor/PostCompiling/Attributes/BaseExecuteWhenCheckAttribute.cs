using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    public abstract class BaseExecuteWhenCheckAttribute : Attribute
    {
        /// <summary>
        /// In which order will this method be executedd.
        /// </summary>
        internal readonly int order;

        protected BaseExecuteWhenCheckAttribute(int order = 0) => this.order = order;
    }
}