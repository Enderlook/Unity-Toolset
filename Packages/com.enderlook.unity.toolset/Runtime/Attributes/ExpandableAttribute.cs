using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Make expandable the content of the drawn serialized property.
    /// </summary>
    [AttributeUsageRequireDataType(true, typeof(UnityEngine.Object))]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ExpandableAttribute : PropertyAttribute { }
}