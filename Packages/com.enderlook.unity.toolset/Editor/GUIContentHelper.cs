using Enderlook.Reflection;
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
        private static GUIContent staticContent;

        internal static void UseNameAttribute(NameAttribute attribute, GUIContent label) => label.text = attribute.name;

        internal static void UseGUIContent(GUIAttribute attribute, SerializedProperty property, ref GUIContent label)
        {
            string text;
            if (attribute.GuiContentOrReferenceName is null)
            {
                string tooltip;
                if (attribute.NameMode == GUIMode.Value)
                {
                    text = attribute.Name;

                    if (attribute.TooltipMode == GUIMode.Value)
                        tooltip = attribute.Tooltip;
                    else
                        tooltip = property.GetParentTargetObject().GetValueFromFirstMember<string>(attribute.Tooltip, true);
                }
                else
                {
                    object parent = property.GetParentTargetObject();
                    text = parent.GetValueFromFirstMember<string>(attribute.Name, true);

                    if (attribute.TooltipMode == GUIMode.Value)
                        tooltip = attribute.Tooltip;
                    else
                        tooltip = parent.GetValueFromFirstMember<string>(attribute.Tooltip, true);
                }

                if (!(text is null))
                    label.text = text;

                if (!(tooltip is null))
                    label.tooltip = tooltip;
            }
            else
            {
                object parent = property.GetParentTargetObject();
                try
                {
                    label = parent.GetValueFromFirstMember<GUIContent>(attribute.GuiContentOrReferenceName);
                }
                catch (MatchingMemberNotFoundException)
                {
                    text = parent.GetValueFromFirstMember<string>(attribute.GuiContentOrReferenceName);
                    label.text = text;
                }
            }
        }

        private static void SetGUIContent(SerializedProperty property, ref GUIContent content)
        {
            content.text = property.displayName;
            content.tooltip = property.tooltip;

            MemberInfo memberInfo = property.GetMemberInfo();
            NameAttribute nameAttribute = memberInfo.GetCustomAttribute<NameAttribute>(true);
            GUIAttribute guiAttribute = memberInfo.GetCustomAttribute<GUIAttribute>(true);

            if (!(nameAttribute is null))
            {
                if (!(guiAttribute is null))
                {
                    if (nameAttribute.order >= guiAttribute.order)
                    {
                        UseNameAttribute(nameAttribute, content);
                        UseGUIContent(guiAttribute, property, ref content);
                    }
                    else
                    {
                        UseGUIContent(guiAttribute, property, ref content);
                        UseNameAttribute(nameAttribute, content);
                    }
                }
                else
                    UseNameAttribute(nameAttribute, content);
            }
            else if (!(guiAttribute is null))
                UseGUIContent(guiAttribute, property, ref content);
        }

        /// <summary>
        /// Produce a <see cref="GUIContent"/> with label of <paramref name="serializedProperty"/>.
        /// </summary>
        /// <param name="serializedProperty">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns><see cref="GUIContent"/> of <see cref="property"/>.</returns>
        public static GUIContent GetGUIContent(this SerializedProperty serializedProperty)
        {
            GUIContent content = new GUIContent(serializedProperty.name, serializedProperty.tooltip);
            SetGUIContent(serializedProperty, ref content);
            return content;
        }

        /// <summary>
        /// Get the display name of <paramref name="serializedProperty"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns>Display name of <see cref="serializedProperty"/>.</returns>
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
        /// Get the tooltip of <paramref name="serializedProperty"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns>Tooltip of <see cref="serializedProperty"/>.</returns>
        public static string GetTooltip(this SerializedProperty property)
        {
            if (property.GetMemberInfo().GetCustomAttribute<GUIAttribute>() is GUIAttribute attribute)
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