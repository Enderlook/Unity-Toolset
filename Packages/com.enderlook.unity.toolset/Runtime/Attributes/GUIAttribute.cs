using Enderlook.Unity.Toolset.Checking;

using System;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageAccessibility(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class GUIAttribute : PropertyAttribute
    {
        /// <summary>
        /// How the name will behave
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Use the string as value.
            /// </summary>
            Value,

            /// <summary>
            /// Use the string to get the real value by reflection.
            /// </summary>
            Reference,
        }

        public readonly string name;

        public readonly string tooltip;

        public readonly Mode nameMode;

        public readonly Mode tooltipMode;

        public readonly string guiContentOrReferenceName;

        public GUIAttribute(string name) => guiContentOrReferenceName = name;

        public GUIAttribute(string name, Mode nameMode)
        {
            this.name = name;
            this.nameMode = nameMode;
        }

        public GUIAttribute(string name, string tooltip, Mode tooltipMode = Mode.Reference) : this(name)
        {
            this.tooltip = tooltip;
            this.tooltipMode = tooltipMode;
        }

        public GUIAttribute(string name, Mode nameMode, string tooltip, Mode tooltipMode = Mode.Reference) : this(name, nameMode)
        {
            this.tooltip = tooltip;
            this.tooltipMode = tooltipMode;
        }
    }
}