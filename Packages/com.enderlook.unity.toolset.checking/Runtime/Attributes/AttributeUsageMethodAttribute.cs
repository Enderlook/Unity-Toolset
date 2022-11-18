using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the onwer of this attribute can only be used to decorate methods that matches a certain criteria.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(Attribute))]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageMethodAttribute : Attribute
    {
#if UNITY_EDITOR
        internal readonly int? parameterIndex;
        internal readonly ParameterModifier parameterModifier;
        internal readonly TypeRelationship typeRelationship;
        internal readonly bool isBlackList;
        internal readonly Type[] types;
#endif

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="types">Data types allowed as return type of the method.<br/>
        /// Pass <see cref="void"/> for no return type.</param>
        public AttributeUsageMethodAttribute(params Type[] types)
        {
#if UNITY_EDITOR
            parameterIndex = null;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="isBlackList">If <see langword="true"/>, <paramref name="types"/> will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, <paramref name="types"/> will be the only allowed types (whitelist).</param>
        /// <param name="types">Data types allowed as return type of the method.<br/>
        /// Pass <see cref="void"/> for no return type.</param>
        public AttributeUsageMethodAttribute(bool isBlackList = false, params Type[] types)
        {
#if UNITY_EDITOR
            parameterIndex = null;
            this.isBlackList = isBlackList;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterIndex">Index of parameter that is being checked.<br/>
        /// For return type, use an overload of the constructor that doesn't have this parameter.</param>
        /// <param name="types">Data types allowed on the parameter at index <paramref name="parameterIndex"/>.<br/>
        /// Pass <see langword="null"/> to disallow the existence of that parameter index.</param>
        public AttributeUsageMethodAttribute(int parameterIndex, params Type[] types)
        {
#if UNITY_EDITOR
            this.parameterIndex = parameterIndex;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterIndex">Index of parameter that is being checked.<br/>
        /// For return type, use an overload of the constructor that doesn't have this parameter.</param>
        /// <param name="parameterModifier">Determines modifiers of the parameter at index <paramref name="parameterIndex"/>.</param>
        /// <param name="types">Data types allowed on the parameter at index <paramref name="parameterIndex"/>.<br/>
        /// Pass <see langword="null"/> to disallow the existence of that parameter index.</param>
        public AttributeUsageMethodAttribute(int parameterIndex, ParameterModifier parameterModifier, params Type[] types)
        {
#if UNITY_EDITOR
            this.parameterIndex = parameterIndex;
            this.parameterModifier = parameterModifier;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterIndex">Index of parameter that is being checked.<br/>
        /// For return type, use an overload of the constructor that doesn't have this parameter.</param>
        /// <param name="typeRelationship">Specifies checking rules for types in <paramref name="types"/>.</param>
        /// <param name="types">Data types allowed on the parameter at index <paramref name="parameterIndex"/>.<br/>
        /// Pass <see langword="null"/> to disallow the existence of that parameter index.</param>
        public AttributeUsageMethodAttribute(int parameterIndex, TypeRelationship typeRelationship, params Type[] types)
        {
#if UNITY_EDITOR
            this.parameterIndex = parameterIndex;
            this.typeRelationship = typeRelationship;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterIndex">Index of parameter that is being checked.<br/>
        /// For return type, use an overload of the constructor that doesn't have this parameter.</param>
        /// <param name="parameterModifier">Determines modifiers of the parameter at index <paramref name="parameterIndex"/>.</param>
        /// <param name="typeRelationship">Specifies checking rules for types in <paramref name="types"/>.</param>
        /// <param name="types">Data types allowed on the parameter at index <paramref name="parameterIndex"/>.<br/>
        /// Pass <see langword="null"/> to disallow the existence of that parameter index.</param>
        public AttributeUsageMethodAttribute(int parameterIndex, ParameterModifier parameterModifier, TypeRelationship typeRelationship, params Type[] types)
        {
#if UNITY_EDITOR
            this.parameterIndex = parameterIndex;
            this.parameterModifier = parameterModifier;
            this.typeRelationship = typeRelationship;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterIndex">Index of parameter that is being checked.<br/>
        /// For return type, use an overload of the constructor that doesn't have this parameter.</param>
        /// <param name="parameterModifier">Determines modifiers of the parameter at index <paramref name="parameterIndex"/>.</param>
        /// <param name="isBlackList">If <see langword="true"/>, <paramref name="types"/> will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, <paramref name="types"/> will be the only allowed types (whitelist).</param>
        /// <param name="types">Data types allowed on the parameter at index <paramref name="parameterIndex"/>.<br/>
        /// Pass <see langword="null"/> to disallow the existence of that parameter index.</param>
        public AttributeUsageMethodAttribute(int parameterIndex, ParameterModifier parameterModifier, bool isBlackList, params Type[] types)
        {
#if UNITY_EDITOR
            this.parameterIndex = parameterIndex;
            this.parameterModifier = parameterModifier;
            this.isBlackList = isBlackList;
            this.types = types;
#endif
        }

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterIndex">Index of parameter that is being checked.<br/>
        /// For return type, use an overload of the constructor that doesn't have this parameter.</param>
        /// <param name="parameterModifier">Determines modifiers of the parameter at index <paramref name="parameterIndex"/>.</param>
        /// <param name="typeRelationship">Specifies checking rules for types in <paramref name="types"/>.</param>
        /// <param name="isBlackList">If <see langword="true"/>, <paramref name="types"/> will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, <paramref name="types"/> will be the only allowed types (whitelist).</param>
        /// <param name="types">Data types allowed on the parameter at index <paramref name="parameterIndex"/>.<br/>
        /// Pass <see langword="null"/> to disallow the existence of that parameter index.</param>
        public AttributeUsageMethodAttribute(int parameterIndex, ParameterModifier parameterModifier = ParameterModifier.None, TypeRelationship typeRelationship = TypeRelationship.IsEqualOrAssignableFrom, bool isBlackList = false, params Type[] types)
        {
#if UNITY_EDITOR
            this.parameterIndex = parameterIndex;
            this.parameterModifier = parameterModifier;
            this.typeRelationship = typeRelationship;
            this.isBlackList = isBlackList;
            this.types = types;
#endif
        }
    }
}