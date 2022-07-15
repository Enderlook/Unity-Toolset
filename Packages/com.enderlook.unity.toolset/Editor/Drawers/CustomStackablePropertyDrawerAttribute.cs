using System;

namespace Enderlook.Unity.Toolset.Drawers
{
    /// <summary>
    /// Tells a custom <see cref="StackablePropertyDrawer"/> which run-time <see cref="System.SerializableAttribute"/> class or <see cref="UnityEngine.PropertyAttribute"/> it's a drawer for.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class CustomStackablePropertyDrawerAttribute : Attribute
    {
        internal readonly Type Type;
        internal readonly bool UseForChildren;

        /// <summary>
        /// Tells a custom <see cref="StackablePropertyDrawer"/> which run-time Serializable class or <see cref="UnityEngine.PropertyAttribute"/> it's a drawer for.
        /// /summary>
        /// <param name="type">If the drawer is for a custom <see cref="System.SerializableAttribute"/> class, the type should be that class.<br/>
        /// If the drawer is for script variables with a specific <see cref="StackablePropertyDrawer"/>, the type should be that attribute.</param>
        public CustomStackablePropertyDrawerAttribute(Type type) => Type = type;

        /// <summary>
        /// Tells a custom <see cref="StackablePropertyDrawer"/> which run-time Serializable class or <see cref="UnityEngine.PropertyAttribute"/> it's a drawer for.
        /// /summary>
        /// <param name="type">If the drawer is for a custom <see cref="System.SerializableAttribute"/> class, the type should be that class.<br/>
        /// If the drawer is for script variables with a specific <see cref="StackablePropertyDrawer"/>, the type should be that attribute.</param>
        /// <param name="useForChildren">If true, the drawer will be used for any children of the specified class unless they define their own drawer.</param>
        public CustomStackablePropertyDrawerAttribute(Type type, bool useForChildren)
        {
            Type = type;
            UseForChildren = useForChildren;
        }
    }
}