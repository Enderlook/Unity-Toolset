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

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            EditorGUI.BeginChangeCheck();
            int layer = EditorGUI.LayerField(position, label, property.intValue);
            if (EditorGUI.EndChangeCheck())
            {
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
            }
        }

        protected internal override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren, float height)
            => EditorGUI.GetPropertyHeight(property, label, includeChildren);
    }
}