using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    public abstract class BaseExecuteWhenCheckAttribute : Attribute
    {
        /// <summary>
        /// In which loop of the execution will this script executed.<br/>
        /// Accept any kind of number.
        /// </summary>
        internal readonly int loop;

        protected BaseExecuteWhenCheckAttribute(int loop = 0) => this.loop = loop;
    }
}