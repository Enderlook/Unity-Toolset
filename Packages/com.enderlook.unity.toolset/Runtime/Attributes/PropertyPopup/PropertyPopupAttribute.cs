using System;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyPopupAttribute : Attribute
    {
        /// <summary>
        /// Name of field used to determine which property must be used.
        /// </summary>
        public readonly string modeName;

        /// <summary>
        /// Enable property popup on the class which is being decorated.
        /// </summary>
        /// <param name="modeName">Name of field used to determine which property must be used.</param>
        public PropertyPopupAttribute(string modeName) => this.modeName = modeName;
    }
}
