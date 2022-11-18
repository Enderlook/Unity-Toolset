using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the onwer of this attribute can only be used to decorate fields or properties that matches a certain type criteria.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(Attribute))]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageRequireDataTypeAttribute : Attribute
    {
#if UNITY_EDITOR
        internal readonly Type[] types;
        internal readonly TypeRelationship typeRelationship = TypeRelationship.IsEqualOrAssignableFrom;
        internal readonly bool isBlackList;
        internal readonly bool supportEnumerables;
#endif

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="types">Data types allowed.</param>
        public AttributeUsageRequireDataTypeAttribute(params Type[] types)
        {
#if UNITY_EDITOR
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="supportEnumerables">If <see langword="true"/>, it will also check the element type of field of array o list types.<br/>
        /// Useful because Unity <see cref="UnityEditor.PropertyDrawer"/> are drawn on each element of an array or list <see cref="UnityEditor.SerializedProperty"/>.</param>
        /// <param name="types">Data types allowed.</param>
        public AttributeUsageRequireDataTypeAttribute(bool supportEnumerables, params Type[] types)
        {
#if UNITY_EDITOR
            this.supportEnumerables = supportEnumerables;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="typeRelationship">Specifies checking rules for types in <paramref name="types"/>.</param>
        /// <param name="types">Data types allowed.</param>
        public AttributeUsageRequireDataTypeAttribute(TypeRelationship typeRelationship, params Type[] types)
        {
#if UNITY_EDITOR
            this.typeRelationship = typeRelationship;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="supportEnumerables">If <see langword="true"/>, it will also check the element type of array o list types.<br/>
        /// Useful because Unity <see cref="UnityEditor.PropertyDrawer"/> are drawn on each element of an array or list <see cref="UnityEditor.SerializedProperty"/>.</param>
        /// <param name="typeRelationship">Specifies checking rules for types in <paramref name="types"/>.</param>
        /// <param name="types">Data types allowed.</param>
        public AttributeUsageRequireDataTypeAttribute(bool supportEnumerables, TypeRelationship typeRelationship, params Type[] types)
        {
#if UNITY_EDITOR
            this.supportEnumerables = supportEnumerables;
            this.typeRelationship = typeRelationship;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="isBlackList">If <see langword="true"/>, <paramref name="types"/> will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, <paramref name="types"/> will be the only allowed types (whitelist).</param>
        /// <param name="supportEnumerables">If <see langword="true"/>, it will also check the element type of array o list types.<br/>
        /// Useful because Unity <see cref="UnityEditor.PropertyDrawer"/> are drawn on each element of an array or list <see cref="UnityEditor.SerializedProperty"/>.</param>
        /// <param name="types">Data types allowed.</param>
        public AttributeUsageRequireDataTypeAttribute(bool isBlackList, bool supportEnumerables, params Type[] types)
        {
#if UNITY_EDITOR
            this.isBlackList = isBlackList;
            this.supportEnumerables = supportEnumerables;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="isBlackList">If <see langword="true"/>, <paramref name="types"/> will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, <paramref name="types"/> will be the only allowed types (whitelist).</param>
        /// <param name="supportEnumerables">If <see langword="true"/>, it will also check the element type of array o list types.<br/>
        /// Useful because Unity <see cref="UnityEditor.PropertyDrawer"/> are drawn on each element of an array or list <see cref="UnityEditor.SerializedProperty"/>.</param>
        /// <param name="typeFlags">Specifies checking rules for types in <paramref name="types"/>.</param>
        /// <param name="types">Data types allowed.</param>
        public AttributeUsageRequireDataTypeAttribute(bool isBlackList = false, bool supportEnumerables = false, TypeRelationship typeFlags = TypeRelationship.IsEqualOrAssignableFrom, params Type[] types)
        {
#if UNITY_EDITOR
            this.isBlackList = isBlackList;
            this.supportEnumerables = supportEnumerables;
            this.typeRelationship = typeFlags;
            this.types = types;
#endif
        }
    }
}