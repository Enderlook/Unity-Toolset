using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyPopupAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Name of field used to determine which property must be used.
        /// </summary>
        internal readonly string ModeFieldName;
#endif

        /// <summary>
        /// Enable property popup on the class which is being decorated.
        /// </summary>
        /// <param name="modeFieldName">Name of field used to determine which property must be used.</param>
        public PropertyPopupAttribute(string modeFieldName)
        {
#if UNITY_EDITOR
            ModeFieldName = modeFieldName;
#endif
        }
    }
}
