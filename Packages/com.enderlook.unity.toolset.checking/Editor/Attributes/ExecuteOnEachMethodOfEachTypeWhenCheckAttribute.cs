using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Executes the method decorated by this attribute for each method on each <see cref="Type"/> compiled by Unity each time Unity compiles code.<br/>
    /// The method to decorate must have the signature DoSomething(<see cref="MethodInfo"/>).
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    [AttributeUsageMethod(1, typeof(MethodInfo))]
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class ExecuteOnEachMethodOfEachTypeWhenCheckAttribute : BaseExecuteWhenCheckAttribute
    {
        /// <summary>
        /// Executes the method decorated by this attribute for each method on each <see cref="Type"/> compiled by Unity.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="MethodInfo"/>).
        /// </summary>
        /// <param name="order">In which order will this method be executed.</param>
        public ExecuteOnEachMethodOfEachTypeWhenCheckAttribute(int order = 0) : base(order) { }
    }
}