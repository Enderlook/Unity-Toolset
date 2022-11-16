namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Determines how a texture is drawn in <see cref="DrawTextureAttribute"/>.
    /// </summary>
    public enum DrawTextureMode
    {
        /// <summary>
        /// Draw the texture in the current line.
        /// </summary>
        CurrentLine,

        /// <summary>
        /// Draw the texture in a new line aligned to the left.
        /// </summary>
        NewLineLeft,

        /// <summary>
        /// Draw the texture in a new line aligned to the right.
        /// </summary>
        NewLineRight,

        /// <summary>
        /// Draw the texture in a new line aligned to the center.
        /// </summary>
        NewLineCenter,
    }
}