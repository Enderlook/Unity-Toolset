using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determine relation of types to look for.
    /// </summary>
    [Flags]
    public enum TypeCasting
    {
        /// <summary>
        /// It must check for the same exact type.
        /// </summary>
        ExactMatch = 0,

        /// <summary>
        /// Whenever it should check if the type is a subclass of one of the listed types.
        /// </summary>
        CheckSubclassTypes = 1 << 0,

        /// <summary>
        /// Whenever it should check if the type is superclass of one of the listed types.
        /// </summary>
        CheckSuperclassTypes = 1 << 1,

        /// <summary>
        /// Whenever it should check for assignable from type to one of the listed types.
        /// </summary>
        CheckIsAssignableTypes = 1 << 2,

        /// <summary>
        /// <see cref="CheckSubclassTypes"/> or <see cref="CheckIsAssignableTypes"/>.
        /// </summary>
        CheckSubclassOrAssignable = CheckSubclassTypes | CheckIsAssignableTypes,

        /// <summary>
        /// Whenever it should check if type can be assigned to one of the listed types.
        /// </summary>
        CheckCanBeAssignedTypes = 1 << 3,

        /// <summary>
        /// <see cref="CheckIsAssignableTypes"/> or <see cref="CheckCanBeAssignedTypes"/>.
        /// </summary>
        CheckSuperClassOrCanBeAssigned = CheckIsAssignableTypes | CheckCanBeAssignedTypes,
    };
}