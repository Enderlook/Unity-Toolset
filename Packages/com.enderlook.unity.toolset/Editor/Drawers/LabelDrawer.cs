using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(LabelAttribute))]
    internal sealed class LabelDrawer : StackablePropertyDrawer
    {
        protected internal override bool SupportUIToolkit => true;

        protected internal override void BeforeCreatePropertyGUI(ref SerializedProperty property, ref string label, ref string tooltip)
            => GUIContentHelper.TryReplaceDisplayNameAndTooltip(property, (LabelAttribute)Attribute, ref label, ref tooltip);

        protected internal override void BeforeGetPropertyHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            // TODO: Remove this hack.
            if (PropertyPopupDrawer.IsFieldOption(FieldInfo))
                return;

            GUIContentHelper.UseGUIContent((LabelAttribute)Attribute, property, ref label);
        }

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            // TODO: Remove this hack.
            if (PropertyPopupDrawer.IsFieldOption(FieldInfo))
                return;

            GUIContentHelper.UseGUIContent((LabelAttribute)Attribute, property, ref label);
        }
    }
}