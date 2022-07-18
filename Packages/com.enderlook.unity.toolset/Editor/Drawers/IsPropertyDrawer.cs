using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(IsPropertyAttribute))]
    internal sealed class IsPropertyDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeGetPropertyHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => label.text = label.text.Replace("<", "").Replace(">k__Backing Field", "");

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => label.text = label.text.Replace("<", "").Replace(">k__Backing Field", "");
    }
}