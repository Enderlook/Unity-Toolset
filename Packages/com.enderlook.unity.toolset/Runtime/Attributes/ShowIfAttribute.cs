using Enderlook.Unity.Toolset.Checking;

using System;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageAccessibility(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        /// <summary>
        /// Action to take depending of the condition.
        /// </summary>
        public enum ActionMode
        {
            /// <summary>
            /// The property will be hidden or show depending of the condition.
            /// </summary>
            ShowHide,

            /// <summary>
            /// The property will be disabled or enabled depending of the condition.
            /// </summary>
            EnableDisable,
        }

        public readonly string nameOfConditional;
        public readonly ActionMode mode;
        public readonly bool mustBeEqual;
        public readonly object compareTo;
        public readonly Type memberType;

        /// <summary>
        /// Action to take depending of the condition.
        /// </summary>
        /// <param name="nameOfConditional">Action to take depending of the condition.</param>
        /// <param name="memberType">Type of the <see cref="nameOfConditional"/>.</param>
        /// <param name="compareTo">The conditional will be compated to this value.</param>
        public ShowIfAttribute(string nameOfConditional, Type memberType, object compareTo, ActionMode mode = ActionMode.ShowHide)
        {
            this.nameOfConditional = nameOfConditional;
            this.memberType = memberType;
            this.mode = mode;
            this.compareTo = compareTo;
            mustBeEqual = true;
        }

        /// <summary>
        /// Action to take depending of the condition.
        /// </summary>
        /// <param name="nameOfConditional">Action to take depending of the condition.</param>
        /// <param name="memberType">Type of the <see cref="nameOfConditional"/>.</param>
        /// <param name="compareTo">The conditional will be compated to this value.</param>
        /// <param name="mustBeEqual">Whenever the conditional value must match or not <paramref name="compareTo"/>.</param>
        /// <param name="mode"></param>
        public ShowIfAttribute(string nameOfConditional, Type memberType, object compareTo, bool mustBeEqual = true, ActionMode mode = ActionMode.ShowHide)
        {
            this.nameOfConditional = nameOfConditional;
            this.mode = mode;
            this.compareTo = compareTo;
            this.mustBeEqual = mustBeEqual;
            this.memberType = memberType;
        }

        /// <summary>
        /// Action to take depending of the condition.
        /// </summary>
        /// <param name="goal">The conditional must be a boolean value equal to this.</param>
        public ShowIfAttribute(string nameOfConditional, bool goal, ActionMode mode = ActionMode.ShowHide)
        {
            this.nameOfConditional = nameOfConditional;
            this.mode = mode;
            mustBeEqual = true;
            compareTo = goal;
            memberType = typeof(bool);
        }

        /// <summary>
        /// Action to take if <paramref name="nameOfConditional"/> results in <see langword="true"/>.
        /// </summary>
        public ShowIfAttribute(string nameOfConditional, ActionMode mode = ActionMode.ShowHide)
        {
            this.nameOfConditional = nameOfConditional;
            this.mode = mode;
            mustBeEqual = true;
            compareTo = true;
            memberType = typeof(bool);
        }
    }
}