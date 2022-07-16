using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(LabelAttribute))]
    internal sealed class LabelDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeGetPropertyHeight(ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            // TODO: Remove this hack.
            if (PropertyPopupDrawer.IsFieldOption(FieldInfo))
                return;

            GUIContentHelper.UseGUIContent((LabelAttribute)Attribute, propertyInfo.SerializedProperty, ref label);
        }

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            // TODO: Remove this hack.
            if (PropertyPopupDrawer.IsFieldOption(FieldInfo))
                return;

            GUIContentHelper.UseGUIContent((LabelAttribute)Attribute, propertyInfo.SerializedProperty, ref label);
        }
    }
}