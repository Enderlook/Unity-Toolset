using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the onwer of this attribute can only be used to decorate fields or properties that matches a certain type criteria.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(Attribute), typeFlags = TypeRelationship.IsSubclassOf)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageRequireDataTypeAttribute : Attribute
    {
#if UNITY_EDITOR
        internal readonly Type[] basicTypes;
#endif

        /// <summary>
        /// Additional checking rules.
        /// </summary>
        public TypeRelationship typeFlags = TypeRelationship.IsEqualOrAssignableFrom;

        /// <summary>
        /// If <see langword="true"/>, <see cref="Types"/> will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, they will be the only allowed types (white list).
        /// </summary>
        public bool isBlackList;

        /// <summary>
        /// If <see langword="true"/>, it will also check the element type of field of array o list types.<br/>
        /// Useful because Unity <see cref="UnityEditor.PropertyDrawer"/> are draw on each element of an array or list <see cref="UnityEditor.SerializedProperty"/>.
        /// </summary>
        public bool supportEnumerableFields;

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="types">Data types allowed. Set <see cref="isBlackList"/> to <see langword="true"/> in order to become it forbidden data types.</param>
        public AttributeUsageRequireDataTypeAttribute(params Type[] types)
        {
#if UNITY_EDITOR
            basicTypes = types;
#endif
        }
    }
}