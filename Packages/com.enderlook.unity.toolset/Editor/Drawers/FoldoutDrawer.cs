using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;
using Enderlook.Unity.Toolset.Windows;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    internal abstract class FoldoutDrawer : StackablePropertyDrawer
    {
        /// <summary>
        /// How button to open in window must be shown.
        /// </summary>
        protected enum ButtonDisplayMode
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
            /// The button is shown inside the box.
            /// </summary>
            Box,
        }

        /// <summary>
        /// Determines the color applied to the foldout.
        /// </summary>
        protected enum BoxColorMode
        {
            /// <summary>
            /// Apply no background color.
            /// </summary>
            None,

            /// <summary>
            /// Apply box but without color.
            /// </summary>
            ColorlessOutline,

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
        /// Determines if it should draw the property.
        /// </summary>
        protected enum DrawMode
        {
            /// <summary>
            /// Draw the property without the attribute.
            /// </summary>
            DrawSimpleField,
            /// <summary>
            /// Draw the property using the attribute.
            /// </summary>
            DrawComplexField,
            /// <summary>
            /// Only draw the error message.
            /// </summary>
            DrawError,
        }

        /// <summary>
        /// Determines if has a foldout.<br/>
        /// If <see langword="false"/>, property is always expanded.
        /// </summary>
        protected abstract bool HAS_FOLDOUT { get; }

        /// <summary>
        /// Determines if field should be shown.
        /// </summary>
        protected abstract bool SHOW_FIELD { get; }

        /// <summary>
        /// How button is show.
        /// </summary>
        protected abstract ButtonDisplayMode SHOW_OPEN_IN_WINDOW_BUTTON { get; }

        /// <summary>
        /// Whenever the script field type must be shown on foldout.
        /// </summary>
        protected abstract bool SHOW_SCRIPT_FIELD { get; }

        /// <summary>
        /// Determines the foldout color
        /// </summary>
        protected abstract BoxColorMode BOX_COLOR { get; }

        /// <summary>
        /// Determines the color on <see cref="BoxColorMode.Dark"/>
        /// </summary>
        private static readonly Color DARK_COLOR = new Color(0, 0, 0, .2f);

        /// <summary>
        /// Determines the color on <see cref="BoxColorMode.Light"/>
        /// </summary>
        private static readonly Color LIGHT_COLOR = new Color(1, 1, 1, .2f);

        /// <summary>
        /// Determines how color is multplied on <see cref="BoxColorMode.Darker"/>.
        /// </summary>
        private const float DARKER_MULTIPLIER = 0.9f;

        /// <summary>
        /// Determines how color is multplied on <see cref="BoxColorMode.Lighten"/>.
        /// </summary>
        private const float LIGHTER_MULTIPLIER = 1.1f;

        /// <summary>
        /// Determines the identation of content of field.
        /// </summary>
        protected abstract int INDENT_WIDTH { get; }

        /// <summary>
        /// Determines spacing amount of space inside the foldout.
        /// </summary>
        private const float INNER_SPACING = 4;

        /// <summary>
        /// Determines spacing amount of space outside the foldout.
        /// </summary>
        private const float OUTER_SPACING = 1.5f;

        private static readonly GUIContent OPEN_IN_NEW_WINDOW_BUTTON = new GUIContent("Open in Window", "Open a window to edit this content.");

        private static readonly GUIStyle INLINE_BUTTON_STYLE = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
        {
            imagePosition = ImagePosition.ImageOnly
        };

        protected internal override bool RequestMain => true;

        protected abstract DrawMode CanDraw(SerializedProperty property, bool log, out string textBoxMessage);

        protected internal sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            switch (CanDraw(property, true, out string messageError))
            {
                case DrawMode.DrawSimpleField:
                    EditorGUI.PropertyField(position, property, label, includeChildren);
                    return;
                case DrawMode.DrawError:
                    EditorGUI.HelpBox(position, messageError, MessageType.Error);
                    return;
            }

            Rect fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            if (SHOW_FIELD)
            {
                if (SHOW_OPEN_IN_WINDOW_BUTTON == ButtonDisplayMode.Inline)
                {
                    // Show field label
                    Rect newPosition = EditorGUI.PrefixLabel(fieldRect, label);

                    // Calculate rect for button
                    Rect buttonRect = new Rect(
                        newPosition.x,
                        newPosition.y + INLINE_BUTTON_STYLE.margin.top,
                        INLINE_BUTTON_STYLE.fixedWidth + INLINE_BUTTON_STYLE.margin.right - (EditorGUI.indentLevel * INLINE_BUTTON_STYLE.fixedWidth),
                        newPosition.height - INLINE_BUTTON_STYLE.margin.top
                    );

                    // Add button
                    GUI.enabled = property.objectReferenceValue != null;
                    if (GUI.Button(buttonRect, GUIContent.none, INLINE_BUTTON_STYLE))
                        ExpandableWindow.CreateWindow(property);
                    GUI.enabled = true;

                    newPosition.xMin += buttonRect.width;
                    newPosition.width -= buttonRect.width;

                    // Add property
                    EditorGUI.PropertyField(newPosition, property, GUIContent.none, true);
                }
                else
                    EditorGUI.PropertyField(fieldRect, property, label, true);
            }
            else
                fieldRect.y -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                return;

            if (HAS_FOLDOUT)
                property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, GUIContent.none, true);
            else
                property.isExpanded = true;

            if (!property.isExpanded)
                return;

            SerializedObject targetObject;
            SerializedProperty field;
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                targetObject = new SerializedObject(property.objectReferenceValue);

                if (targetObject == null)
                    return;

                field = targetObject.GetIterator();
            }
            else
            {
                targetObject = property.serializedObject;
                field = property;
            }

            EditorGUI.BeginChangeCheck();

            if (BOX_COLOR != BoxColorMode.None)
            {
                fieldRect.x += INDENT_WIDTH;
                fieldRect.width -= INDENT_WIDTH;
                fieldRect.y += INNER_SPACING + OUTER_SPACING;
            }

            Rect box = position;
            // Calculate container box
            if (BOX_COLOR != BoxColorMode.None)
            {
                box.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;// + OUTER_SPACING; // That may be added
                box.height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                box.height += INNER_SPACING;
                box.height -= OUTER_SPACING * 3; // If problems, this value should be 2
                box.width += INDENT_WIDTH * 2;
                box.x -= INDENT_WIDTH;
                GUI.Box(box, GUIContent.none);
            }

            Color backgroundColorOld = GUI.backgroundColor;
            switch (BOX_COLOR)
            {
                case BoxColorMode.Dark:
                    EditorGUI.DrawRect(box, DARK_COLOR);
                    break;
                case BoxColorMode.Light:
                    EditorGUI.DrawRect(box, LIGHT_COLOR);
                    break;
                case BoxColorMode.HelpBox:
                    EditorGUI.HelpBox(box, "", MessageType.None);
                    break;
                case BoxColorMode.Darker:
                    GUI.backgroundColor = new Color(
                        GUI.backgroundColor.r * DARKER_MULTIPLIER,
                        GUI.backgroundColor.g * DARKER_MULTIPLIER,
                        GUI.backgroundColor.b * DARKER_MULTIPLIER,
                        GUI.backgroundColor.a
                    );
                    break;
                case BoxColorMode.Lighten:
                    GUI.backgroundColor = new Color(
                        GUI.backgroundColor.r * LIGHTER_MULTIPLIER,
                        GUI.backgroundColor.g * LIGHTER_MULTIPLIER,
                        GUI.backgroundColor.b * LIGHTER_MULTIPLIER,
                        GUI.backgroundColor.a
                    );
                    break;
            }

            if (SHOW_OPEN_IN_WINDOW_BUTTON == ButtonDisplayMode.Box)
            {
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (GUI.Button(fieldRect, OPEN_IN_NEW_WINDOW_BUTTON))
                    ExpandableWindow.CreateWindow(property);
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
            }

            if (SHOW_SCRIPT_FIELD && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUIHelper.DrawScriptField(fieldRect, field.serializedObject.targetObject);
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
            }

            float previousHeigth = EditorGUIUtility.singleLineHeight;
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                field.NextVisible(true);
                while (field.NextVisible(false))
                    DrawNested(ref fieldRect);
            }
            else
            {
                string path = field.propertyPath;
                field.NextVisible(true);
                do
                {
                    if (!field.propertyPath.StartsWith(path))
                        break;

                    DrawNested(ref fieldRect);
                }
                while (field.NextVisible(false));
            }

            GUI.backgroundColor = backgroundColorOld;

            if (EditorGUI.EndChangeCheck())
                targetObject.ApplyModifiedProperties();

            void DrawNested(ref Rect fieldRect_)
            {
                fieldRect_.y += previousHeigth + EditorGUIUtility.standardVerticalSpacing;
                float totalHeight = EditorGUI.GetPropertyHeight(field, true);
                fieldRect_.height = previousHeigth = totalHeight;

                try
                {
                    EditorGUI.PropertyField(fieldRect_, field, true);
                }
                catch (StackOverflowException)
                {
                    field.objectReferenceValue = null;
                    Debug.LogError("Detected self-nesting which caused StackOverFlowException. Avoid circular reference in nested objects.");
                }

                // TODO: maybe this could be done more efficiently.
                if (field.isExpanded)
                    fieldRect_.y += totalHeight - EditorGUI.GetPropertyHeight(field, false);
            }
        }

        protected internal sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            switch (CanDraw(property, false, out string messageError))
            {
                case DrawMode.DrawSimpleField:
                    return base.GetPropertyHeight(property, label, includeChildren);
                case DrawMode.DrawError:
                    float height;
                    GUIContent gui = GUIContentHelper.RentGUIContent();
                    {
                        gui.text = messageError;
                        height = GUI.skin.box.CalcHeight(gui, EditorGUIUtility.currentViewWidth);
                    }
                    GUIContentHelper.ReturnGUIContent(gui);
                    return height;
            }

            float totalHeight = SHOW_FIELD ? EditorGUIUtility.singleLineHeight : 0;

            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                goto end;

            if (!property.isExpanded)
                goto end;

            if (BOX_COLOR != BoxColorMode.None)
                totalHeight += (INNER_SPACING * 2) + (OUTER_SPACING * 2);

            SerializedProperty field;
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                SerializedObject targetObject = new SerializedObject(property.objectReferenceValue);

                if (targetObject == null)
                    goto end;

                field = targetObject.GetIterator();
            }
            else
                field = property;

            if (SHOW_OPEN_IN_WINDOW_BUTTON == ButtonDisplayMode.Box)
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (SHOW_SCRIPT_FIELD && property.propertyType == SerializedPropertyType.ObjectReference)
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                field.NextVisible(true);
                while (field.NextVisible(false))
                    totalHeight += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                string path = field.propertyPath;
                field.NextVisible(true);
                do
                {
                    if (!field.propertyPath.StartsWith(path))
                        break;

                    totalHeight += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
                } while (field.NextVisible(false));
                totalHeight -= EditorGUIUtility.standardVerticalSpacing;
            }

        end:
            return totalHeight;
        }
    }
}