using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Make the drawn serialized property readonly by the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    public sealed class ReadOnlyAttribute : PropertyAttribute { }
}