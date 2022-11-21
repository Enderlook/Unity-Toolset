using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Determines that this serialized type will display a single serialized property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyPopupAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Name of field used to determine which property must be used.
        /// </summary>
        internal readonly string ModeReferenceName;
#endif

        /// <summary>
        /// Enable property popup on the class which is being decorated.
        /// </summary>
        /// <param name="modeName">Name of field or property used to determine which property must be used.</param>
        public PropertyPopupAttribute(string modeName)
        {
#if UNITY_EDITOR
            ModeReferenceName = modeName;
#endif
        }
    }
}
