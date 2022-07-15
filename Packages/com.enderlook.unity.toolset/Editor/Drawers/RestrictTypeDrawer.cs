using Enderlook.Unity.Toolset.Attributes;

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

        protected internal override bool HasOnGUI => true;

        private Rect GetBoxPosition(string message, Rect position)
        {
            height = GUI.skin.box.CalcHeight(new GUIContent(message), position.width);
            return new Rect(position.x, position.y, position.width, height.Value);
        }

        protected internal override void OnGUI(Rect position, SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren)
        {
            void DrawErrorBox(string message)
            {
                Debug.LogError(message);
                EditorGUI.HelpBox(GetBoxPosition(message, position), message, MessageType.Error);
            }

            height = null;

            SerializedProperty serializedProperty = propertyInfo.SerializedProperty;

            RestrictTypeAttribute attribute = (RestrictTypeAttribute)Attribute;
            if (!attribute.CheckRestrictionFeasibility(propertyInfo.MemberType, out string errorMessage))
            {
                DrawErrorBox($"Field {serializedProperty.name} error. {errorMessage}");
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, serializedProperty, label);
            if (EditorGUI.EndChangeCheck() || firstTime)
            {
                firstTime = false;
                UnityObject result = serializedProperty.objectReferenceValue;
                if (result != null && !attribute.CheckIfTypeIsAllowed(result.GetType(), out errorMessage))
                {
                    Debug.LogError($"Field {serializedProperty.name} error. {errorMessage}");
                    serializedProperty.objectReferenceValue = null;
                }
            }
        }

        protected internal override float GetPropertyHeight(SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, float height)
            => this.height ?? EditorGUI.GetPropertyHeight(propertyInfo.SerializedProperty, label, includeChildren);
    }
}