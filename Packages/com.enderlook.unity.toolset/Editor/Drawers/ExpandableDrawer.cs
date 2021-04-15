using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;
using Enderlook.Unity.Toolset.Windows;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    //[CustomPropertyDrawer(typeof(UnityEngine.Object), true)] // Will affect all objects
    //[CustomPropertyDrawer(typeof(ScriptableObject), true)] // Will only affect scriptable objects
    [CustomPropertyDrawer(typeof(ExpandableAttribute), true)]
    internal sealed class ExpandableDrawer : SmartPropertyDrawer
    {
        /// <summary>
        /// How button to open in window must be shown.
        /// </summary>
        private enum ButtonDisplayMode
        {
            /// <summary>
            /// No button is show.
            /// </summary>
            None,

            /// <summary>
            /// The button is show in the same line.
            /// </summary>
            Inline,

            /// <summary>
            /// The button is shown inside the foldout.
            /// </summary>
            Foldout,
        }

        /// <summary>
        /// How button is show.
        /// </summary>
        private const ButtonDisplayMode SHOW_OPEN_IN_WINDOW_BUTTON = ButtonDisplayMode.Inline;

        /// <summary>
        /// Whenever the script field type must be shown on foldout.
        /// </summary>
        private const bool SHOW_SCRIPT_FIELD = true;

        /// <summary>
        /// Whenever the foldout has an outline or not.
        /// </summary>
        private const bool HAS_OUTLINE = true;

        /// <summary>
        /// Determines the color applied to the foldout.
        /// </summary>
        private enum FoldoutColor
        {
            /// <summary>
            /// Apply background color.
            /// </summary>
            None,

            /// <summary>
            /// Apply a dark color.
            /// </summary>
            Dark,

            /// <summary>
            /// Apply a dark color which becomes darker when nested.
            /// </summary>
            Darker,

            /// <summary>
            /// Apply a light color.
            /// </summary>
            Light,

            /// <summary>
            /// Apply a light color which becomes darker when nested.
            /// </summary>
            Lighten,

            /// <summary>
            /// Apply a help box style.
            /// </summary>
            HelpBox,
        }

        /// <summary>
        /// Determines the foldout color
        /// </summary>
        private const FoldoutColor HAS_FOLDOUT_COLOR = FoldoutColor.Darker;

        /// <summary>
        /// Determines the color on <see cref="FoldoutColor.Dark"/>
        /// </summary>
        private static readonly Color DARK_FOLDOUT_COLOR = new Color(0, 0, 0, .2f);

        /// <summary>
        /// Determines the color on <see cref="FoldoutColor.Light"/>
        /// </summary>
        private static readonly Color LIGHT_FOLDOUT_COLOR = new Color(1, 1, 1, .2f);

        /// <summary>
        /// Determines how color is multplied on <see cref="FoldoutColor.Darker"/>.
        /// </summary>
        private const float DARKER_MULTIPLIER = 0.9f;

        /// <summary>
        /// Determines how color is multplied on <see cref="FoldoutColor.Lighten"/>.
        /// </summary>
        private const float LIGHTEN_MULTIPLIER = 1.1f;

        private const int INDENT_WIDTH = 8; // TODO: This is wrong.

        /// <summary>
        /// Determines spacing amount of space inside the foldout.
        /// </summary>
        private const float INNER_SPACING = 4;

        /// <summary>
        /// Determines spacing amount of space outside the foldout.
        /// </summary>
        private const float OUTER_SPACING = 1.5f;

        private static readonly GUIContent OPEN_IN_NEW_WINDOW_BUTTON = new GUIContent("Open in Window", "Open a window to edit this content.");

        private static readonly GUIStyle inlineButtonStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
        {
            imagePosition = ImagePosition.ImageOnly
        };

#pragma warning disable CS0162
        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            Type type = property.serializedObject.targetObject.GetType();
            if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                Debug.LogError($"{nameof(ExpandableAttribute)} can only be used on types subclasses of {nameof(UnityEngine.Object)}. {property.name} from {property.GetParentTargetObjectOfProperty()} (path {property.propertyPath}) is type {type}.");
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
                return;
            }

            Rect fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            if (SHOW_OPEN_IN_WINDOW_BUTTON == ButtonDisplayMode.Inline)
            {
                // Show field label
                Rect newPosition = EditorGUI.PrefixLabel(fieldRect, label);

                // Calculate rect for button
                Rect buttonRect = new Rect(
                    newPosition.x,
                    newPosition.y + inlineButtonStyle.margin.top,
                    inlineButtonStyle.fixedWidth + inlineButtonStyle.margin.right - (EditorGUI.indentLevel * inlineButtonStyle.fixedWidth),
                    newPosition.height - inlineButtonStyle.margin.top
                );

                // Add button
                GUI.enabled = property.objectReferenceValue != null;
                if (GUI.Button(buttonRect, GUIContent.none, inlineButtonStyle))
                    ExpandableWindow.CreateWindow(property);
                GUI.enabled = true;

                newPosition.xMin += buttonRect.width;
                newPosition.width -= buttonRect.width;

                // Add property
                EditorGUI.PropertyField(newPosition, property, GUIContent.none, true);
            }
            else
                EditorGUI.PropertyField(fieldRect, property, label, true);

            if (property.objectReferenceValue == null)
                return;

            property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, GUIContent.none, true);

            if (!property.isExpanded)
                return;

            SerializedObject targetObject = new SerializedObject(property.objectReferenceValue);

            if (targetObject == null)
                return;

            SerializedProperty field = targetObject.GetIterator();
            field.NextVisible(true);
            
            fieldRect.x += INDENT_WIDTH;
            fieldRect.width -= INDENT_WIDTH;
            fieldRect.y += INNER_SPACING + OUTER_SPACING;

            Rect box = position;
            // Calculate container box
            if (HAS_OUTLINE || HAS_FOLDOUT_COLOR != FoldoutColor.None)
            {
                box.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;// + OUTER_SPACING; // That may be added
                box.height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                box.height += INNER_SPACING;
                box.height -= OUTER_SPACING * 3; // If problems, this value should be 2
                box.width += INDENT_WIDTH * 2;
                box.x -= INDENT_WIDTH;
            }

            if (HAS_OUTLINE)
                GUI.Box(box, GUIContent.none);

            Color backgroundColorOld = GUI.backgroundColor;
            switch (HAS_FOLDOUT_COLOR)
            {
                case FoldoutColor.Dark:
                    EditorGUI.DrawRect(box, DARK_FOLDOUT_COLOR);
                    break;
                case FoldoutColor.Light:
                    EditorGUI.DrawRect(box, LIGHT_FOLDOUT_COLOR);
                    break;
                case FoldoutColor.HelpBox:
                    EditorGUI.HelpBox(box, "", MessageType.None);
                    break;
                case FoldoutColor.Darker:
                    GUI.backgroundColor = new Color(
                        GUI.backgroundColor.r * DARKER_MULTIPLIER,
                        GUI.backgroundColor.g * DARKER_MULTIPLIER,
                        GUI.backgroundColor.b * DARKER_MULTIPLIER,
                        GUI.backgroundColor.a
                    );
                    break;
                case FoldoutColor.Lighten:
                    GUI.backgroundColor = new Color(
                        GUI.backgroundColor.r * LIGHTEN_MULTIPLIER,
                        GUI.backgroundColor.g * LIGHTEN_MULTIPLIER,
                        GUI.backgroundColor.b * LIGHTEN_MULTIPLIER,
                        GUI.backgroundColor.a
                    );
                    break;
            }

            if (SHOW_OPEN_IN_WINDOW_BUTTON == ButtonDisplayMode.Foldout)
            {
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (GUI.Button(fieldRect, OPEN_IN_NEW_WINDOW_BUTTON))
                    ExpandableWindow.CreateWindow(property);
            }

            if (SHOW_SCRIPT_FIELD)
            {
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(fieldRect, field, true);
                EditorGUI.EndDisabledGroup();
            }

            while (field.NextVisible(false))
            {
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                float totalHeight = EditorGUI.GetPropertyHeight(field, true);
                fieldRect.height = totalHeight;

                try
                {
                    EditorGUI.PropertyField(fieldRect, field, true);
                }
                catch (StackOverflowException)
                {
                    field.objectReferenceValue = null;
                    Debug.LogError("Detected self-nesting which caused StackOverFlowException. Avoid circular reference in nested objects.");
                }

                // TODO: maybe this could be done more efficiently.
                if (field.isExpanded)
                    fieldRect.y += totalHeight - EditorGUI.GetPropertyHeight(field, false);
            }

            GUI.backgroundColor = backgroundColorOld;

            if (GUI.changed)
                targetObject.ApplyModifiedProperties();
        }

        protected override float GetPropertyHeightSmart(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (property.objectReferenceValue == null)
                return totalHeight;

            if (!property.isExpanded)
                return totalHeight;

            totalHeight += (INNER_SPACING * 2) + (OUTER_SPACING * 2);

            SerializedObject targetObject = new SerializedObject(property.objectReferenceValue);

            if (targetObject == null)
                return totalHeight;

            SerializedProperty field = targetObject.GetIterator();

            field.NextVisible(true);

            if (SHOW_OPEN_IN_WINDOW_BUTTON == ButtonDisplayMode.Foldout)
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (SHOW_SCRIPT_FIELD)
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            while (field.NextVisible(false))
                totalHeight += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;

            return totalHeight;
        }
#pragma warning restore CS162
    }
}