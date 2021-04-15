using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(IsPropertyAttribute))]
    internal sealed class IsPropertyDrawer : SmartPropertyDrawer
    {
        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = label.text.Replace("<", "").Replace(">k__Backing Field", "");

            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}