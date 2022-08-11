using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
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

        internal static void UseGUIContent(LabelAttribute attribute, SerializedProperty property, ref GUIContent label, bool throwOnError)
        {
            if (attribute.DisplayNameMode == LabelMode.ByValue)
            {
                label.text = attribute.DisplayNameOrGuiContent;

                if (attribute.TooltipMode == LabelMode.ByValue)
                    label.tooltip = attribute.Tooltip;
                else
                {
                    object parent = property.GetParentTargetObject();
                    if (parent.TryGetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive(attribute.Tooltip, BindingFlags, out string tooltip))
                        label.tooltip = tooltip;
                    else
                        ThrowTooltip(label, parent);
                }
            }
            else
            {
                object parent = property.GetParentTargetObject();
                if (attribute.Tooltip is null)
                {
                    if (parent.TryGetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive(attribute.DisplayNameOrGuiContent, BindingFlags, out string text))
                        label.text = text;
                    else if (parent.TryGetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive(attribute.DisplayNameOrGuiContent, BindingFlags, out GUIContent content))
                        label = content;
                    else
                        ThrowTextOrGUIContent(label);

                    void ThrowTextOrGUIContent(GUIContent label_)
                    {
                        string message = $"Not found in type `{parent.GetType()}`{(throwOnError ? "" : $" (in property `{property.propertyPath}`)")} a field or property (with getter) named `{attribute.DisplayNameOrGuiContent}` of type '{typeof(string)}' or `{typeof(GUIContent)}`, nor a method of the same name and return type where all its parameters are optional, has default value or are param.";
                        if (throwOnError)
                            throw new ArgumentException(message, nameof(property));
                        else
                        {
                            label_.text = "<LABEL_ERROR>";
                            label_.tooltip = message;
                            Debug.LogError(message);
                        }
                    }
                }
                else
                {
                    if (parent.TryGetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive(attribute.DisplayNameOrGuiContent, BindingFlags, out string text))
                        label.text = text;
                    else
                        ThrowText(label);

                    if (attribute.TooltipMode == LabelMode.ByValue)
                        label.tooltip = attribute.Tooltip;
                    else
                    {
                        if (parent.TryGetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive(attribute.Tooltip, BindingFlags, out string tooltip))
                            label.tooltip = tooltip;
                        else
                            ThrowTooltip(label, parent);
                    }

                    void ThrowText(GUIContent label_)
                    {
                        string message = $"Not found in type `{parent.GetType()}`{(throwOnError ? "" : $" (in property `{property.propertyPath}`)")} a field or property (with getter) named `{attribute.DisplayNameOrGuiContent}` of type '{typeof(string)}', nor a method of the same name and return type where all its parameters are optional, has default value or are param.";
                        if (throwOnError) 
                            throw new ArgumentException(message, nameof(property));
                        else
                        {
                            label_.text = "<LABEL_ERROR>";
                            label_.tooltip = message;
                            Debug.LogError(message);
                        }
                    }
                }
            }

            void ThrowTooltip(GUIContent label_, object parent)
            {
                string message = $"Not found in type `{parent.GetType()}`{(throwOnError ? "" : $" (in property `{property.propertyPath}`)")} a field or property (with getter) named `{attribute.Tooltip}` of type '{typeof(string)}', nor a method of the same name and return type where all its parameters are optional, has default value or are param.";
                if (throwOnError)
                    throw new ArgumentException(message, nameof(property));
                else
                {
                    label_.text = "<LABEL_ERROR>";
                    label_.tooltip = message;
                    Debug.LogError(message);
                }
            }
        }

        private static void SetGUIContent(SerializedProperty property, ref GUIContent content, bool throwOnError)
        {
            content.text = property.displayName;
            content.tooltip = property.tooltip;

            LabelAttribute guiAttribute = property.GetMemberInfo().GetCustomAttribute<LabelAttribute>(true);

            if (!(guiAttribute is null))
                UseGUIContent(guiAttribute, property, ref content, throwOnError);
        }

        /// <summary>
        /// Produce a <see cref="GUIContent"/> with label of <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns><see cref="GUIContent"/> of <paramref name="property"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="property"/>'s field is decorated with an invalid configuration of <see cref="LabelAttribute"/>.</exception>
        public static GUIContent GetGUIContent(this SerializedProperty property)
        {
            if (property is null) ThrowHelper.ThrowArgumentNullExceptionProperty();
            GUIContent content = new GUIContent(property.name, property.tooltip);
            SetGUIContent(property, ref content, true);
            return content;
        }

        internal static GUIContent GetGUIContentNotThrow(this SerializedProperty property)
        {
            if (property is null) ThrowHelper.ThrowArgumentNullExceptionProperty();
            GUIContent content = new GUIContent(property.name, property.tooltip);
            SetGUIContent(property, ref content, false);
            return content;
        }

        /// <summary>
        /// Get the display name of <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property to get its <see cref="GUIContent"/>.</param>
        /// <returns>Display name of <paramref name="property"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="property"/>'s field is decorated with an invalid configuration of <see cref="LabelAttribute"/>.</exception>
        public static string GetDisplayName(this SerializedProperty property)
        {
            if (property is null) ThrowHelper.ThrowArgumentNullExceptionProperty();
            GUIContent content = Interlocked.Exchange(ref staticContent, null) ?? new GUIContent();
            SetGUIContent(property, ref content, true);
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Throw when <paramref name="property"/>'s field is decorated with an invalid configuration of <see cref="LabelAttribute"/>.</exception>
        public static string GetTooltip(this SerializedProperty property)
        {
            if (property is null) ThrowHelper.ThrowArgumentNullExceptionProperty();
            GUIContent content = Interlocked.Exchange(ref staticContent, null) ?? new GUIContent();
            SetGUIContent(property, ref content, true);
            string tooltip = content.tooltip;
            content.text = null;
            content.tooltip = null;
            staticContent = content;
            return tooltip;
        }
    }
}