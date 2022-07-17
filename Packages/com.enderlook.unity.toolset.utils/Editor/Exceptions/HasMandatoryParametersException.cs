using System.Reflection;

using System;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// Exception that is thrown when a method does require mandatory parameters.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "<pendiente>")]
    public class HasMandatoryParametersException : Exception
    {
        /// <summary>
        /// Represents a methods which requires parameters that aren't optional nor params.
        /// </summary>
        /// <param name="methodInfo">Method info found.</param>
        public HasMandatoryParametersException(MethodInfo methodInfo) : base($"{nameof(MethodInfo)} {methodInfo} from {nameof(Type)} {methodInfo.ReflectedType} has parameters which aren't optional, hasn't default value nor has the attribute {nameof(ParamArrayAttribute)}.") { }
    }
}