using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(object), true)]
    internal sealed class PropertyPopupDrawer2 : SmartPropertyDrawer
    {
        private static readonly Dictionary<Type, PropertyPopup> alloweds = new Dictionary<Type, PropertyPopup>();

        private static readonly HashSet<Type> disalloweds = new HashSet<Type>();

        private float height = -1;

        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            Type classType = fieldInfo.FieldType;
            if (alloweds.TryGetValue(classType, out PropertyPopup propertyPopup))
                height = propertyPopup.DrawField(position, property, label);
            else if (disalloweds.Contains(classType))
                EditorGUI.PropertyField(position, property, label, true);
            else
            {
                PropertyPopupAttribute propertyPopupAttribute = classType.GetCustomAttribute<PropertyPopupAttribute>(true);
                if (propertyPopupAttribute == null)
                {
                    disalloweds.Add(classType);
                    EditorGUI.PropertyField(position, property, label, true);
                }
                else
                {
                    PropertyPopupOption[] modes =
                        classType.GetInheritedFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(e => (e, e.GetCustomAttribute<PropertyPopupOptionAttribute>(true)))
                        .Where(e => e.Item2 != null)
                        .Select(e => new PropertyPopupOption(e.e.Name, e.Item2))
                        .ToArray();

                    propertyPopup = new PropertyPopup(propertyPopupAttribute.modeName, modes);
                    alloweds.Add(classType, propertyPopup);
                    height = propertyPopup.DrawField(position, property, label);
                }
            }
        }

        protected override float GetPropertyHeightSmart(SerializedProperty property, GUIContent label)
            => height == -1 ? EditorGUI.GetPropertyHeight(property, label, true) : height;
    }
}
