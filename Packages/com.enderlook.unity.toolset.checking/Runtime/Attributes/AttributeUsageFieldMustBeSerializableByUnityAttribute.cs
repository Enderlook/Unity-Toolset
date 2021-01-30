using System;

namespace Enderlook.Unity.Toolset.Checking
{
    [AttributeUsageRequireDataType(typeof(Attribute), typeFlags = TypeCasting.CheckSubclassTypes)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageFieldMustBeSerializableByUnityAttribute : Attribute { }
}