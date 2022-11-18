using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the onwer of this attribute can only be used to decorate methods that matches a certain criteria.
    /// </summary>
    [AttributeUsageRequireDataType(typeof(Attribute), typeFlags = TypeRelationship.IsSubclassOf)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageMethodAttribute : Attribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Number of the parameter to check.<br/>
        /// Example:<br/>
        ///     • 0 -> Return type.<br/>
        ///     • 1 -> First method parameter.<br/>
        ///     • 2 -> Second method parameter<br/>
        ///     • 3 -> Third method parameter...
        /// </summary>
        internal readonly int parameterNumber;
#endif

        /// <summary>
        /// Determine the type of parameter. Use <see cref="ParameterMode.VoidOrNone"/> to specify that it should not exist.
        /// </summary>
        public ParameterMode parameterType;

        /// <summary>
        /// Additional checking rules.
        /// </summary>
        public TypeRelationship checkingFlags;

        /// <summary>
        /// If <see langword="true"/>, types will be forbidden types (blacklist).<br/>
        /// If <see langword="false"/>, they will be the only allowed types (white list).
        /// </summary>
        public bool isBlackList;

#if UNITY_EDITOR
        internal readonly Type[] basicTypes;
#endif

        /// <summary>
        /// Each time Unity compile scripts, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterNumber">Number of the parameter to check.<br/>
        /// Example:<br/>
        ///     • 0 -> Return type.<br/>
        ///     • 1 -> First method parameter.<br/>
        ///     • 2 -> Second method parameter.<br/>
        ///     • 3 -> Third method parameter...</param>
        /// <param name="types">Data types allowed.<br/>
        /// If empty, any data type is allowed.<br/>
        /// To specify that a parameter should not exist, use <see cref="ParameterMode.VoidOrNone"/> in <see cref="parameterType"/>.</param>
        public AttributeUsageMethodAttribute(int parameterNumber, params Type[] types)
        {
#if UNITY_EDITOR
            this.parameterNumber = parameterNumber;
            basicTypes = types;
#endif
        }
    }
}