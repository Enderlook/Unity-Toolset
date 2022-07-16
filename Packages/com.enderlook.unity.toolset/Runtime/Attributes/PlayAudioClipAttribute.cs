using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageRequireDataType(typeof(AudioClip), typeof(string), includeEnumerableTypes = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PlayAudioClipAttribute : PropertyAttribute
    {
        internal readonly bool ShowProgressBar;

        public PlayAudioClipAttribute(bool showProgressBar) => ShowProgressBar = showProgressBar;

        public PlayAudioClipAttribute() { }
    }
}