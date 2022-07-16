﻿using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset
{
    /// <summary>
    /// Helper methods to create <see cref="GUIContent"/> from <see cref="SerializedProperty"/>
    /// </summary>
    public static class GUIContentHelper
    {
        internal static void UseNameAttribute(NameAttribute attribute, GUIContent label) => label.text = attribute.name;

        internal static void UseGUIContent(GUIAttribute attribute, SerializedProperty property, ref GUIContent label)
        {
            string text;
            if (attribute.guiContentOrReferenceName is null)
            {
                string tooltip;
                if (attribute.nameMode == GUIMode.Value)
                {
                    text = attribute.name;

                    if (attribute.tooltipMode == GUIMode.Value)
                        tooltip = attribute.tooltip;
                    else
                        tooltip = property.GetParentTargetObject().GetValueFromFirstMember<string>(attribute.tooltip, true);
                }
                else
                {
                    object parent = property.GetParentTargetObject();
                    text = parent.GetValueFromFirstMember<string>(attribute.name, true);

                    if (attribute.tooltipMode == GUIMode.Value)
                        tooltip = attribute.tooltip;
                    else
                        tooltip = parent.GetValueFromFirstMember<string>(attribute.tooltip, true);
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
                    label = parent.GetValueFromFirstMember<GUIContent>(attribute.guiContentOrReferenceName);
                }
                catch (MatchingMemberNotFoundException)
                {
                    text = parent.GetValueFromFirstMember<string>(attribute.guiContentOrReferenceName);
                    label.text = text;
                }
            }
        }

        /// <summary>
        /// Check if the <see cref="SerializedProperty"/> does have a <see cref="GUIAttribute"/> <see cref="System.Attribute"/> and if has change <paramref name="label"/> by its <see cref="GUIContent"/>.
        /// </summary>
        /// <param name="property">Serialized property whose label is being modified.</param>
        /// <param name="label">Current <see cref="GUIContent"/>.</param>
        /// <returns>Whenever there was or not an special <see cref="GUIContent"/>.</returns>
        internal static bool GetGUIContent(SerializedProperty property, ref GUIContent label)
        {
            bool isSpecial = false;
            MemberInfo memberInfo = property.GetMemberInfo();

            NameAttribute nameAttribute = memberInfo.GetCustomAttribute<NameAttribute>(true);
            if (!(nameAttribute is null))
            {
                UseNameAttribute(nameAttribute, label);
                isSpecial = true;
            }

            GUIAttribute guiAttribute = memberInfo.GetCustomAttribute<GUIAttribute>(true);
            if (!(guiAttribute is null))
            {
                UseGUIContent(guiAttribute, property, ref label);
                isSpecial = true;
            }

            return isSpecial;
        }

        /// <summary>
        /// Produce a <see cref="GUIContent"/> with the <see cref="SerializedProperty.displayName"/> as <see cref="GUIContent.text"/> and <see cref="SerializedProperty.tooltip"/> as <see cref="GUIContent.tooltip"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns><see cref="GUIContent"/> of <see cref="serializedProperty"/>.</returns>
        public static GUIContent GetGUIContent(this SerializedProperty property)
        {
            GUIContent guiContent = new GUIContent(property.displayName, property.tooltip);
            GetGUIContent(property, ref guiContent);
            return guiContent;
        }
    }
}