namespace Enderlook.Unity.Toolset.Attributes
{
    public enum ComparisonMode
    {
        /// <summary>
        /// The property must be equal to the object to compare.
        /// </summary>
        Equal,

        /// <summary>
        /// The property must be unequal to the object to compare.
        /// </summary>
        NotEqual,

        /// <summary>
        /// The property must be greater than the object to compare.
        /// </summary>
        Greater,

        /// <summary>
        /// The property must be greater or equal than the object to compare.
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// The property must be lesser than the object to compare.
        /// </summary>
        Less,

        /// <summary>
        /// The property must be lesser or equal than the object to compare.
        /// </summary>
        LessOrEqual,

        /// <summary>
        /// Both the property and the object to compare must be <see cref="true"/>.<br/>
        /// This is only valid on <see cref="bool"/>.
        /// </summary>
        And,

        /// <summary>
        /// Either the property or the object to compare must be <see cref="true"/>.<br/>
        /// This is only valid on <see cref="bool"/>.
        /// </summary>
        Or,

        /// <summary>
        /// The property has the flag specified by the object to comapre.<br/>
        /// This is only valid on <see cref="enum"/>.
        /// </summary>
        HasFlag,

        /// <summary>
        /// The property doesn't have the flag specified by the object to comapre.<br/>
        /// This is only valid on <see cref="enum"/>.
        /// </summary>
        NotFlag,
    }
}