﻿using Enderlook.Unity.Toolset.Checking;

using System;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Attributes
{
    [AttributeUsageAccessibility(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
    [AttributeUsageFieldMustBeSerializableByUnity]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class EnableIfAttribute : PropertyAttribute
#if UNITY_EDITOR
        , IConditionalAttribute
#endif
    {
        private static readonly object TRUE = true;

        private readonly string firstProperty;
        private readonly string secondProperty;
        private readonly object compareTo;
        private readonly ComparisonMode comparison;
        private readonly bool chain;

#if UNITY_EDITOR
        private readonly IConditionalAttribute.ConditionalMode mode;

        string IConditionalAttribute.FirstProperty => firstProperty;

        string IConditionalAttribute.SecondProperty => secondProperty;

        object IConditionalAttribute.CompareTo => compareTo;

        ComparisonMode IConditionalAttribute.Comparison => comparison;

        bool IConditionalAttribute.Chain => chain;

        IConditionalAttribute.ConditionalMode IConditionalAttribute.Mode => mode;
#endif

        /// <summary>
        /// Enable or disable a property depending on a condition.
        /// </summary>
        /// <param name="property">Value compared to <paramref name="compareTo"/>.</param>
        /// <param name="compareTo">The conditional will be compated to this value.</param>
        /// <param name="comparison">Comparison mode between <paramref name="property"/> and <paramref name="compareTo"/>.</param>
        /// <param name="chain">If <see cref="true"/> and <paramref name="property"/> has attribute <see cref="ShowIfAttribute"/> or <see cref="EnableIfAttribute"/>, this will execute only if <paramref name="property"/> was success.</param>
        public EnableIfAttribute(string property, object compareTo, ComparisonMode comparison = ComparisonMode.Equal, bool chain = true)
        {
#if UNITY_EDITOR
            mode = IConditionalAttribute.ConditionalMode.WithObject;
#endif
            firstProperty = property;
            this.compareTo = compareTo;
            this.comparison = comparison;
            this.chain = chain;
        }

        /// <summary>
        /// Enable or disable a property depending on a condition.
        /// </summary>
        /// <param name="comparison">Comparison mode between <paramref name="firstProperty"/> and <paramref name="secondProperty"/>.</param>
        /// <param name="firstProperty">Value compared to <paramref name="secondProperty"/>.</param>
        /// <param name="secondProperty">Value compared to <paramref name="firstProperty"/>.</param>
        /// <param name="chain">If <see cref="true"/> and <paramref name="property"/> has attribute <see cref="ShowIfAttribute"/> or <see cref="EnableIfAttribute"/>, this will execute only if <paramref name="property"/> was success.</param>
        public EnableIfAttribute(ComparisonMode comparison, string firstProperty, string secondProperty, bool chain = true)
        {
#if UNITY_EDITOR
            mode = IConditionalAttribute.ConditionalMode.WithProperty;
#endif
            this.firstProperty = firstProperty;
            this.secondProperty = secondProperty;
            this.comparison = comparison;
            this.chain = chain;
        }

        /// <summary>
        /// Enable or disable a property depending if <paramref name="property"/> is <see cref="true"/>.
        /// </summary>
        /// <param name="chain">If <see cref="true"/> and <paramref name="property"/> has attribute <see cref="ShowIfAttribute"/> or <see cref="EnableIfAttribute"/>, this will execute only if <paramref name="property"/> was success.</param>
        public EnableIfAttribute(string property, bool chain = true)
        {
#if UNITY_EDITOR
            mode = IConditionalAttribute.ConditionalMode.Single;
#endif
            firstProperty = property;
            compareTo = TRUE;
            comparison = ComparisonMode.Equal;
            this.chain = chain;
        }
    }
}