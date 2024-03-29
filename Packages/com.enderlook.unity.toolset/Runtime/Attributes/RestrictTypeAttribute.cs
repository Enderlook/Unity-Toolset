﻿using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Restrict the allowed types to the drawn serialized property.
    /// </summary>
    [AttributeUsageRequireDataType(true, typeof(UnityEngine.Object))]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RestrictTypeAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        internal readonly Type[] restriction;
#endif

        /// <summary>
        /// Restrict the values of this field by only types which implement, inherit or can be casted to all the types from <paramref name="restriction"/>.<br/>
        /// Additionally, the values of this field must inherit from <see cref="UnityEngine.Object"/>.
        /// </summary>
        /// <param name="restriction">Values must implement, inherit or be castable to all this types.<br/>
        /// If a type is a class, it must inherit from <see cref="UnityEngine.Object"/>. Struct types are not allowed.</param>
        public RestrictTypeAttribute(params Type[] restriction)
        {
#if UNITY_EDITOR
            this.restriction = restriction;
#endif
        }
    }
}