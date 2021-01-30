using System;

namespace Enderlook.Unity.Toolset.Checking
{
    [AttributeUsageRequireDataType(typeof(Attribute), typeFlags = TypeCasting.CheckSubclassTypes)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageRequireDataTypeAttribute : Attribute
    {
        internal readonly Type[] basicTypes;

        /// <summary>
        /// Additional checking rules.
        /// </summary>
        public TypeCasting typeFlags;

        /// <summary>
        /// If <see langword="true"/>, <see cref="Types"/> will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, they will be the only allowed types (white list).<br/>
        /// </summary>
        public bool isBlackList;

        /// <summary>
        /// If <see langword="true"/>, it will also check for array o list versions of types.<br/>
        /// Useful because Unity <see cref="UnityEditor.PropertyDrawer"/> are draw on each element of an array or list <see cref="UnityEditor.SerializedProperty"/>.
        /// </summary>
        public bool includeEnumerableTypes;

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper DataTypes.
        /// </summary>
        /// <param name="types">Data types allowed. Set <see cref="isBlackList"/> to <see langword="true"/> in order to become it forbidden data types.</param>
        public AttributeUsageRequireDataTypeAttribute(params Type[] types) => basicTypes = types;
    }
}