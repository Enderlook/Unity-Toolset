using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking
{
    public static class AttributeUsageAccessibilityExtensions
    {
        public static void CheckAllowance(this AttributeUsageAccessibilityAttribute attribute, MemberInfo memberInfo, string attributeName)
        {
            if (memberInfo.ReflectedType.GetMember(memberInfo.Name, attribute.bindingFlags).Length == 0)
                Debug.LogError($"According to {nameof(AttributeUsageAccessibilityAttribute)}, the attribute {attributeName} can only be applied in members with the following {nameof(BindingFlags)}: {attribute.bindingFlags}.");
        }
    }
}