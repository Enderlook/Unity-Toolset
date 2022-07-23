using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Executes the method decorated by this attribute each time Unity compiles code.<br/>
    /// The method to decorate must have the signature DoSomething().
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    [AttributeUsageMethod(1, parameterType = ParameterMode.VoidOrNone)]
    public sealed class ExecuteWhenCheckAttribute : BaseExecuteWhenCheckAttribute
    {
        /// <summary>
        /// Executes the method decorated by this attribute.<br/>
        /// The method to decorate must have the signature DoSomething().
        /// </summary>
        /// <param name="order">In which order will this method be executed.</param>
        public ExecuteWhenCheckAttribute(int order = 0) : base(order) { }
    }
}