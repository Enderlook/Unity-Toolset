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
        internal readonly string name;

        internal readonly string tooltip;

        internal readonly GUIMode nameMode;

        internal readonly GUIMode tooltipMode;

        internal readonly string guiContentOrReferenceName;

        public GUIAttribute(string name) => guiContentOrReferenceName = name;

        public GUIAttribute(string name, GUIMode nameMode = GUIMode.Value)
        {
            this.name = name;
            this.nameMode = nameMode;
        }

        public GUIAttribute(string name, string tooltip, GUIMode tooltipMode = GUIMode.Reference)
        {
            this.name = name;
            this.tooltip = tooltip;
            this.tooltipMode = tooltipMode;
        }

        public GUIAttribute(string name, GUIMode nameMode, string tooltip, GUIMode tooltipMode = GUIMode.Reference)
        {
            this.name = name;
            this.nameMode = nameMode;
            this.tooltip = tooltip;
            this.tooltipMode = tooltipMode;
        }
    }
}
