using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Executes the method decorated by this attribute for each <see cref="Type"/> compiled by Unity each time Unity compiles code.<br/>
    /// The method to decorate must have the signature DoSomething(<see cref="Type"/>).
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    [AttributeUsageMethod(1, typeof(Type))]
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
        /// <param name="loop">In which loop of the execution will this script execute.</param>
        public ExecuteOnEachTypeWhenCheckAttribute(TypeFlags typeFlags = TypeFlags.AnyType, int loop = 0) : base(loop) => typeFilter = typeFlags;

        /// <summary>
        /// Executes the method decorated by this attribute for each <see cref="Type"/> compiled by Unity, that matches the <paramref name="typeFlags"/> criteria.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="Type"/>).
        /// </summary>
        /// <param name="loop">In which loop of the execution will this script execute.</param>
        public ExecuteOnEachTypeWhenCheckAttribute(int loop = 0) : base(loop) => typeFilter = TypeFlags.AnyType;
    }
}