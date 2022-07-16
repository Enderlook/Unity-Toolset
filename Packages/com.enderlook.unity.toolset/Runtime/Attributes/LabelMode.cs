namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// How the content will behave.
    /// </summary>
    public enum LabelMode
    {
        /// <summary>
        /// Use the <see cref="string"/> as value.
        /// </summary>
        ByValue = 0,

        /// <summary>
        /// Use the <see cref="string"/> to get the real value by reflection.<br/>
        /// The value will be queried from a field, property or parameterless (or all parameters have default value) method with a return type of <see cref="string"/>
        /// </summary>
        ByReference = 1,
    }
}