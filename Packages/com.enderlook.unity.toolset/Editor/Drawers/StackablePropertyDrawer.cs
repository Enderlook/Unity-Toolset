using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    /// <summary>
    /// Base class to derive custom stackable property drawers from.<br/>
    /// Use this to create custom drawers for your own script variables with custom <see cref="PropertyAttribute"/>.
    /// </summary>
    public abstract class StackablePropertyDrawer
    {
        /// <summary>
        /// Determines if this drawer request to be the main drawer.<br/>
        /// Only one attribute per script variable can return <see langword="true"/>.<br/>
        /// If multiple attributes returns <see langword="true"/>, only the method of the first attribute is executed.<br/>
        /// The method <see cref="IsMain(bool)"/> will be called to determine if it was choosen as the main drawer.
        /// </summary>
        protected internal virtual bool RequestMain => false;

        /// <summary>
        /// The <see cref="PropertyAttribute"/> for the property or run-time <see cref="System.SerializableAttribute"/> class (if it was decorated with such attribute).
        /// </summary>
        protected internal PropertyAttribute Attribute { get; internal set; }

        /// <summary>
        /// The <see cref="System.Reflection.FieldInfo"/> for the member this property represents.
        /// </summary>
        protected internal FieldInfo FieldInfo { get; internal set; }

        /// <summary>
        /// Determines that this drawer was choosen as the main one.
        /// This method is only executed if <see cref="RequestMain"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="isMain">If <see langword="true"/>, this drawer was choosen as the main one.</param>
        protected internal virtual void IsMain(bool isMain) { }

        protected internal virtual void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible) { }

        protected internal virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren) { }

        protected internal virtual void AfterOnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren, bool visible) { }

        protected internal virtual void BeforeGetPropertyHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible) { }

        protected internal virtual float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren, float height) => height;

        protected internal virtual float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren) => EditorGUI.GetPropertyHeight(property, label, includeChildren);

        protected internal virtual void AfterGetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren, bool visible, float height) { }
    }
}