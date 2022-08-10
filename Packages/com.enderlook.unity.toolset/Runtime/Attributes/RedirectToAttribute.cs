﻿using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    public sealed class RedirectToAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Determines which field of the type should be drawed.
        /// </summary>
        internal readonly string RedirectFieldName;
#endif

        /// <summary>
        /// Determines which field of the type should be drawed.
        /// </summary>
        /// <param name="redirectFieldName">Field that must be drawed.</param>
        public RedirectToAttribute(string redirectFieldName)
        {
#if UNITY_EDITOR
            RedirectFieldName = redirectFieldName;
#endif
        }
    }
}