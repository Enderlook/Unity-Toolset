using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(RangeAttribute))]
    internal sealed class RangeDrawer : StackablePropertyDrawer
    {
        private bool isMain;

        protected internal override bool RequestMain => true;

        protected internal override void IsMain(bool isMain) => this.isMain = isMain;

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (isMain)
            {
                RangeAttribute attribute = (RangeAttribute)Attribute;
                if (property.propertyType == SerializedPropertyType.Integer)
                    EditorGUI.IntSlider(position, property, (int)attribute.min, (int)attribute.max, label);
                else
                    EditorGUI.Slider(position, property, attribute.min, attribute.max, label);
            }
        }
    }
}