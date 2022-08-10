using System;

namespace Enderlook.Unity.Toolset.Checking
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class DoNotCheckAttribute : Attribute
    {
#if UNITY_EDITOR
        internal readonly Type[] ignoreTypes;
#endif

        public DoNotCheckAttribute(params Type[] attributesToNotCheck)
        {
#if UNITY_EDITOR
            ignoreTypes = attributesToNotCheck;
#endif
        }
    }
}