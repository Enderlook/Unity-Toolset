using Enderlook.Unity.Toolset.Attributes;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    internal sealed class LayerAttributeEditor : SmartPropertyDrawer
    {
        private static readonly string ERROR_SERIALIZED_PROPERTY_TYPE = $"{typeof(LayerAttribute)} only support serialized properties of type {nameof(SerializedPropertyType.Integer)} ({typeof(int)}), {nameof(SerializedPropertyType.Float)} ({typeof(float)}), {nameof(SerializedPropertyType.String)} ({typeof(string)}) or {nameof(SerializedPropertyType.LayerMask)} ({typeof(LayerMask)})";

        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            int layer = EditorGUI.LayerField(position, label, property.intValue);
            if (EditorGUI.EndChangeCheck())
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = layer;
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = layer;
                        break;
                    case SerializedPropertyType.LayerMask:
                        property.intValue = layer;
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = LayerMask.LayerToName(layer);
                        break;
                    default:
                        throw new ArgumentException(ERROR_SERIALIZED_PROPERTY_TYPE);
                }
            EditorGUI.EndProperty();
        }
    }
}