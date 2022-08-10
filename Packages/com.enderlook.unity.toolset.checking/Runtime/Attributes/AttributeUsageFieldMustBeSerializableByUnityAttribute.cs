using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the onwer of this attribute can only be used to decorate fields of types that can be serialized by Unity.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(Attribute), typeFlags = TypeCasting.CheckSubclassTypes)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageFieldMustBeSerializableByUnityAttribute : Attribute { }
}