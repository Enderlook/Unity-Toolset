using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(object), true)]
    internal sealed class PropertyPopupDrawer2 : StackablePropertyDrawer
    {
        private static readonly Dictionary<Type, PropertyPopup> alloweds = new Dictionary<Type, PropertyPopup>();

        private static readonly HashSet<Type> disalloweds = new HashSet<Type>();

        private float height = -1;

        protected internal override bool HasOnGUI => true;

        [DidReloadScripts]
        private static void Reset()
        {
            alloweds.Clear();
            disalloweds.Clear();
        }

        protected internal override void OnGUI(Rect position, SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren)
        {
            Type classType = propertyInfo.MemberType;
            SerializedProperty property = propertyInfo.SerializedProperty;
            if (alloweds.TryGetValue(classType, out PropertyPopup propertyPopup))
                height = propertyPopup.DrawField(position, property, label);
            else if (disalloweds.Contains(classType))
                EditorGUI.PropertyField(position, property, label, true);
            else
            {
                PropertyPopupAttribute propertyPopupAttribute = classType.GetCustomAttribute<PropertyPopupAttribute>(true);
                if (propertyPopupAttribute is null)
                {
                    disalloweds.Add(classType);
                    EditorGUI.PropertyField(position, property, label, true);
                }
                else
                {
                    List<PropertyPopupOption> list = new List<PropertyPopupOption>();
                    foreach (var element in classType.GetInheritedFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        if (element.GetCustomAttribute<PropertyPopupOptionAttribute>(true) is PropertyPopupOptionAttribute attribute)
                            list.Add(new PropertyPopupOption(element.Name, attribute));
                    PropertyPopupOption[] modes = list.ToArray();

                    propertyPopup = new PropertyPopup(propertyPopupAttribute.modeName, modes);
                    alloweds.Add(classType, propertyPopup);
                    height = propertyPopup.DrawField(position, property, label);
                }
            }
        }

        protected internal override float GetPropertyHeight(SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, float height)
            => this.height == -1 ? EditorGUI.GetPropertyHeight(propertyInfo.SerializedProperty, label, true) : this.height;
    }
}
