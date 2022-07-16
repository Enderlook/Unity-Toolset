using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyPopupOptionAttribute : PropertyAttribute
    {
        /// <summary>
        /// Value that must match the mode field in order to show this field.
        /// </summary>
        internal readonly object target;

        /// <summary>
        /// Allow to use the decorated field as option for the property popup.
        /// </summary>
        /// <param name="target">Value that must match the mode member in other to show this field.</param>
        public PropertyPopupOptionAttribute(object target) => this.target = target;
    }
}
