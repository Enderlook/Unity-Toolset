using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Make the drawn serialized property to only be editable when inspector is not playing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    public sealed class InitOnlyAttribute : PropertyAttribute { }
}