using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(GUIAttribute))]
    internal sealed class GUIDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeGetPropertyHeight(ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => GUIContentHelper.UseGUIContent((GUIAttribute)Attribute, propertyInfo.SerializedProperty, ref label);

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => GUIContentHelper.UseGUIContent((GUIAttribute)Attribute, propertyInfo.SerializedProperty, ref label);
    }
}