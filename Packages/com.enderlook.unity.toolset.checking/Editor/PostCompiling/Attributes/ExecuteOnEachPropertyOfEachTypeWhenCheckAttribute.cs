﻿using System;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes
{
    /// <summary>
    /// Executes the method decorated by this attribute for each property on each <see cref="Type"/> compiled by Unity each time Unity compiles code.<br/>
    /// The method to decorate must have the signature DoSomething(<see cref="PropertyInfo"/>).
    /// </summary>
    [AttributeUsageAccessibility(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)]
    [AttributeUsageMethod(1, typeof(PropertyInfo))]
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class ExecuteOnEachPropertyOfEachTypeWhenCheckAttribute : BaseExecuteWhenCheckAttribute
    {
        /// <summary>
        /// Executes the method decorated by this attribute for each property on each <see cref="Type"/> compiled by Unity.<br/>
        /// The method to decorate must have the signature DoSomething(<see cref="PropertyInfo"/>).
        /// </summary>
        /// <param name="order">In which order will this method be executed.</param>
        public ExecuteOnEachPropertyOfEachTypeWhenCheckAttribute(int order = 0) : base(order) { }
    }
}