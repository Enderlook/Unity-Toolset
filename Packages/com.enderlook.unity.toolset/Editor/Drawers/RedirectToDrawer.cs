using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(RedirectToAttribute))]
    internal sealed class RedirectToDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeGetPropertyHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => property = property.FindPropertyRelative(((RedirectToAttribute)Attribute).RedirectFieldName);

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => property = property.FindPropertyRelative(((RedirectToAttribute)Attribute).RedirectFieldName);
    }
}