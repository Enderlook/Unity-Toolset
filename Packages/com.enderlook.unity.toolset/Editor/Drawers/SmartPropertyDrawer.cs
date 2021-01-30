using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(NameAttribute))]
    [CustomPropertyDrawer(typeof(GUIAttribute))]
    [CustomPropertyDrawer(typeof(IndentedAttribute))]
    public class SmartPropertyDrawer : PropertyDrawer
    {
        protected SerializedPropertyHelper helper;

        private int identationOffset;

        private void TryCreateHelper(SerializedProperty property)
        {
            if (helper == null)
                helper = property.GetHelper();
            else if (helper.serializedProperty.propertyPath == property.propertyPath)
                helper.ResetCycle();
            else
                helper = property.GetHelper();
        }

        private void Before(ref Rect position, ref SerializedProperty property, ref GUIContent label)
        {
            TryCreateHelper(property);

            SerializedPropertyGUIHelper.GetGUIContent(property, ref label);

            IndentedAttribute indentedAttribute = helper.GetAttributeFromField<IndentedAttribute>(true);
            identationOffset = indentedAttribute?.indentationOffset ?? 0;
            EditorGUI.indentLevel += identationOffset;
        }

        private void After(Rect position, SerializedProperty property, GUIContent label)
            => EditorGUI.indentLevel -= identationOffset;

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Before(ref position, ref property, ref label);
            OnGUISmart(position, property, label);
            After(position, property, label);
        }

        protected virtual void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
            => EditorGUI.PropertyField(position, property);

        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TryCreateHelper(property);
            SerializedPropertyGUIHelper.GetGUIContent(property, ref label);
            return GetPropertyHeightSmart(property, label);
        }

        protected virtual float GetPropertyHeightSmart(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}