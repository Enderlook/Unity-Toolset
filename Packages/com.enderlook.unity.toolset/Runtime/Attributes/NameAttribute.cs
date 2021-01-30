using Enderlook.Unity.Toolset.Checking;

using System;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageAccessibility(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NameAttribute : PropertyAttribute
    {
        public readonly string name;

        public NameAttribute(string name) => this.name = name;
    }
}