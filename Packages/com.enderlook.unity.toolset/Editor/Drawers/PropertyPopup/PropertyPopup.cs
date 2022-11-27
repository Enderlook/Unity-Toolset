using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Drawers
{
    /// <summary>
    /// A helper class to draw properties according to a popup selector.
    /// </summary>
    internal sealed class PropertyPopup
    {
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const int NAME_NOT_FOUND = -1;
        private const int VALUE_NOT_FOUND = -2;
        private const string NOT_FOUND_OPTION = "Not found an option which satisfy {0}.{1} ({2}).";
        private const string NOT_FOUND_NAME = "Not found serialized property, field or property (with get and set method) named {0}.";

        private static readonly GUIStyle popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
        {
            imagePosition = ImagePosition.ImageOnly
        };

        private static Type[] tmpType;

        private readonly string modeProperty;
        private readonly PropertyPopupOption[] modes;
        private readonly string[] popupOptions;

        private static class Comparers
        {
            // This must be stored in a separated class because `Reset` method is called when script reloads,
            // which executes the static constructor of the class, and that initialized the static field `popupStyle`
            // but GUI functions (`GUIStyle` and `GUI.skin`) can can only be called from inside OnGUI method,
            // which results in an exception thrown.

            private static readonly Dictionary<Type, IEqualityComparer> comparers = new Dictionary<Type, IEqualityComparer>();
            private static ReadWriteLock @lock = new ReadWriteLock();

            [DidReloadScripts]
            private static void Reset()
            {
                @lock.WriteBegin();
                {
                    comparers.Clear();
                }
                @lock.WriteEnd();
            }

            public static IEqualityComparer Get(Type type)
            {
                IEqualityComparer comparer;
                bool found;
                @lock.ReadBegin();
                {
                    found = comparers.TryGetValue(type, out comparer);
                }
                @lock.ReadEnd();
                if (!found)
                    comparer = Create(type);
                return comparer;

                static IEqualityComparer Create(Type type)
                {
                    Type[] array = Interlocked.Exchange(ref tmpType, null) ?? new Type[1];
                    ref Type slot = ref array[0];
                    slot = type;
                    Type comparerType = typeof(EqualityComparer<>).MakeGenericType(slot);
                    slot = null;
                    tmpType = array;
                    IEqualityComparer comparer = (IEqualityComparer)comparerType
                        .GetProperty(nameof(EqualityComparer<object>.Default))
                        .GetValue(null);
                    @lock.WriteBegin();
                    {
                        if (!comparers.ContainsKey(type))
                            comparers.Add(type, comparer);
                    }
                    @lock.WriteEnd();
                    return comparer;
                }
            }
        }

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

        private static LogBuilder GetLogger(SerializedProperty serializedProperty)
        {
            string propertyName = serializedProperty.name;
            string propertyPath = serializedProperty.propertyPath;
            string targetObjectName = serializedProperty.serializedObject.targetObject.name;
            // This value was got by concatenating the sum of the largest possible path of appended constants that can happen in methods of this class,
            // and an approximate length of variables.
            int minCapacity = 178 + 40 + propertyName.Length + propertyPath.Length + targetObjectName.Length + propertyPath.Length;
            return LogBuilder.GetLogger(minCapacity)
                .Append($"Invalid use {nameof(PropertyPopupAttribute)} ")
                .Append(" on serialized property '")
                .Append(propertyName)
                .Append("' at path '")
                .Append(propertyPath)
                .Append("' on object at name '")
                .Append(targetObjectName)
                .Append("':");
        }

        /// <summary>
        /// Draw the field in the given place.
        /// </summary>
        /// <param name="position">Position to draw the field.</param>
        /// <param name="property">Property used to draw the field.</param>
        /// <param name="label">Label to show in inspector.</param>
        public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            (int index, object value) tuple = GetIndex(property);

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

            int newUsagePopupIndex = EditorGUI.Popup(buttonRect, tuple.index, popupOptions, popupStyle);
            if (newUsagePopupIndex != tuple.index)
            {
                object value = modes[newUsagePopupIndex].Target;
                Set(property, value);
            }

            switch (newUsagePopupIndex)
            {
                case NAME_NOT_FOUND:
                    EditorGUI.HelpBox(newPosition, string.Format(NOT_FOUND_NAME, modeProperty), MessageType.Error);
                    break;
                case VALUE_NOT_FOUND:
                    EditorGUI.HelpBox(newPosition, string.Format(NOT_FOUND_OPTION, property.propertyPath, modeProperty, tuple.value), MessageType.Error);
                    break;
                default:
                    PropertyPopupOption propertyPopupOption = modes[newUsagePopupIndex];

                    SerializedProperty optionProperty = property.FindPropertyRelative(propertyPopupOption.PropertyName);
                    if (IsLargerThanOneLine(optionProperty))
                        EditorGUI.PropertyField(position, optionProperty, GUIContent.none, true);
                    else
                        EditorGUI.PropertyField(newPosition, optionProperty, GUIContent.none, true);
                    break;
            }
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

        private (int index, object value) GetIndex(SerializedProperty serializedProperty)
        {
            (Type type, object value) tuple = Get(serializedProperty);
            if (tuple.type is null)
                return (NAME_NOT_FOUND, default);

            IEqualityComparer comparer = Comparers.Get(tuple.type);

            for (int modeIndex = 0; modeIndex < modes.Length; modeIndex++)
                if (comparer.Equals(modes[modeIndex].Target, tuple.value))
                    return (modeIndex, tuple.value);

            return (VALUE_NOT_FOUND, tuple.value);
        }

        private (Type type, object value) Get(SerializedProperty serializedProperty)
        {
            string modeProperty = this.modeProperty;
            if (string.IsNullOrEmpty(modeProperty))
                goto notFound;

            SerializedProperty mode = serializedProperty.FindPropertyRelative(modeProperty);
            if (!(mode is null))
                return (mode.GetPropertyType(), mode.GetValue());

            UnityObject targetObject = serializedProperty.serializedObject.targetObject;
            Type targetObjectType = targetObject.GetType();

            FieldInfo fieldInfo = targetObjectType.GetField(modeProperty, bindingFlags);
            if (!(fieldInfo is null))
                return (fieldInfo.FieldType, fieldInfo.GetValue(targetObject));

            PropertyInfo propertyInfo = targetObjectType.GetProperty(modeProperty, bindingFlags);
            if (!(propertyInfo is null) && propertyInfo.CanRead && propertyInfo.CanWrite)
                return (propertyInfo.PropertyType, propertyInfo.GetValue(targetObject));

        notFound:
            return default;
        }

        private void Set(SerializedProperty serializedProperty, object value)
        {
            string modeProperty = this.modeProperty;
            if (!string.IsNullOrEmpty(modeProperty))
            {
                SerializedProperty mode = serializedProperty.FindPropertyRelative(modeProperty);
                if (!(mode is null))
                    mode.SetValue(value);
                else
                {
                    UnityObject targetObject = serializedProperty.serializedObject.targetObject;
                    Type targetObjectType = targetObject.GetType();

                    FieldInfo fieldInfo = targetObjectType.GetField(modeProperty, bindingFlags);
                    if (!(fieldInfo is null))
                        fieldInfo.SetValue(targetObject, value);
                    else
                    {
                        PropertyInfo propertyInfo = targetObjectType.GetProperty(modeProperty, bindingFlags);
                        if (!(propertyInfo is null))
                            propertyInfo.SetValue(targetObject, value);
                    }
                }
                serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Get the height of the drawed property.
        /// </summary>
        /// <param name="property">Property used to draw the field.</param>
        /// <param name="label">Label to show in inspector.</param>
        /// <returns>Property height.</returns>
        public float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            (int index, object value) = GetIndex(property);
            switch (index)
            {
                case NAME_NOT_FOUND:
                    return NotFoundName();
                case VALUE_NOT_FOUND:
                    return NotFoundValue();
                default:
                    return GetPropertyHeight();
            }

            float NotFoundName()
            {
                float height;
                GUIContent guiContent = GUIContentHelper.RentGUIContent();
                {
                    guiContent.text = string.Format(NOT_FOUND_NAME, modeProperty);
                    float width = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - popupStyle.fixedWidth - popupStyle.margin.right;
                    height = GUI.skin.box.CalcHeight(guiContent, width);
                    guiContent.text = null;
                }
                GUIContentHelper.ReturnGUIContent(guiContent);
                GetLogger(property)
                    .Append(" not found serialized property, field or property (with get and set method) named ")
                    .Append(property.propertyPath)
                    .Append('.')
                    .Append(modeProperty)
                    .Append('.')
                    .LogError();
                return height;
            }

            float NotFoundValue()
            {
                float height;
                GUIContent guiContent = GUIContentHelper.RentGUIContent();
                {
                    guiContent.text = string.Format(NOT_FOUND_OPTION, property.propertyPath, modeProperty, value);
                    float width = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - popupStyle.fixedWidth - popupStyle.margin.right;
                    height = GUI.skin.box.CalcHeight(guiContent, width);
                    guiContent.text = null;
                }
                GUIContentHelper.ReturnGUIContent(guiContent);
                GetLogger(property)
                    .Append(" not found any option which satisfy ")
                    .Append(property.propertyPath)
                    .Append('.')
                    .Append(modeProperty)
                    .Append(" (")
                    .Append(value)
                    .Append(").")
                    .LogError();
                return height;
            }

            float GetPropertyHeight()
            {
                PropertyPopupOption propertyPopupOption = modes[index];
                SerializedProperty choosenProperty = property.FindPropertyRelative(propertyPopupOption.PropertyName);
                float height = EditorGUI.GetPropertyHeight(property, label, false);
                if (IsLargerThanOneLine(choosenProperty) && choosenProperty.isExpanded)
                    height += EditorGUI.GetPropertyHeight(choosenProperty, choosenProperty.GetGUIContent(), true);
                return height;
            }
        }
    }
}
