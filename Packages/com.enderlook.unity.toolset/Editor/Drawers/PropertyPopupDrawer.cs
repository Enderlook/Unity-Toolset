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
        private static ReadWriteLock @lock = new ReadWriteLock();
        private static readonly Dictionary<Type, PropertyPopup> allowedTypes = new Dictionary<Type, PropertyPopup>();

        protected internal override bool RequestMain => true;

        [DidReloadScripts]
        private static void Reset()
        {
            @lock.WriteBegin();
            {
                allowedTypes.Clear();
            }
            @lock.WriteEnd();
        }

        public static bool IsFieldOption(FieldInfo fieldInfo)
        {
            Type classType = fieldInfo.ReflectedType;
            PropertyPopup propertyPopup;
            @lock.ReadBegin();
            {
                if (!allowedTypes.TryGetValue(classType, out propertyPopup))
                {
                    PropertyPopupAttribute propertyPopupAttribute = classType.GetCustomAttribute<PropertyPopupAttribute>(true);
                    if (propertyPopupAttribute is null)
                    {
                        @lock.ReadEnd();
                        @lock.WriteBegin();
                        {
                            allowedTypes.Add(classType, null);
                        }
                        @lock.WriteEnd();
                        return false;
                    }

                    return true;
                }
            }
            @lock.ReadEnd();
            return !(propertyPopup is null);
        }

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (!TryGetPropertyPopup(property, out PropertyPopup propertyPopup))
                EditorGUI.PropertyField(position, property, label, true);
            else
                propertyPopup.OnGUI(position, property, label);
        }

        protected internal override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (!TryGetPropertyPopup(property, out PropertyPopup propertyPopup))
                return base.GetPropertyHeight(property, label, includeChildren);
            return propertyPopup.GetPropertyHeight(property, label);
        }

        private static bool TryGetPropertyPopup(SerializedProperty property, out PropertyPopup propertyPopup)
        {
            Type classType = property.GetPropertyType();
            bool value;
            @lock.ReadBegin();
            {
                value = allowedTypes.TryGetValue(classType, out propertyPopup);
            }
            @lock.ReadEnd();
            if (!value)
            {
                PropertyPopupAttribute propertyPopupAttribute = classType.GetCustomAttribute<PropertyPopupAttribute>(true);
                if (propertyPopupAttribute is null)
                {
                    @lock.WriteBegin();
                    {
                        allowedTypes[classType] = null;
                    }
                    @lock.WriteEnd();
                    return false;
                }

                List<PropertyPopupOption> list = new List<PropertyPopupOption>();
                foreach (FieldInfo fieldInfo in classType.GetFieldsExhaustive(ExhaustiveBindingFlags.Instance))
                    if (fieldInfo.GetCustomAttribute<PropertyPopupOptionAttribute>(true) is PropertyPopupOptionAttribute attribute)
                        list.Add(new PropertyPopupOption(fieldInfo.Name, property.FindPropertyRelative(fieldInfo.Name).GetDisplayName(), attribute));

                PropertyPopupOption[] modes = list.ToArray();

                @lock.WriteBegin();
                {
                    if (!allowedTypes.TryGetValue(classType, out propertyPopup))
                        allowedTypes.Add(classType, propertyPopup = new PropertyPopup(propertyPopupAttribute.ModeReferenceName, modes));
                }
                @lock.WriteEnd();
            }
            return !(propertyPopup is null);
        }
    }
}
