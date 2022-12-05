using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(LayerAttribute))]
    internal sealed class LayerAttributeEditor : StackablePropertyDrawer
    {
        protected internal override bool RequestMain => true;

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            EditorGUI.BeginChangeCheck();

            Type type = null;
            int value;
            SerializedPropertyType propertyType = property.propertyType;
            bool error = false;
            if (propertyType == SerializedPropertyType.String)
            {
                string layer = property.stringValue;
                value = string.IsNullOrEmpty(layer) ? 0 : LayerMask.NameToLayer(layer);
            }
            else if (propertyType == SerializedPropertyType.LayerMask)
                value = property.intValue;
            else
            {
                type = property.GetPropertyType();
                if (type == typeof(sbyte) ||
                    type == typeof(byte) ||
                    type == typeof(ushort) ||
                    type == typeof(short) ||
                    type == typeof(int) ||
                    type == typeof(uint) ||
                    type == typeof(long) ||
                    type == typeof(ulong) ||
                    type == typeof(float) ||
                    type == typeof(double) ||
                    type == typeof(decimal))
                {
                    object obj = property.GetValue();
                    value = (int)Convert.ChangeType(obj, typeof(int));
                }
                else
                {
                    value = 0;
                    error = true;
                }
            }

            if (error)
            {
                EditorGUI.HelpBox(position, $"Field {property.name} error. {typeof(LayerAttribute)} only support serialized properties of type {typeof(byte)}, {typeof(sbyte)}, {typeof(short)}, {typeof(ushort)}, {typeof(int)}, {typeof(uint)}, {typeof(long)}, {typeof(ulong)}, {typeof(float)}, {typeof(double)}, {typeof(decimal)}, {typeof(LayerMask)}, {typeof(string)}. Is {type}.", MessageType.Error);
                Debug.LogError($"Field {property.name} at path {property.propertyPath} in object {property.serializedObject.targetObject} error. {typeof(LayerAttribute)} only support serialized properties of type {typeof(byte)}, {typeof(sbyte)}, {typeof(short)}, {typeof(ushort)}, {typeof(int)}, {typeof(uint)}, {typeof(long)}, {typeof(ulong)}, {typeof(float)}, {typeof(double)}, {typeof(decimal)}, {typeof(LayerMask)}, {typeof(string)}. Is {type}.");
            }
            else
                value = EditorGUI.LayerField(position, label, value);

            if (EditorGUI.EndChangeCheck())
            {
                if (propertyType == SerializedPropertyType.String)
                    property.stringValue = LayerMask.LayerToName(value);
                else if (propertyType == SerializedPropertyType.LayerMask)
                    property.intValue = value;
                else
                    property.SetValue(Convert.ChangeType(value, type));
            }
        }

        protected internal override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            Type type = property.GetPropertyType();
            if (type == typeof(sbyte) ||
                type == typeof(byte) ||
                type == typeof(ushort) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal) ||
                type == typeof(string) ||
                type == typeof(LayerMask))
                return base.GetPropertyHeight(property, label, includeChildren);
            GUIContent tmp = GUIContentHelper.RentGUIContent();
            tmp.text = $"Field {property.name} error. {typeof(LayerAttribute)} only support serialized properties of type {typeof(byte)}, {typeof(sbyte)}, {typeof(short)}, {typeof(ushort)}, {typeof(int)}, {typeof(uint)}, {typeof(long)}, {typeof(ulong)}, {typeof(float)}, {typeof(double)}, {typeof(decimal)}, {typeof(LayerMask)}, {typeof(string)}. Is {type}.";
            float height = GUI.skin.box.CalcHeight(tmp, EditorGUIUtility.currentViewWidth);
            GUIContentHelper.ReturnGUIContent(tmp);
            return height;
        }
    }
}