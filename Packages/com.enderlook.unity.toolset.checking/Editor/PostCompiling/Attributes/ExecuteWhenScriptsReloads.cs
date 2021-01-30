using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Executes the method decorated by this attribute each time Unity compiles code.<br/>
    /// The method to decorate must have the signature DoSomething().
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    [AttributeUsageMethod(1, parameterType = ParameterMode.VoidOrNone)]
    public sealed class ExecuteWhenScriptsReloads : BaseExecuteWhenScriptsReloads
    {
        /// <summary>
        /// Executes the method decorated by this attribute.<br/>
        /// The method to decorate must have the signature DoSomething().
        /// </summary>
        /// <param name="loop">In which loop of the execution will this script execute.</param>
        public ExecuteWhenScriptsReloads(int loop = 0) : base(loop) { }
    }
}