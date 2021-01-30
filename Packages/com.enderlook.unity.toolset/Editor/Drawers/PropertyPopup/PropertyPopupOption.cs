using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Drawers
{
    public struct PropertyPopupOption
    {
        public readonly string propertyName;
        public readonly string displayName;
        public readonly object target;

        public PropertyPopupOption(string propertyName, string displayName, object target)
        {
            this.propertyName = propertyName;
            this.displayName = displayName;
            this.target = target;
        }

        public PropertyPopupOption(string propertyName, object target)
            : this(propertyName, ObjectNames.NicifyVariableName(propertyName), target) { }

        public PropertyPopupOption(string propertyName, PropertyPopupOptionAttribute propertyPopupOptionAttribute)
            : this(propertyName, propertyPopupOptionAttribute.target) { }

        public PropertyPopupOption(string propertyName, string displayName, PropertyPopupOptionAttribute propertyPopupOptionAttribute)
            : this(propertyName, displayName, propertyPopupOptionAttribute.target) { }
    }
}
