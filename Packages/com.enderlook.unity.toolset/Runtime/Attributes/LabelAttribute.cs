using Enderlook.Unity.Toolset.Checking;

using System;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Modifies the <see cref="GUIContent"/> associated with the <see cref="UnityEditor.SerializedProperty"/> of this field.
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LabelAttribute : PropertyAttribute
    {
        internal readonly string DisplayNameOrGuiContent;

        internal readonly string Tooltip;

        internal readonly LabelMode DisplayNameMode;

        internal readonly LabelMode TooltipMode;

        /// <summary>
        /// Modifies the <see cref="GUIContent"/> associated with the <see cref="UnityEditor.SerializedProperty"/> of this field.
        /// </summary>
        /// <param name="displayName">If <c><paramref name="displayNameMode"/> == <see cref="LabelMode.ByValue"/></c>, this is the property's display name.<br/>
        /// If <c><paramref name="displayNameMode"/> == <see cref="LabelMode.ByReference"/></c>, this is a member that contains the property's tooltip or <see cref="GUIContent"/>.<br/>
        /// The member must be a field, property (with Get method) or method (with only optional or params parameters) that returns <see cref="string"/> or <see cref="GUIContent"/>.</param>
        /// <param name="displayNameMode">Determines how <paramref name="displayName"/> is interpreted.</param>
        public LabelAttribute(string displayName, LabelMode displayNameMode = LabelMode.ByValue)
        {
            DisplayNameOrGuiContent = displayName;
            DisplayNameMode = displayNameMode;
        }

        /// <summary>
        /// Modifies the <see cref="GUIContent"/> associated with the <see cref="UnityEditor.SerializedProperty"/> of this field.
        /// </summary>
        /// <param name="displayName">Display name of the property.</param>
        /// <param name="tooltip">If <c><paramref name="tooltipMode"/> == <see cref="LabelMode.ByValue"/></c>, this is the property's tooltip.<br/>
        /// If <c><paramref name="tooltipMode"/> == <see cref="LabelMode.ByReference"/></c>, this is a member that contains the property's tooltip.<br/>
        /// The member must be a field, property (with Get method) or method (with only optional or params parameters) that returns <see cref="string"/>.</param>
        /// <param name="tooltipMode">Determines how <paramref name="tooltip"/> is interpreted.</param>
        public LabelAttribute(string displayName, string tooltip, LabelMode tooltipMode = LabelMode.ByValue)
        {
            DisplayNameOrGuiContent = displayName;
            Tooltip = tooltip;
            TooltipMode = tooltipMode;
        }

        /// <summary>
        /// Modifies the <see cref="GUIContent"/> associated with the <see cref="UnityEditor.SerializedProperty"/> of this field.
        /// </summary>
        /// <param name="displayName">If <c><paramref name="displayNameMode"/> == <see cref="LabelMode.ByValue"/></c>, this is the property's display name.<br/>
        /// If <c><paramref name="displayNameMode"/> == <see cref="LabelMode.ByReference"/></c>, this is a member that contains the property's display name.<br/>
        /// The member must be a field, property (with Get method) or method (with only optional or params parameters) that returns <see cref="string"/>.</param>
        /// <param name="displayNameMode">Determines how <paramref name="displayName"/> is interpreted.</param>
        /// <param name="tooltip">If <c><paramref name="tooltipMode"/> == <see cref="LabelMode.ByValue"/></c>, this is the property's tooltip.<br/>
        /// If <c><paramref name="tooltipMode"/> == <see cref="LabelMode.ByReference"/></c>, this is a member that contains the property's tooltip.<br/>
        /// The member must be a field, property (with Get method) or method (with only optional or params parameters) that returns <see cref="string"/>.</param>
        /// <param name="tooltipMode">Determines how <paramref name="tooltip"/> is interpreted.</param>
        public LabelAttribute(string displayName, LabelMode displayNameMode, string tooltip, LabelMode tooltipMode = LabelMode.ByValue)
        {
            DisplayNameOrGuiContent = displayName;
            DisplayNameMode = displayNameMode;
            Tooltip = tooltip;
            TooltipMode = tooltipMode;
        }
    }
}