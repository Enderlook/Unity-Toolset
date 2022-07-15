using Enderlook.Unity.Toolset.Attributes;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(LayerAttribute))]
    internal sealed class LayerAttributeEditor : StackablePropertyDrawer
    {
        private static readonly string ERROR_SERIALIZED_PROPERTY_TYPE = $"{typeof(LayerAttribute)} only support serialized properties of type {nameof(SerializedPropertyType.Integer)} ({typeof(int)}), {nameof(SerializedPropertyType.Float)} ({typeof(float)}), {nameof(SerializedPropertyType.String)} ({typeof(string)}) or {nameof(SerializedPropertyType.LayerMask)} ({typeof(LayerMask)})";

        protected internal override bool HasOnGUI => true;

        protected internal override void OnGUI(Rect position, SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren)
        {
            EditorGUI.BeginChangeCheck();
            SerializedProperty serializedProperty = propertyInfo.SerializedProperty;
            int layer = EditorGUI.LayerField(position, label, serializedProperty.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                switch (serializedProperty.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        serializedProperty.intValue = layer;
                        break;
                    case SerializedPropertyType.Float:
                        serializedProperty.floatValue = layer;
                        break;
                    case SerializedPropertyType.LayerMask:
                        serializedProperty.intValue = layer;
                        break;
                    case SerializedPropertyType.String:
                        serializedProperty.stringValue = LayerMask.LayerToName(layer);
                        break;
                    default:
                        throw new ArgumentException(ERROR_SERIALIZED_PROPERTY_TYPE);
                }
            }
        }

        protected internal override float GetPropertyHeight(SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, float height)
            => EditorGUI.GetPropertyHeight(propertyInfo.SerializedProperty, label, includeChildren);
    }
}