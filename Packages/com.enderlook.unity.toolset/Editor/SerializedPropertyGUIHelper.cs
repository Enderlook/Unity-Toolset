using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset
{
    /// <summary>
    /// A helper class to manage <see cref="GUIAttribute"/> and <see cref="NameAttribute"/>.
    /// </summary>
    internal static class SerializedPropertyGUIHelper
    {
        private static void UseNameAttribute(NameAttribute attribute, GUIContent label) => label.text = attribute.name;

        private static void GetGUIContent(GUIAttribute attribute, SerializedPropertyHelper helper, ref GUIContent label)
        {
            string text = null, tooltip = null;

            if (attribute.guiContentOrReferenceName == null)
            {
                bool reference = false;
                if (attribute.nameMode == GUIAttribute.Mode.Value)
                    text = attribute.name;
                else
                    reference = true;

                if (attribute.tooltipMode == GUIAttribute.Mode.Value)
                    tooltip = attribute.tooltip;
                else
                    reference = true;

                if (reference && helper.TryGetParentTargetObjectOfProperty(out object parent))
                {
                    if (attribute.nameMode == GUIAttribute.Mode.Reference)
                        text = parent.GetValueFromFirstMember<string>(attribute.name);
                    if (attribute.tooltipMode == GUIAttribute.Mode.Reference)
                        tooltip = parent.GetValueFromFirstMember<string>(attribute.tooltip);
                }

                if (!(text is null))
                    label.text = text;

                if (!(tooltip is null))
                    label.tooltip = tooltip;
            }
            else if (helper.TryGetParentTargetObjectOfProperty(out object parent))
                try
                {
                    label = parent.GetValueFromFirstMember<GUIContent>(attribute.guiContentOrReferenceName);
                }
                catch (MatchingMemberNotFoundException)
                {
                    text = parent.GetValueFromFirstMember<string>(attribute.guiContentOrReferenceName);
                    label.text = text;
                }
        }

        /// <summary>
        /// Check if the <see cref="SerializedProperty"/> does have a <see cref="GUIAttribute"/> <see cref="System.Attribute"/> and if has change <paramref name="label"/> by its <see cref="GUIContent"/>.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="label">Current <see cref="GUIContent"/>.</param>
        /// <returns>Whenever there was or not an special <see cref="GUIContent"/>.</returns>
        public static bool GetGUIContent(SerializedPropertyHelper helper, ref GUIContent label)
        {
            bool isSpecial = false;

            if (helper.TryGetAttributeFromField(out NameAttribute nameAttribute))
            {
                UseNameAttribute(nameAttribute, label);
                isSpecial = true;
            }

            if (helper.TryGetAttributeFromField(out GUIAttribute guiAttribute))
            {
                GetGUIContent(guiAttribute, helper, ref label);
                isSpecial = true;
            }

            return isSpecial;
        }

        /// <summary>
        /// Check if the <see cref="SerializedProperty"/> does have a <see cref="GUIAttribute"/> <see cref="System.Attribute"/> and if has change <paramref name="label"/> by its <see cref="GUIContent"/>.
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <param name="label">Current <see cref="GUIContent"/>.</param>
        /// <returns>Whenever there was or not an special <see cref="GUIContent"/>.</returns>
        public static bool GetGUIContent(SerializedProperty serializedProperty, ref GUIContent label)
        {
            SerializedPropertyHelper helper = serializedProperty.GetHelper();
            return GetGUIContent(helper, ref label);
        }
    }
}