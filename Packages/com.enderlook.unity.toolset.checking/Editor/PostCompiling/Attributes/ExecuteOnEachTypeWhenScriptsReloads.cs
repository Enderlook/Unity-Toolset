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
    public sealed class ExecuteOnEachTypeWhenScriptsReloads : BaseExecuteWhenScriptsReloads
    {
        /// <summary>
        /// Rules that should be match by the type.
        /// </summary>
        [Flags]
        public enum TypeFlags
        {
            /// <summary>
            /// Only execute on types which <see cref="Type.IsEnum"/> is <see langword="true"/>.
            /// </summary>
            IsEnum = 1,

            /// <summary>
            /// Only execute on types which <see cref="Type.IsEnum"/> is <see langword="false"/>.
            /// </summary>
            IsNonEnum = 1 << 2,

            /// <summary>
            /// Execute on types regardless <see cref="Type.IsEnum"/>.
            /// </summary>
            IsEitherEnumNonEnum = IsEnum | IsNonEnum,
        }

        /// <summary>
        /// Determines rules about in which types does match.
        /// </summary>
        public readonly TypeFlags typeFilter;

        /// <summary>
        /// Executes the method decorated by this attribute for each <see cref="Type"/> compiled by Unity, that matches the <paramref name="typeFlags"/> criteria.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="Type"/>).
        /// </summary>
        /// <param name="typeFlags">Determines rules about in which types does match.</param>
        /// <param name="loop">In which loop of the execution will this script execute.</param>
        public ExecuteOnEachTypeWhenScriptsReloads(TypeFlags typeFlags = TypeFlags.IsEitherEnumNonEnum, int loop = 0) : base(loop) => typeFilter = typeFlags;
    }
}