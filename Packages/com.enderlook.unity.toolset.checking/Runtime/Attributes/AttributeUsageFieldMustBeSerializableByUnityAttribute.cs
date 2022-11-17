using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the onwer of this attribute can only be used to decorate fields of types that can be serialized by Unity.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(Attribute), typeFlags = TypeCasting.CheckSubclassTypes)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageFieldMustBeSerializableByUnityAttribute : Attribute
    {
#if UNITY_EDITOR
        internal readonly bool notSupportEnumerableFields;
#endif

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="notSupportEnumerableFields">If <see langword="true"/>, it will disallow arrays or lists regardless if they are serializable by Unity.</param>
        public AttributeUsageFieldMustBeSerializableByUnityAttribute(bool notSupportEnumerableFields = false)
        {
#if UNITY_EDITOR
            this.notSupportEnumerableFields = notSupportEnumerableFields;
#endif
        }
    }
}