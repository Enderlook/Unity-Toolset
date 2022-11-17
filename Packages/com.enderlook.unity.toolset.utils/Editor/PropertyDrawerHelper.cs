using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A set of helper functions to develop <see cref="PropertyDrawer"/>s.
    /// </summary>
    public static class PropertyDrawerHelper
    {
        /// <summary>
        /// Get all <see cref="SerializedProperty"/> of the <see cref="UnityEngine.MonoBehaviour"/>s of the current(s) active(s) editor(s).
        /// </summary>
        /// <returns>An enumerable with all the properties, fields, attributes and the editor where they were taken.</returns>
        public static IEnumerable<(SerializedProperty serializedProperty, T field, Editor editor)> FindAllSerializePropertiesInActiveEditorOf<T>()
        {
            foreach (Editor editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                SerializedProperty serializedProperty = editor.serializedObject.GetIterator();
                while (serializedProperty.Next(true))
                {
                    UnityEngine.Object targetObject = serializedProperty.serializedObject.targetObject;
                    // Used to skip missing components
                    if (targetObject == null)
                        continue;
                    Type targetObjectClassType = targetObject.GetType();
                    FieldInfo field = targetObjectClassType.GetFieldExhaustive(serializedProperty.propertyPath, ExhaustiveBindingFlags.Instance);
                    if (!(field is null) && typeof(T).IsAssignableFrom(field.FieldType))
                        yield return (serializedProperty, (T)field.GetValue(targetObject), editor);
                }
            }
        }

        /// <summary>
        /// Get all <see cref="SerializedProperty"/> that have the <typeparamref name="T"/> attribute (or an attribute assignable to it) and are in one of the <see cref="UnityEngine.MonoBehaviour"/> of the current(s) active(s) editor(s).
        /// </summary>
        /// <typeparam name="T">Attribute type to look for.</typeparam>
        /// <param name="inherit">Whenever it should look for inherited attributes.</param>
        /// <returns>An enumerable with all the properties, fields, attributes and the editor where they were taken.</returns>
        public static IEnumerable<(SerializedProperty serializedProperty, MemberInfo memberInfo, T attribute, Editor editor)> FindAllSerializePropertiesInActiveEditorWithTheAttribute<T>(bool inherit = true) where T : Attribute
        {
            foreach (Editor editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                SerializedProperty serializedProperty = editor.serializedObject.GetIterator();
                while (serializedProperty.Next(true))
                {
                    UnityEngine.Object targetObject = serializedProperty.serializedObject.targetObject;
                    // Used to skip missing components
                    if (targetObject == null)
                        continue;
                    if (!serializedProperty.TryGetMemberInfo(out MemberInfo memberInfo)) // Catch all properties with errors, such as Unity-related fields that aren't as (like those fields which starts with `m_`)
                        continue;
                    Attribute attribute = memberInfo.GetCustomAttribute(typeof(T), inherit);
                    if (typeof(T).IsAssignableFrom(attribute?.GetType()))
                        yield return (serializedProperty, memberInfo, (T)attribute, editor);
                }
            }
        }
    }
}