using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Drawers
{
    internal readonly struct PropertyPopupOption
    {
        public readonly string PropertyName;
        public readonly string DisplayName;
        public readonly object Target;

        public PropertyPopupOption(string propertyName, string displayName, object target)
        {
            PropertyName = propertyName;
            DisplayName = displayName;
            Target = target;
        }

        public PropertyPopupOption(string propertyName, object target)
            : this(propertyName, ObjectNames.NicifyVariableName(propertyName), target) { }

        public PropertyPopupOption(string propertyName, PropertyPopupOptionAttribute propertyPopupOptionAttribute)
            : this(propertyName, propertyPopupOptionAttribute.Target) { }

        public PropertyPopupOption(string propertyName, string displayName, PropertyPopupOptionAttribute propertyPopupOptionAttribute)
            : this(propertyName, displayName, propertyPopupOptionAttribute.Target) { }
    }
}
