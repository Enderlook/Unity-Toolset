using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Draw a layer picker in the decorated serialized property.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(int), typeof(float), typeof(LayerMask), typeof(string), includeEnumerableTypes = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LayerAttribute : PropertyAttribute { }
}