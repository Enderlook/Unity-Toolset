using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(LabelAttribute))]
    internal sealed class LabelDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeGetPropertyHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            // TODO: Remove this hack.
            if (PropertyPopupDrawer.IsFieldOption(FieldInfo))
                return;

            GUIContentHelper.UseGUIContent((LabelAttribute)Attribute, property, ref label, false);
        }

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            // TODO: Remove this hack.
            if (PropertyPopupDrawer.IsFieldOption(FieldInfo))
                return;

            GUIContentHelper.UseGUIContent((LabelAttribute)Attribute, property, ref label, false);
        }
    }
}