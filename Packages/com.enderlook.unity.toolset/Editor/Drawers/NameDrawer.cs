using Enderlook.Unity.Toolset.Attributes;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(NameAttribute))]
    internal sealed class NameDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeGetPropertyHeight(ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => GUIContentHelper.UseNameAttribute((NameAttribute)Attribute, label);

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => GUIContentHelper.UseNameAttribute((NameAttribute)Attribute, label);
    }
}