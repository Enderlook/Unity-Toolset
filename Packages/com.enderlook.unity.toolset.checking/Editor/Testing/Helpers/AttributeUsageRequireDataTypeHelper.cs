using System;
using System.Collections.Generic;

namespace Enderlook.Unity.Toolset.Checking
{
    public struct AttributeUsageRequireDataTypeHelper
    {
        private readonly AttributeUsageRequireDataTypeAttribute attribute;

        private HashSet<Type> Types => types ?? (types = AttributeUsageHelper.GetHashsetTypes(attribute.basicTypes, attribute.includeEnumerableTypes));
        private HashSet<Type> types;

        private string allowedTypes;
        private string AllowedTypes => allowedTypes ?? (allowedTypes = AttributeUsageHelper.GetTextTypes(Types, attribute.typeFlags, attribute.isBlackList));

        public AttributeUsageRequireDataTypeHelper(AttributeUsageRequireDataTypeAttribute attribute)
        {
            this.attribute = attribute;
            types = null;
            allowedTypes = null;
        }

        /// <summary>
        /// Unity Editor only.
        /// </summary>
        /// <param name="toCheckType"></param>
        /// <param name="toCheckName"></param>
        /// <param name="attributeName"></param>
        /// <remarks>Only use in Unity Editor.</remarks>
        public void CheckAllowance(Type toCheckType, string toCheckName, string attributeName)
            => AttributeUsageHelper.CheckContains(
                nameof(AttributeUsageRequireDataTypeAttribute),
                Types,
                attribute.typeFlags,
                attribute.isBlackList,
                AllowedTypes,
                toCheckType,
                attributeName,
                toCheckName
           );
    }
}