using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(DrawTextureAttribute))]
    internal sealed class DrawTextureDrawer : StackablePropertyDrawer
    {
        private const int INDENT_WIDTH = 8; // TODO: This is wrong.

        private const int SAME_LINE_TEXTURE_SPACE = 2;

        private GUIContent textureContent;
        private GUIContent errorContent;

        protected internal override bool HasOnGUI => true;

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (TryProduceTextureGUIContent(property, label))
            {
                DrawTextureAttribute drawTextureAttribute = (DrawTextureAttribute)Attribute;
                float height = CalculateTextureHeight(position.height, drawTextureAttribute);

                float width = drawTextureAttribute.width;
                if (width == -1)
                    width = height;

                if (drawTextureAttribute.drawOnSameLine)
                {
                    // Set texture position in same line
                    float width_ = height;
                    if (textureContent != null && textureContent.image != null)
                        width_ = width_ / textureContent.image.height * textureContent.image.width;
                    Rect texturePosition = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, Mathf.Min(width, width_), height);
                    EditorGUI.LabelField(texturePosition, textureContent);

                    Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
                    EditorGUI.LabelField(labelRect, label);

                    float widthToRemove = EditorGUIUtility.labelWidth + texturePosition.width + SAME_LINE_TEXTURE_SPACE;
                    position.x += widthToRemove;
                    position.width -= widthToRemove;
                    EditorGUI.PropertyField(position, property, GUIContent.none, true);
                }
                else
                {
                    // Set texture position in new line
                    float x = position.x;
                    if (drawTextureAttribute.centered)
                        x += position.width / 2;

                    float width_ = width - INDENT_WIDTH;
                    float height_ = width_;
                    if (textureContent != null && textureContent.image != null)
                        height_ = height_ / textureContent.image.width * textureContent.image.height;
                    Rect texturePosition = new Rect(x + INDENT_WIDTH, position.y + EditorGUIUtility.singleLineHeight, width_, Mathf.Min(height, height_));
                    EditorGUI.LabelField(texturePosition, textureContent);

                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else
            {
                (string message, float height) = GetPropertyTypeErrorMessage(property, position.width);
                if (!(message is null))
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, position.height - height), property, label, true);
                    Debug.LogError($"Property {property.displayName} from {property.propertyPath} doesn't have an object of type {typeof(Sprite)}, {typeof(Texture2D)} nor {typeof(string)}.");
                    EditorGUI.HelpBox(new Rect(position.x, position.y + position.height - height, position.width, height), message, MessageType.Error);
                }
                else
                    EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private bool TryProduceTextureGUIContent(SerializedProperty property, GUIContent label)
        {
            if (TryGetTexture(property, out Texture texture))
            {
                if (textureContent is null)
                {
                    textureContent = new GUIContent
                    {
                        tooltip = label.text + "\n" + label.tooltip,
                        image = texture
                    };
                }
                else
                    textureContent.image = texture;
                return true;
            }
            else
            {
                if (!(textureContent is null))
                    textureContent.image = null;
                return false;
            }
        }

        private static bool TryGetTexture(SerializedProperty property, out Texture value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    if (property.objectReferenceValue is Texture2D texture2D)
                    {
                        value = texture2D;
                        return true;
                    }
                    else if (property.objectReferenceValue is Sprite sprite)
                    {
                        value = sprite.texture;
                        return true;
                    }
                    break;
                case SerializedPropertyType.String:
                    string path = property.stringValue;
                    Texture texture = Resources.Load<Texture2D>(path);
                    if (texture == null)
                    {
                        Sprite sprite = Resources.Load<Sprite>(path);
                        if (texture != null)
                            texture = sprite.texture;
                        else
                            break;
                    }
                    value = texture;
                    return true;
            }
            value = default;
            return false;
        }

        private (string message, float height) GetPropertyTypeErrorMessage(SerializedProperty property, float width)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    Type propertyType = property.GetPropertyType();
                    if (propertyType != typeof(Sprite) && propertyType != typeof(Texture2D))
                        goto default;
                    break;
                case SerializedPropertyType.String:
                    break;
                default:
                    string message = $"Doesn't have an object of type {typeof(Sprite)}, {typeof(Texture2D)} nor {typeof(string)}.";
                    if (errorContent is null)
                        errorContent = new GUIContent(message);
                    else
                        errorContent.text = message;
                    float height = GUI.skin.box.CalcHeight(errorContent, width);
                    return (message, height);
            }
            return default;
        }

        private static float CalculateTextureHeight(float height, DrawTextureAttribute drawTextureAttribute)
        {
            float height_ = drawTextureAttribute.height;
            if (height_ == -1)
                return height;
            return height;
        }

        protected internal override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren, float height)
        {
            height = EditorGUI.GetPropertyHeight(property, label);
            return CalculateWithAditionalPropertyHeight(property, height, EditorGUIUtility.currentViewWidth);
        }

        private float CalculateWithAditionalPropertyHeight(SerializedProperty property, float height, float width)
        {
            if (TryGetTexture(property, out _))
            {
                DrawTextureAttribute drawTextureAttribute = (DrawTextureAttribute)Attribute;
                if (!drawTextureAttribute.drawOnSameLine)
                    height += EditorGUIUtility.singleLineHeight + CalculateTextureHeight(height, drawTextureAttribute);
            }
            else
            {
                (string _, float height_) = GetPropertyTypeErrorMessage(property, width);
                height += height_;
            }
            return height;
        }
    }
}