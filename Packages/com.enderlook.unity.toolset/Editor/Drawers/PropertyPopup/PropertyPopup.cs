using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;
using System.Threading;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    /// <summary>
    /// A helper class to draw properties according to a popup selector.
    /// </summary>
    internal sealed class PropertyPopup
    {
        private const string NOT_FOUND_OPTION = "Not found an option which satisfy {0} ({1}).";

        private static readonly GUIStyle popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
        {
            imagePosition = ImagePosition.ImageOnly
        };

        private static GUIContent tmpContent;

        private readonly string modeProperty;
        private readonly PropertyPopupOption[] modes;
        private readonly string[] popupOptions;

        /// <summary>
        /// Determie the posible options for the popup.
        /// </summary>
        /// <param name="modeProperty">Property used to determine which property draw.</param>
        /// <param name="modes">Possible options. (name to show in inspector, name of property which must show if selected, target value to determine if chosen).</param>
        public PropertyPopup(string modeProperty, params PropertyPopupOption[] modes)
        {
            this.modeProperty = modeProperty;
            this.modes = modes;
            popupOptions = new string[modes.Length];
            for (int i = 0; i < modes.Length; i++)
                popupOptions[i] = modes[i].DisplayName;
        }

        /// <summary>
        /// Draw the field in the given place.
        /// </summary>
        /// <param name="position">Position to draw the field.</param>
        /// <param name="property">Property used to draw the field.</param>
        /// <param name="label">Label to show in inspector.</param>
        public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            (SerializedProperty mode, int popupIndex) = GetModeAndIndex(property);

            // Show field label
            Rect newPosition = EditorGUI.PrefixLabel(position, label);

            // Calculate rect for configuration button
            //float labelWidth = GUI.skin.label.CalcSize(label).x;
            Rect buttonRect = new Rect(
                newPosition.x,
                newPosition.y + popupStyle.margin.top,
                popupStyle.fixedWidth + popupStyle.margin.right,
                newPosition.height - popupStyle.margin.top);

            newPosition.xMin += buttonRect.width;

            int newUsagePopupIndex = EditorGUI.Popup(buttonRect, popupIndex, popupOptions, popupStyle);
            if (newUsagePopupIndex != popupIndex)
            {
                object value = modes[newUsagePopupIndex].Target;
                mode.SetValue(value);
                mode.serializedObject.ApplyModifiedProperties();
            }

            if (newUsagePopupIndex != -1)
            {
                PropertyPopupOption propertyPopupOption = modes[newUsagePopupIndex];

                SerializedProperty optionProperty = property.FindPropertyRelative(propertyPopupOption.PropertyName);
                if (IsLargerThanOneLine(optionProperty))
                    EditorGUI.PropertyField(position, optionProperty, GUIContent.none, true);
                else
                    EditorGUI.PropertyField(newPosition, optionProperty, GUIContent.none, true);
            }
            else
                EditorGUI.HelpBox(newPosition, string.Format(NOT_FOUND_OPTION, mode.propertyPath, GetValue(mode)), MessageType.Error);
        }

        private static bool IsLargerThanOneLine(SerializedProperty optionProperty)
        {
            bool isExpanded = optionProperty.isExpanded;
            GUIContent label = optionProperty.GetGUIContentNotThrow();

            optionProperty.isExpanded = false;
            float nonExpanded = EditorGUI.GetPropertyHeight(optionProperty, label, true);
            optionProperty.isExpanded = true;
            float expanded = EditorGUI.GetPropertyHeight(optionProperty, label, true);

            optionProperty.isExpanded = isExpanded;
            return nonExpanded != expanded;
        }

        private (SerializedProperty mode, int index) GetModeAndIndex(SerializedProperty property)
        {
            // Get current mode
            SerializedProperty mode = property.FindPropertyRelative(modeProperty);
            if (mode is null)
                Throw();
            int popupIndex = GetPopupIndex(mode);
            return (mode, popupIndex);

            void Throw() => throw new ArgumentNullException(nameof(mode), $"Can't find propety {mode.name} at path {mode.propertyPath} in {property.name}.");
        }

        private int GetPopupIndex(SerializedProperty mode)
        {
            object value = GetValue(mode);

            for (int modeIndex = 0; modeIndex < modes.Length; modeIndex++)
                if (modes[modeIndex].Target.Equals(value))
                    return modeIndex;

            Debug.LogError(string.Format(NOT_FOUND_OPTION, mode.propertyPath, value));
            return -1;
        }

        private static object GetValue(SerializedProperty mode)
        {
            if (mode.TryGetTargetObject(out object value))
            {
                // We give special treat with enums
                Type type = value.GetType();
                if (type.IsEnum)
                    value = Convert.ChangeType(value, Enum.GetUnderlyingType(type));
                return value;
            }
            return null;
        }

        /// <summary>
        /// Get the height of the drawed property.
        /// </summary>
        /// <param name="property">Property used to draw the field.</param>
        /// <param name="label">Label to show in inspector.</param>
        /// <returns>Property height.</returns>
        public float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty mode = property.FindPropertyRelative(modeProperty);
            int popupIndex = GetPopupIndex(mode);
            return popupIndex == -1 ? NotFound() : GetPropertyHeight();

            float NotFound()
            {
                GUIContent guiContent = Interlocked.Exchange(ref tmpContent, null) ?? new GUIContent();
                guiContent.text = string.Format(NOT_FOUND_OPTION, mode.propertyPath, mode.GetValue());
                float width = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - popupStyle.fixedWidth - popupStyle.margin.right;
                float height = GUI.skin.box.CalcHeight(guiContent, width);
                guiContent.text = null;
                tmpContent = guiContent;
                return height;
            }

            float GetPropertyHeight()
            {
                PropertyPopupOption propertyPopupOption = modes[popupIndex];
                SerializedProperty choosenProperty = property.FindPropertyRelative(propertyPopupOption.PropertyName);
                float height = EditorGUI.GetPropertyHeight(property, label, false);
                if (IsLargerThanOneLine(choosenProperty) && choosenProperty.isExpanded)
                    height += EditorGUI.GetPropertyHeight(choosenProperty, choosenProperty.GetGUIContent(), true);
                return height;
            }
        }
    }
}
