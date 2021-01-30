using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageRequireDataType(typeof(UnityEngine.Object), includeEnumerableTypes = true, typeFlags = TypeCasting.CheckSuperClassOrCanBeAssigned)]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RestrictTypeAttribute : PropertyAttribute
    {
        public readonly Type[] restriction;

        /// <summary>
        /// Restrict the values of this field by only types which implement, inherit or can be casted to all the types from <paramref name="restriction"/>.<br/>
        /// Additionally, the values of this field must inherit from <see cref="UnityEngine.Object"/>.
        /// </summary>
        /// <param name="restriction">Values must implement, inherit or be castable to all this types.<br/>
        /// If a type is a class, it must inherit from <see cref="UnityEngine.Object"/>. Struct types are not allowed.</param>
        public RestrictTypeAttribute(params Type[] restriction) => this.restriction = restriction;
    }
}