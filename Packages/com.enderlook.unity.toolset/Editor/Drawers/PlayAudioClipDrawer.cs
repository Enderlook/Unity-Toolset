using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(PlayAudioClipAttribute))]
    internal class PlayAudioClipDrawer : StackablePropertyDrawer
    {
        private const int SPACE_BETTWEN_FIELD_AND_ERROR = 2;

        private GUIContent errorContent;

        protected internal override bool HasOnGUI => true;

        protected internal override void OnGUI(Rect position, SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren)
        {
            PlayAudioClipAttribute attribute = (PlayAudioClipAttribute)Attribute;

            SerializedProperty property = propertyInfo.SerializedProperty;
            if (!IsFine(propertyInfo))
            {
                EditorGUI.PropertyField(position, property, label);

                (float height, string message) = CalculateError(propertyInfo, position.width);

                EditorGUI.HelpBox(new Rect(position.x, position.y + position.height + SPACE_BETTWEN_FIELD_AND_ERROR, position.width, height), message, MessageType.Error);
                Debug.LogError(message);
                return;
            }

            AudioClip audioClip = GetAudioClip(property);
            if (audioClip == null)
                EditorGUI.PropertyField(position, property, label);
            else
            {
                bool isPlaying = AudioUtil.IsClipPlaying(audioClip);
                GUIContent playGUIContent = EditorGUIUtility.IconContent(isPlaying ? "PauseButton" : "PlayButton");
                float width = GUI.skin.button.CalcSize(playGUIContent).x;

                playGUIContent.tooltip = TimeSpan.FromSeconds(isPlaying
                    ? audioClip.length - AudioUtil.GetClipPosition(audioClip)
                    : audioClip.length
                ).ToString(@"hh\:mm\:ss\:ff");

                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width - width, position.height), property, label, true);

                bool showProgressBar = isPlaying && attribute.ShowProgressBar;

                if (GUI.Button(new Rect(position.x + position.width - width, position.y, width, position.height / (showProgressBar ? 2 : 1)), playGUIContent))
                    if (isPlaying)
                        AudioUtil.StopClip(audioClip);
                    else
                        AudioUtil.PlayClip(audioClip);

                if (showProgressBar)
                    EditorGUI.ProgressBar(new Rect(position.x + position.width - width, position.y + position.height / 2, width, position.height / 2), AudioUtil.GetClipPosition(audioClip) / audioClip.length, "");

                if (isPlaying)
                    // Forces repaint all the inspector per frame
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        protected internal override float GetPropertyHeight(SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, float height)
        {
            height = EditorGUI.GetPropertyHeight(propertyInfo.SerializedProperty, label, includeChildren);
            if (!IsFine(propertyInfo))
            {
                height += CalculateError(propertyInfo, EditorGUIUtility.currentViewWidth).height;
                height += SPACE_BETTWEN_FIELD_AND_ERROR;
            }
            return height;
        }

        private (float height, string message) CalculateError(SerializedPropertyInfo propertyInfo, float width)
        {
            string message = $"Attribute {nameof(PlayAudioClipAttribute)} can only be used in field of type {typeof(AudioClip)} or {typeof(string)}. Was {propertyInfo.MemberType}.";
            if (errorContent is null)
                errorContent = new GUIContent();
            errorContent.text = message;
            float height = GUI.skin.box.CalcHeight(errorContent, width);
            return (height, message);
        }

        private bool IsFine(SerializedPropertyInfo propertyInfo)
        {
            SerializedProperty property = propertyInfo.SerializedProperty;
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return true;
                case SerializedPropertyType.ObjectReference:
                    return !(property.objectReferenceValue is AudioClip) || propertyInfo.MemberType == typeof(AudioClip);
            }
            return false;
        }

        private AudioClip GetAudioClip(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return Resources.Load<AudioClip>(property.stringValue);
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue as AudioClip;
            }
            return null;
        }
    }
}