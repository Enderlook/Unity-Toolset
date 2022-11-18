using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Executes the method decorated by this attribute for each <see cref="Type"/> compiled by Unity each time Unity compiles code.<br/>
    /// The method to decorate must have the signature DoSomething(<see cref="Type"/>).
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    [AttributeUsageMethod(0, typeof(Type))]
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class ExecuteOnEachTypeWhenCheckAttribute : BaseExecuteWhenCheckAttribute
    {
        /// <summary>
        /// Determines rules about in which types does match.
        /// </summary>
        internal readonly TypeFlags typeFilter;

        /// <summary>
        /// Executes the method decorated by this attribute for each <see cref="Type"/> compiled by Unity, that matches the <paramref name="typeFlags"/> criteria.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="Type"/>).
        /// </summary>
        /// <param name="typeFlags">Determines rules about in which types does match.</param>
        /// <param name="order">In which order will this method be executed.</param>
        public ExecuteOnEachTypeWhenCheckAttribute(TypeFlags typeFlags = TypeFlags.AnyType, int order = 0) : base(order) => typeFilter = typeFlags;

        /// <summary>
        /// Executes the method decorated by this attribute for each <see cref="Type"/> compiled by Unity, that matches the <paramref name="typeFlags"/> criteria.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="Type"/>).
        /// </summary>
        /// <param name="order">In which order will this method be executed.</param>
        public ExecuteOnEachTypeWhenCheckAttribute(int order = 0) : base(order) => typeFilter = TypeFlags.AnyType;
    }
}