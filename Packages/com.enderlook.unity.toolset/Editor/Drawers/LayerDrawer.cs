using Enderlook.Unity.Toolset.Attributes;

using System;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(LayerAttribute))]
    internal sealed class LayerAttributeEditor : StackablePropertyDrawer
    {
        private static readonly string ERROR_SERIALIZED_PROPERTY_TYPE = $"{typeof(LayerAttribute)} only support serialized properties of type {nameof(SerializedPropertyType.Integer)} ({typeof(int)}), {nameof(SerializedPropertyType.Float)} ({typeof(float)}), {nameof(SerializedPropertyType.String)} ({typeof(string)}) or {nameof(SerializedPropertyType.LayerMask)} ({typeof(LayerMask)})";

        private static readonly EventCallback<ChangeEvent<int>> callback = e =>
        {
            SerializedProperty property = (SerializedProperty)((LayerField)e.target).userData;
            property.stringValue = LayerMask.LayerToName(e.newValue);
            property.serializedObject.ApplyModifiedProperties();
        };

        protected internal override bool RequestMain => true;

        protected internal override VisualElement CreatePropertyGUI(SerializedProperty property, string label, string tooltip)
        {
            LayerField field = new LayerField(label, default);
            field.tooltip = tooltip;
            if (property.propertyType == SerializedPropertyType.String)
            {
                field.value = LayerMask.NameToLayer(property.stringValue);
                field.userData = property;
                field.RegisterValueChangedCallback(callback);
            }
            else
                field.BindProperty(property);
            return field;
        }

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            EditorGUI.BeginChangeCheck();
            int value;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    value = property.intValue;
                    break;
                case SerializedPropertyType.Float:
                    value = (int)property.floatValue;
                    break;
                case SerializedPropertyType.String:
                    value = LayerMask.NameToLayer(property.stringValue);
                    break;
                default:
                    Throw();
                    return;
            }
            int layer = EditorGUI.LayerField(position, label, value);
            if (EditorGUI.EndChangeCheck())
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                    case SerializedPropertyType.LayerMask:
                        property.intValue = layer;
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = layer;
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = LayerMask.LayerToName(layer);
                        break;
                    default:
                        Throw();
                        return;
                }
            }

            void Throw() => throw new ArgumentException(ERROR_SERIALIZED_PROPERTY_TYPE);
        }
    }
}