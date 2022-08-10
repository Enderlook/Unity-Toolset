using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Add or remove indentation to the drawn serialized property.
    /// </summary>
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IndentedAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        /// <summary>
        /// Indentation to add.
        /// </summary>
        internal readonly int indentationOffset;
#endif

        /// <summary>
        /// Add or remove indentation to the drawn serialized property.
        /// </summary>
        /// <param name="indentationOffset">Indentation to add. Negative values remove indentation.</param>
        public IndentedAttribute(int indentationOffset = 1)
        {
#if UNITY_EDITOR
            this.indentationOffset = indentationOffset;
#endif
        }
    }
}