using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Determines that this serialized property is a valid property to display by the owner <see cref="PropertyPopupAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyPopupOptionAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Value that must match the mode field in order to show this field.
        /// </summary>
        internal readonly object Target;
#endif

        /// <summary>
        /// Allow to use the decorated field as option for the property popup.
        /// </summary>
        /// <param name="target">Value that must match the mode member in other to show this field.</param>
        public PropertyPopupOptionAttribute(object target)
        {
#if UNITY_EDITOR
            Target = target;
#endif
        }
    }
}
