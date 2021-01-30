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
    public sealed class ExecuteOnEachFieldOfEachTypeWhenScriptsReloads : BaseExecuteWhenScriptsReloads
    {
        /// <summary>
        /// Determines rules about in which field does match.
        /// </summary>
        public readonly FieldSerialization fieldFilter;

        /// <summary>
        /// Executes the method decorated by this attribute for each field on each <see cref="Type"/> compiled by Unity.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="FieldInfo"/>).
        /// </summary>
        /// <param name="fieldFlags">Whenever it must be Unity able to serialize it or if it does not matter.</param>
        /// <param name="loop">In which loop of the execution will this script execute.</param>
        public ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization fieldFlags = FieldSerialization.EitherSerializableOrNotByUnity, int loop = 0) : base(loop) => fieldFilter = fieldFlags;
    }
}