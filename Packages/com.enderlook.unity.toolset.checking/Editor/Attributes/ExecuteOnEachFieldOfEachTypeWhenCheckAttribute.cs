using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Executes the method decorated by this attribute for each field on each <see cref="Type"/> compiled by Unity each time Unity compiles code.<br/>
    /// The method to decorate must have the signature DoSomething(<see cref="FieldInfo"/>).
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    [AttributeUsageMethod(1, typeof(FieldInfo))]
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class ExecuteOnEachFieldOfEachTypeWhenCheckAttribute : BaseExecuteWhenCheckAttribute
    {
        /// <summary>
        /// Determines rules about in which field does match.
        /// </summary>
        internal readonly FieldSerialization fieldFilter;

        /// <summary>
        /// Executes the method decorated by this attribute for each field on each <see cref="Type"/> compiled by Unity.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="FieldInfo"/>).
        /// </summary>
        /// <param name="fieldFlags">Whenever it must be Unity able to serialize it or if it does not matter.</param>
        /// <param name="order">In which order will this method be executed.</param>
        public ExecuteOnEachFieldOfEachTypeWhenCheckAttribute(FieldSerialization fieldFlags = FieldSerialization.AnyField, int order = 0) : base(order) => fieldFilter = fieldFlags;

        /// <summary>
        /// Executes the method decorated by this attribute for each field on each <see cref="Type"/> compiled by Unity.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="FieldInfo"/>).
        /// </summary>
        /// <param name="order">In which order will this method be executed.</param>
        public ExecuteOnEachFieldOfEachTypeWhenCheckAttribute(int order = 0) : base(order) => fieldFilter = FieldSerialization.AnyField;
    }
}