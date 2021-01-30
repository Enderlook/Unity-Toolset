using System;

namespace Enderlook.Unity.Toolset.Checking
{
    [AttributeUsageRequireDataType(typeof(Attribute), typeFlags = TypeCasting.CheckSubclassTypes)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageMethodAttribute : Attribute
    {
        /// <summary>
        /// Number of the parameter to check.<br/>
        /// Example:<br/>
        ///     • 0 -> Return type.<br/>
        ///     • 1 -> First method parameter.<br/>
        ///     • 2 -> Second method parameter<br/>
        ///     • 3 -> Third method parameter...
        /// </summary>
        internal readonly int parameterNumber;

        /// <summary>
        /// Determine the type of parameter. Use <see cref="ParameterMode.VoidOrNone"/> to specify that it should not exist.
        /// </summary>
        public ParameterMode parameterType;

        /// <summary>
        /// Additional checking rules.
        /// </summary>
        public TypeCasting checkingFlags;

        internal readonly Type[] basicTypes;

        /// <summary>
        /// Each time Unity compile script, they will be analyzed to check if the attribute is being used in proper methods.
        /// </summary>
        /// <param name="parameterNumber">Number of the parameter to check.<br/>
        /// Example:<br/>
        ///     • 0 -> Return type.<br/>
        ///     • 1 -> First method parameter.<br/>
        ///     • 2 -> Second method parameter<br/>
        ///     • 3 -> Third method parameter...</param>
        /// <param name="types">Data types allowed.<br/>
        /// If empty, any data type is allowed.<br/>
        /// To specify that a parameter should not exist, use <see cref="ParameterMode.VoidOrNone"/> in <see cref="parameterType"/>.</param>
        public AttributeUsageMethodAttribute(int parameterNumber, params Type[] types)
        {
            this.parameterNumber = parameterNumber;
            basicTypes = types;
        }
    }
}