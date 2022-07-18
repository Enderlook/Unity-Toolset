using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using UnityEditor;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(RestrictTypeAttribute))]
    internal sealed class RestrictTypeDrawer : StackablePropertyDrawer
    {
        private float? height;
        private bool firstTime = true;

        protected internal override bool RequestMain => true;

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            RestrictTypeAttribute attribute = (RestrictTypeAttribute)Attribute;
            if (!attribute.CheckRestrictionFeasibility(property.GetPropertyType(), out string errorMessage))
            {
                DrawErrorBox($"Field {property.name} error. {errorMessage}");
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (EditorGUI.EndChangeCheck() || firstTime)
            {
                firstTime = false;
                UnityObject result = property.objectReferenceValue;
                if (result != null && !attribute.CheckIfTypeIsAllowed(result.GetType(), out errorMessage))
                {
                    DrawErrorBox($"Field {property.name} error. {errorMessage}");
                    property.objectReferenceValue = null;
                }
            }

            void DrawErrorBox(string message)
            {
                Debug.LogError(message);
                height = GUI.skin.box.CalcHeight(new GUIContent(message), position.width);
                Rect box = new Rect(position.x, position.y, position.width, height.Value);
                EditorGUI.HelpBox(box, message, MessageType.Error);
            }
        }

        protected internal override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
            => height ?? base.GetPropertyHeight(property, label, includeChildren);
    }
}