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
        internal readonly string Name;

        internal readonly string Tooltip;

        internal readonly GUIMode NameMode;

        internal readonly GUIMode TooltipMode;

        internal readonly string GuiContentOrReferenceName;

        public GUIAttribute(string name) => Name = name;

        public GUIAttribute(string name, GUIMode nameMode = GUIMode.Value)
        {
            Name = name;
            NameMode = nameMode;
        }

        public GUIAttribute(string name, string tooltip, GUIMode tooltipMode = GUIMode.Reference)
        {
            Name = name;
            Tooltip = tooltip;
            TooltipMode = tooltipMode;
        }

        public GUIAttribute(string name, GUIMode nameMode, string tooltip, GUIMode tooltipMode = GUIMode.Reference)
        {
            Name = name;
            NameMode = nameMode;
            Tooltip = tooltip;
            TooltipMode = tooltipMode;
        }
    }
}
