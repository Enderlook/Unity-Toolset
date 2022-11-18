namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// The parameter mode.
    /// </summary>
    public enum ParameterModifier
    {
        /// <summary>
        /// Specifies a parameter without modifiers.
        /// </summary>
        None,

        /// <summary>
        /// Specifies an <see langword="in"/> parameter.
        /// </summary>
        In,

        /// <summary>
        /// Specifies an <see langword="out"/> parameter.
        /// </summary>
        Out,

        /// <summary>
        /// Specifies a <see langword="ref"/> parameter.
        /// </summary>
        Ref,
    }
}