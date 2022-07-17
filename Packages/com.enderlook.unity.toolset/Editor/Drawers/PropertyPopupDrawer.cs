using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(PropertyPopupAttribute), true)]
    internal sealed class PropertyPopupDrawer : StackablePropertyDrawer
    {
        private static readonly Dictionary<Type, PropertyPopup> allowedTypes = new Dictionary<Type, PropertyPopup>();

        protected internal override bool HasOnGUI => true;

        [DidReloadScripts]
        private static void Reset() => allowedTypes.Clear();

        public static bool IsFieldOption(FieldInfo fieldInfo)
        {
            Type classType = fieldInfo.ReflectedType;
            if (!allowedTypes.TryGetValue(classType, out var propertyPopup))
            {
                PropertyPopupAttribute propertyPopupAttribute = classType.GetCustomAttribute<PropertyPopupAttribute>(true);
                if (propertyPopupAttribute is null)
                {
                    allowedTypes.Add(classType, null);
                    return false;
                }

                return true;
            }
            return !(propertyPopup is null);
        }

        protected internal override void OnGUI(Rect position, SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren)
        {
            SerializedProperty property = propertyInfo.SerializedProperty;
            if (!TryGetPropertyPopup(propertyInfo, out PropertyPopup propertyPopup))
                EditorGUI.PropertyField(position, property, label, true);
            else
                propertyPopup.OnGUI(position, property, label);
        }

        protected internal override float GetPropertyHeight(SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, float height)
        {
            if (!TryGetPropertyPopup(propertyInfo, out PropertyPopup propertyPopup))
                return height;
            return propertyPopup.GetPropertyHeight(propertyInfo.SerializedProperty, label);
        }

        private static bool TryGetPropertyPopup(SerializedPropertyInfo propertyInfo, out PropertyPopup propertyPopup)
        {
            Type classType = propertyInfo.MemberType;
            if (!allowedTypes.TryGetValue(classType, out propertyPopup))
            {
                PropertyPopupAttribute propertyPopupAttribute = classType.GetCustomAttribute<PropertyPopupAttribute>(true);
                if (propertyPopupAttribute is null)
                {
                    allowedTypes.Add(classType, null);
                    return false;
                }

                List<PropertyPopupOption> list = new List<PropertyPopupOption>();
                foreach (FieldInfo fieldInfo in classType.GetFieldsExhaustive(ExhaustiveBindingFlags.Instance))
                    if (fieldInfo.GetCustomAttribute<PropertyPopupOptionAttribute>(true) is PropertyPopupOptionAttribute attribute)
                        list.Add(new PropertyPopupOption(fieldInfo.Name, propertyInfo.SerializedProperty.FindPropertyRelative(fieldInfo.Name).GetDisplayName(), attribute));

                PropertyPopupOption[] modes = list.ToArray();

                propertyPopup = new PropertyPopup(propertyPopupAttribute.ModeName, modes);
                allowedTypes.Add(classType, propertyPopup);
            }
            return !(propertyPopup is null);
        }
    }
}
