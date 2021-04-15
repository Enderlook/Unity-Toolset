using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    [AttributeUsageRequireDataType(typeof(Sprite), typeof(Texture2D), typeof(string), includeEnumerableTypes = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    public sealed class DrawTextureAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whenever the texture will be drawn on the same line as the property or in a line bellow.
        /// </summary>
        public readonly bool drawOnSameLine;

        /// <summary>
        /// Whenever the texture will be centered.
        /// </summary>
        public readonly bool centered;

        /// <summary>
        /// Height of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, the height of the property is used.
        /// </summary>
        public readonly float height;

        /// <summary>
        /// Width of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, <see cref="height"/> is used.
        /// </summary>
        public readonly float width;

        /// <summary>
        /// Draw the texture next to the field in the inspector.
        /// </summary>
        public DrawTextureAttribute() : this(-1, -1, true, false) { }

        /// <summary>
        /// Draw the texture next to the field in the inspector.
        /// </summary>
        /// <param name="drawOnSameLine">Whenever the texture will be drawn on the same line as the property or in a line bellow.</param>
        public DrawTextureAttribute(bool drawOnSameLine = true) : this(-1, -1, drawOnSameLine, false) { }

        /// <summary>
        /// Draw the texture next to the field in the inspector.
        /// </summary>
        /// <param name="drawOnSameLine">Whenever the texture will be drawn on the same line as the property or in a line bellow.</param>
        /// <param name="centered">Whenever the textre will be centered.<br/>
        /// This is ignored if <paramref name="drawOnSameLine"/> is <see langword="true"/>.</param>
        public DrawTextureAttribute(bool drawOnSameLine = true, bool centered = false) : this(-1, -1, drawOnSameLine, centered) { }

        /// <summary>
        /// Draw the texture below the field in the inspector.
        /// </summary>
        /// <param name="size">Size of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, the height of the property is used.</param>
        /// <param name="drawOnSameLine">Whenever the texture will be drawn on the same line as the property or in a line bellow.</param>
        /// <param name="centered">Whenever the textre will be centered.<br/>
        /// This is ignored if <paramref name="drawOnSameLine"/> is <see langword="true"/>.</param>
        public DrawTextureAttribute(float size, bool drawOnSameLine = false, bool centered = false) : this(size, size, drawOnSameLine, centered) { }

        /// <summary>
        /// Draw the texture of the field in the inspector.
        /// </summary>
        /// <param name="height">Height of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, the height of the property is used.</param>
        /// <param name="width">Width of the <see cref="Rect"/> used to show the texture.<br/>
        /// On -1, <paramref name="height"/> is used.</param>
        /// <param name="drawOnSameLine">Whenever the texture will be drawn on the same line as the property or in a line bellow.<br/>
        /// This is ignored if <see cref="hideMode"/> is <see cref="Hide.All"/>.</param>
        /// <param name="centered">Whenever the textre will be centered.<br/>
        /// This is ignored if <paramref name="drawOnSameLine"/> is <see langword="true"/>.</param>
        public DrawTextureAttribute(float height, float width, bool drawOnSameLine = false, bool centered = false)
        {
            this.height = height;
            this.width = width;
            this.drawOnSameLine = drawOnSameLine;
            this.centered = centered;
        }
    }
}