using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyPopupOptionAttribute : PropertyAttribute
    {
        /// <summary>
        /// Value that must match the mode field in other to show this field.
        /// </summary>
        public readonly object target;

        /// <summary>
        /// Allow to use the decoreated field as option for the property popup.
        /// </summary>
        /// <param name="target">Value that must match the mode field in other to show this field.</param>
        public PropertyPopupOptionAttribute(object target) => this.target = target;
    }
}
