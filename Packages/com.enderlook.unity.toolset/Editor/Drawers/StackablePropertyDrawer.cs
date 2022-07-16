using System.Reflection;

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
        /// If <see cref="OnGUI(Rect, SerializedPropertyInfo, GUIContent, bool)"/> should be executed.<br/>
        /// Only one attribute per script variable can return <see langword="true"/>.<br/>
        /// If multiple attributes returns <see langword="true"/>, only the method of the first attribute is executed.
        /// </summary>
        protected internal virtual bool HasOnGUI => false;

        /// <summary>
        /// The <see cref="PropertyAttribute"/> for the property or run-time <see cref="System.SerializableAttribute"/> class (if it was decorated with such attribute).
        /// </summary>
        protected internal PropertyAttribute Attribute { get; internal set; }

        /// <summary>
        /// The <see cref="System.Reflection.FieldInfo"/> for the member this property represents.
        /// </summary>
        protected internal FieldInfo FieldInfo { get; internal set; }

        protected internal virtual void BeforeOnGUI(ref Rect position, ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible) { }

        protected internal virtual void OnGUI(Rect position, SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren) { }

        protected internal virtual void AfterOnGUI(Rect position, SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, bool visible) { }

        protected internal virtual void BeforeGetPropertyHeight(ref SerializedPropertyInfo propertyInfo, ref GUIContent label, ref bool includeChildren, ref bool visible) { }

        protected internal virtual float GetPropertyHeight(SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, float height) => height;

        protected internal virtual void AfterGetPropertyHeight(SerializedPropertyInfo propertyInfo, GUIContent label, bool includeChildren, bool visible, float height) { }
    }
}