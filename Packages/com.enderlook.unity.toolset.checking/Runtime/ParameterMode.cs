namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// The parameter mode.
    /// </summary>
    public enum ParameterMode
    {
        /// <summary>
        /// Specifies an common parameter.<br/>
        /// If <see cref="AttributeUsageMethodAttribute.parameterNumber"/> is 0, this will be ignored.
        /// </summary>
        Common,

        /// <summary>
        /// Specifies an in parameter.<br/>
        /// If <see cref="AttributeUsageMethodAttribute.parameterNumber"/> is 0, this will be ignored.
        /// </summary>
        In,

        /// <summary>
        /// Specifies an out parameter.
        /// If <see cref="AttributeUsageMethodAttribute.parameterNumber"/> is 0, this will be ignored.
        /// </summary>
        Out,

        /// <summary>
        /// Specifies a reference parameter.
        /// If <see cref="AttributeUsageMethodAttribute.parameterNumber"/> is 0, this will be ignored.
        /// </summary>
        Ref,

        /// <summary>
        /// Specifies that this parameter should not exist.
        /// </summary>
        VoidOrNone
    }
}