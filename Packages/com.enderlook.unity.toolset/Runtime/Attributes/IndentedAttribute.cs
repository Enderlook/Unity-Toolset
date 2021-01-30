using Enderlook.Unity.Toolset.Checking;

using System;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    /// <summary>
    /// Add or remove indentation to the drew serialized property.
    /// </summary>
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IndentedAttribute : PropertyAttribute
    {
        /// <summary>
        /// Indentation to add.
        /// </summary>
        public readonly int indentationOffset;

        /// <summary>
        /// Add or remove indentation to the drew serialized property.
        /// </summary>
        /// <param name="indentationOffset">Indentation to add. Negative values remove indentation.</param>
        public IndentedAttribute(int indentationOffset = 1) => this.indentationOffset = indentationOffset;
    }
}