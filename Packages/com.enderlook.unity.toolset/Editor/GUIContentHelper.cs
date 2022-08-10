using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System.Reflection;
using System.Threading;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset
{
    /// <summary>
    /// Helper methods to create <see cref="GUIContent"/> from <see cref="SerializedProperty"/>
    /// </summary>
    public static class GUIContentHelper
    {
        private const ExhaustiveBindingFlags BindingFlags = ExhaustiveBindingFlags.Instance | ExhaustiveBindingFlags.Static;
        private static GUIContent staticContent;

        internal static void UseGUIContent(LabelAttribute attribute, SerializedProperty property, ref GUIContent label)
        {
            if (attribute.DisplayNameMode == LabelMode.ByValue)
            {
                label.text = attribute.DisplayNameOrGuiContent;

                if (attribute.TooltipMode == LabelMode.ByValue)
                    label.tooltip = attribute.Tooltip;
                else
                    label.tooltip = property.GetParentTargetObject().GetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive<string>(attribute.Tooltip, BindingFlags);
            }
            else
            {
                object parent = property.GetParentTargetObject();
                if (attribute.Tooltip is null)
                {
                    try
                    {
                        label.text = parent.GetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive<string>(attribute.DisplayNameOrGuiContent, BindingFlags);
                    }
                    catch (MatchingMemberNotFoundException)
                    {
                        label = parent.GetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive<GUIContent>(attribute.DisplayNameOrGuiContent, BindingFlags);
                    }
                }
                else
                {
                    label.text = parent.GetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive<string>(attribute.DisplayNameOrGuiContent, BindingFlags);

                    if (attribute.TooltipMode == LabelMode.ByValue)
                        label.tooltip = attribute.Tooltip;
                    else
                        label.tooltip = parent.GetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive<string>(attribute.Tooltip, BindingFlags);
                }
            }
        }

        private static void SetGUIContent(SerializedProperty property, ref GUIContent content)
        {
            content.text = property.displayName;
            content.tooltip = property.tooltip;

            LabelAttribute guiAttribute = property.GetMemberInfo().GetCustomAttribute<LabelAttribute>(true);

            if (!(guiAttribute is null))
                UseGUIContent(guiAttribute, property, ref content);
        }

        /// <summary>
        /// Produce a <see cref="GUIContent"/> with label of <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns><see cref="GUIContent"/> of <paramref name="property"/>.</returns>
        public static GUIContent GetGUIContent(this SerializedProperty property)
        {
            GUIContent content = new GUIContent(property.name, property.tooltip);
            SetGUIContent(property, ref content);
            return content;
        }

        /// <summary>
        /// Get the display name of <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns>Display name of <paramref name="property"/>.</returns>
        public static string GetDisplayName(this SerializedProperty property)
        {
            GUIContent content = Interlocked.Exchange(ref staticContent, null) ?? new GUIContent();
            SetGUIContent(property, ref content);
            string name = content.text;
            content.text = null;
            content.tooltip = null;
            staticContent = content;
            return name;
        }

        /// <summary>
        /// Get the tooltip of <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns>Tooltip of <paramref name="property"/>.</returns>
        public static string GetTooltip(this SerializedProperty property)
        {
            if (property.GetMemberInfo().GetCustomAttribute<LabelAttribute>() is LabelAttribute attribute)
            {
                GUIContent content = Interlocked.Exchange(ref staticContent, null) ?? new GUIContent();
                UseGUIContent(attribute, property, ref content);
                string tooltip = content.tooltip;
                content.text = null;
                content.tooltip = null;
                staticContent = content;
                return tooltip;
            }
            return property.tooltip;
        }
    }
}