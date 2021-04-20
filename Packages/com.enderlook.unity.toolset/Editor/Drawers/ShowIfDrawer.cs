using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    internal sealed class ShowIfDrawer : SmartPropertyDrawer
    {
        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            ShowIfAttribute.ActionMode mode = showIfAttribute.mode;

            if (mode == ShowIfAttribute.ActionMode.ShowHide)
            {
                if (IsActive(showIfAttribute))
                    DrawField();
            }
            else if (mode == ShowIfAttribute.ActionMode.EnableDisable)
            {
                EditorGUI.BeginDisabledGroup(IsActive(showIfAttribute));
                DrawField();
                EditorGUI.EndDisabledGroup();
            }

            void DrawField()
            {
                GUIContentHelper.GetGUIContent(property, ref label);
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        protected override float GetPropertyHeightSmart(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            if (IsActive(showIfAttribute) || showIfAttribute.mode == ShowIfAttribute.ActionMode.EnableDisable)
                return EditorGUI.GetPropertyHeight(property, label, true);
            return 0;
        }

        private bool IsActive(ShowIfAttribute showIfAttribute)
        {
            object parent = serializedProperty.GetParentTargetObjectOfProperty();
            object value = parent.GetValueFromFirstMember(showIfAttribute.nameOfConditional, showIfAttribute.memberType, true);
            bool active = value.Equals(showIfAttribute.compareTo);
            if (!showIfAttribute.mustBeEqual)
                active = !active;
            return active;
        }
    }
}