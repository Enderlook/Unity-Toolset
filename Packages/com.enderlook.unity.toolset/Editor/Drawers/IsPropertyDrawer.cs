using Enderlook.Unity.Toolset.Attributes;

using System.Text.RegularExpressions;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(IsPropertyAttribute))]
    internal sealed class IsPropertyDrawer : StackablePropertyDrawer
    {
        private static readonly Regex backingFieldRegex = new Regex("^<(.*)>K__Backing Field", RegexOptions.Compiled);

        protected internal override bool SupportUIToolkit => true;

        protected internal override void BeforeCreatePropertyGUI(ref SerializedProperty property, ref string label, ref string tooltip)
        {
            Match match = backingFieldRegex.Match(label);
            if (match.Length > 1)
                label = match.Groups[1].Value;
        }

        protected internal override void BeforeGetPropertyHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            Match match = backingFieldRegex.Match(label.text);
            if (match.Length > 1)
                label.text = match.Groups[1].Value;
        }

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            Match match = backingFieldRegex.Match(label.text);
            if (match.Length > 1)
                label.text = match.Groups[1].Value;
        }
    }
}