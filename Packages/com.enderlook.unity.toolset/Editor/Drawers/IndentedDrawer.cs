using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(IndentedAttribute))]
    internal sealed class IndentedDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
            => EditorGUI.indentLevel += ((IndentedAttribute)Attribute).indentationOffset;

        protected internal override void AfterOnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren, bool visible)
            => EditorGUI.indentLevel -= ((IndentedAttribute)Attribute).indentationOffset;
    }
}