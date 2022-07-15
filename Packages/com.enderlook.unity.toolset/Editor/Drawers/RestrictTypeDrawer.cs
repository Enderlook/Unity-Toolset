using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(RestrictTypeAttribute))]
    internal sealed class RestrictTypeDrawer : SmartPropertyDrawer
    {
        private float? height;
        private bool firstTime = true;

        private Rect GetBoxPosition(string message, Rect position)
        {
            height = GUI.skin.box.CalcHeight(new GUIContent(message), position.width);
            return new Rect(position.x, position.y, position.width, height.Value);
        }

        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            void DrawErrorBox(string message)
            {
                Debug.LogError(message);
                EditorGUI.HelpBox(GetBoxPosition(message, position), message, MessageType.Error);
            }

            height = null;

            if (!((RestrictTypeAttribute)attribute).CheckRestrictionFeasibility(fieldInfo.FieldType, out string errorMessage))
            {
                DrawErrorBox($"Field {property.name} error. {errorMessage}");
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                firstTime = false;
                UnityObject result = property.objectReferenceValue;
                if (result != null && !((RestrictTypeAttribute)attribute).CheckIfTypeIsAllowed(result.GetType(), out errorMessage))
                {
                    Debug.LogError($"Field {property.name} error. {errorMessage}");
                    property.objectReferenceValue = null;
                }
            }
        }

        protected override float GetPropertyHeightSmart(SerializedProperty property, GUIContent label) => height ?? EditorGUI.GetPropertyHeight(property, label, true);
    }
}