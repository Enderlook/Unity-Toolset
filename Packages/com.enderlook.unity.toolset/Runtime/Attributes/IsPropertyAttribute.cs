using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageFieldMustBeSerializableByUnityAttribute]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class IsPropertyAttribute : PropertyAttribute { }
}