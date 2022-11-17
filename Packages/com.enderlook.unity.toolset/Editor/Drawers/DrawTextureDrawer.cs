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
        private const int NEW_LINE_TEXTURE_SPACE = 2;
        private const int NEW_LINE_TEXTURE_SPACE_RIGHT = 2;

        private GUIContent textureContent;
        private GUIContent errorContent;

        protected internal override bool RequestMain => true;

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (TryProduceTextureGUIContent(property, label, out Texture2D texture))
            {
                DrawTextureAttribute drawTextureAttribute = (DrawTextureAttribute)Attribute;

                float size = CalculateTextureHeight(texture, drawTextureAttribute);
                float height = size;
                float width = size / texture.height * texture.width;
                if (size > texture.height)
                {
                    // Unity doesn't upscale texture manually, so we do it.
                    if (texture is Texture2D texture2D)
                    {
                        int width_ = (int)width;
                        Texture2D newTexture = new Texture2D(width_, (int)height, texture2D.format, true);
                        Color[] pixels = newTexture.GetPixels(0);
                        try
                        {
                            for (int i = 0; i < pixels.Length; i++)
                            {
                                int x = i % width_;
                                int y = i / width_;
                                pixels[i] = texture2D.GetPixelBilinear(x / width, y / height);
                            }
                        }
                        catch (UnityException)
                        {
                            // Texture was not configured with read access.
                            goto fallback;
                        }
                        newTexture.SetPixels(pixels, 0);
                        newTexture.Apply();
                        textureContent.image = newTexture;
                        goto next;
                    }
                fallback:
                    height = texture.height;
                    width = texture.width;
                next:;
                }

                if (drawTextureAttribute.mode == DrawTextureMode.CurrentLine)
                {
                    // Set texture position in same line.

                    float y;
                    float y2;
                    if (height < EditorGUIUtility.singleLineHeight)
                    {
                        // Align `y` of the texture if the texture is smaller than the line height.
                        y = position.y + ((EditorGUIUtility.singleLineHeight - height) / 2);
                        y2 = position.y;
                    }
                    else
                    {
                        y = position.y;
                        if (height > EditorGUIUtility.singleLineHeight)
                            // Align the `y` of the property field if te texture is larger than the line height.
                            y2 = position.y + ((height - EditorGUIUtility.singleLineHeight) / 2);
                        else
                            y2 = position.y;
                    }

                    Rect texturePosition = new Rect(position.x + EditorGUIUtility.labelWidth, y, width, height);
                    EditorGUI.LabelField(texturePosition, textureContent);

                    Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
                    EditorGUI.LabelField(labelRect, label);

                    float widthToRemove = EditorGUIUtility.labelWidth + texturePosition.width + SAME_LINE_TEXTURE_SPACE;
                    position.x += widthToRemove;
                    position.y = y2;
                    position.width -= widthToRemove;
                    EditorGUI.PropertyField(position, property, GUIContent.none, true);
                }
                else
                {
                    // Set texture position in new line.

                    float x = position.x;
                    switch (drawTextureAttribute.mode)
                    {
                        case DrawTextureMode.NewLineCenter:
                            x += position.width / 2;
                            break;
                        case DrawTextureMode.NewLineRight:
                            x += position.width - width - NEW_LINE_TEXTURE_SPACE_RIGHT;
                            break;
                    }

                    Rect texturePosition = new Rect(x + INDENT_WIDTH, position.y + EditorGUIUtility.singleLineHeight + NEW_LINE_TEXTURE_SPACE, width, height);
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
                    Debug.LogError($"Field {property.displayName} from {property.name} error. {message}");
                    EditorGUI.HelpBox(new Rect(position.x, position.y + position.height - height, position.width, height), message, MessageType.Error);
                }
                else
                    EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private bool TryProduceTextureGUIContent(SerializedProperty property, GUIContent label, out Texture2D texture)
        {
            if (TryGetTexture(property, out texture))
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

        private static bool TryGetTexture(SerializedProperty property, out Texture2D value)
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
                    Texture2D texture = Resources.Load<Texture2D>(path);
                    if (texture == null)
                    {
                        Sprite sprite = Resources.Load<Sprite>(path);
                        if (sprite != null)
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

        private static float CalculateTextureHeight(Texture texture, DrawTextureAttribute drawTextureAttribute)
        {
            float height_ = drawTextureAttribute.height;
            if (height_ == -1)
                height_ = drawTextureAttribute.mode == DrawTextureMode.CurrentLine ? EditorGUIUtility.singleLineHeight : texture.height;

            if (height_ > texture.height)
            {
                // Unity doesn't upscale texture manually, so we do it.
                if (texture is Texture2D texture2D)
                {
                    try
                    {
                        texture2D.GetPixelBilinear(0, 0);
                    }
                    catch (UnityException)
                    {
                        // Texture was not configured with read access.
                        goto fallback;
                    }
                    goto end;
                }
            fallback:
                return texture.height;
            }
        end:
            return height_;
        }

        protected internal override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            float height = EditorGUI.GetPropertyHeight(property, label);
            if (TryGetTexture(property, out Texture2D texture))
            {
                DrawTextureAttribute drawTextureAttribute = (DrawTextureAttribute)Attribute;
                if (drawTextureAttribute.mode != DrawTextureMode.CurrentLine)
                    height += CalculateTextureHeight(texture, drawTextureAttribute);
                else
                {
                    float height_ = CalculateTextureHeight(texture, drawTextureAttribute);
                    // Only increase the space if the texture will take more space than the current line.
                    if (height_ > height)
                        height += height_ - EditorGUIUtility.singleLineHeight;
                }
            }
            else
                height += GetPropertyTypeErrorMessage(property, EditorGUIUtility.currentViewWidth).height;
            return height;
        }
    }
}