using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Draw a texture next to the drawn serialized property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    [AttributeUsageRequireDataType(typeof(Sprite), typeof(Texture2D), typeof(string), includeEnumerableTypes = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    public sealed class DrawTextureAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Determines how the texture should be drawn.
        /// </summary>
        internal readonly DrawTextureMode mode;

        /// <summary>
        /// Height of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, the height of the property is used.
        /// </summary>
        internal readonly float height;

        /// <summary>
        /// Width of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, <see cref="height"/> is used.
        /// </summary>
        internal readonly float width;
#endif

        /// <summary>
        /// Draw the texture next to the field in the inspector.
        /// </summary>
        /// <param name="mode">Determines how the texture should be drawn.</param>
        public DrawTextureAttribute(DrawTextureMode mode = DrawTextureMode.CurrentLine)
        {
#if UNITY_EDITOR
            height = -1;
            width = -1;
            this.mode = mode;
#endif
        }

        /// <summary>
        /// Draw the texture below the field in the inspector.
        /// </summary>
        /// <param name="size">Size of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, the height of the property is used.<br/>
        /// This determines the height of the texture. The width is changes automatically to maintain the correct aspect ratio.<br/>
        /// Note that in order to upscale the texture, it requires read access that must be configured during the import of the texture.</param>
        /// <param name="mode">Determines how the texture should be drawn.</param>
        /// This is ignored if <paramref name="drawOnSameLine"/> is <see langword="true"/>.</param>
        public DrawTextureAttribute(float size, DrawTextureMode mode = DrawTextureMode.CurrentLine)
        {
#if UNITY_EDITOR
            height = size;
            width = size;
            this.mode = mode;
#endif
        }
    }
}