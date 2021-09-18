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
        private static readonly object TRUE = true;

        public readonly DisplayMode displayMode;
        public readonly string firstProperty;
        public readonly string secondProperty;
        public readonly object compareTo;
        public readonly ComparisonMode comparison;
        public readonly Mode mode;
        public readonly bool chain;

        public enum Mode
        {
            WithObject,
            WithProperty,
            Single,
        }

        /// <summary>
        /// Hide, show, enable or disable a property depending on a condition.
        /// </summary>
        /// <param name="property">Value compared to <paramref name="compareTo"/>.</param>
        /// <param name="compareTo">The conditional will be compated to this value.</param>
        /// <param name="comparison">Comparison mode between <paramref name="property"/> and <paramref name="compareTo"/>.</param>
        /// <param name="mode">How property should be displayed.</param>
        /// <param name="chain">If <see cref="true"/> and <paramref name="property"/> has attribute <see cref="ShowIfAttribute"/>, this will execute only if <paramref name="property"/> was success.</param>
        public ShowIfAttribute(string property, object compareTo, ComparisonMode comparison = ComparisonMode.Equal, DisplayMode mode = DisplayMode.ShowHide, bool chain = true)
        {
            this.mode = Mode.WithObject;
            displayMode = mode;
            firstProperty = property;
            this.compareTo = compareTo;
            this.comparison = comparison;
            this.chain = chain;
        }

        /// <summary>
        /// Hide, show, enable or disable a property depending on a condition.
        /// </summary>
        /// <param name="comparison">Comparison mode between <paramref name="firstProperty"/> and <paramref name="secondProperty"/>.</param>
        /// <param name="firstProperty">Value compared to <paramref name="secondProperty"/>.</param>
        /// <param name="secondProperty">Value compared to <paramref name="firstProperty"/>.</param>
        /// <param name="mode">How property should be displayed.</param>
        /// <param name="chain">If <see cref="true"/> and <paramref name="property"/> has attribute <see cref="ShowIfAttribute"/>, this will execute only if <paramref name="property"/> was success.</param>
        public ShowIfAttribute(ComparisonMode comparison, string firstProperty, string secondProperty, DisplayMode mode = DisplayMode.ShowHide, bool chain = true)
        {
            this.mode = Mode.WithProperty;
            displayMode = mode;
            this.firstProperty = firstProperty;
            this.secondProperty = secondProperty;
            this.comparison = comparison;
            this.chain = chain;
        }

        /// <summary>
        /// Hide, show, enable or disable a property depending if <paramref name="property"/> is <see cref="true"/>.
        /// </summary>
        /// <param name="chain">If <see cref="true"/> and <paramref name="property"/> has attribute <see cref="ShowIfAttribute"/>, this will execute only if <paramref name="property"/> was success.</param>
        public ShowIfAttribute(string property, DisplayMode mode = DisplayMode.ShowHide, bool chain = true)
        {
            this.mode = Mode.Single;
            displayMode = mode;
            firstProperty = property;
            compareTo = TRUE;
            comparison = ComparisonMode.Equal;
            this.chain = chain;
        }
    }
}