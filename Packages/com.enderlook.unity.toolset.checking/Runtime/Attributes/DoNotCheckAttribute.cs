using System;

namespace Enderlook.Unity.Toolset.Checking
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class DoNotCheckAttribute : Attribute
    {
        internal readonly Type[] ignoreTypes;

        public DoNotCheckAttribute(params Type[] attributesToNotCheck) => ignoreTypes = attributesToNotCheck;
    }
}