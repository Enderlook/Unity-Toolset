using System;

namespace Enderlook.Unity.Toolset.Checking
{
    /// <summary>
    /// Determines that the owner of this attribute has an attribute that should not be inspected by the post compiling checker.
    /// </summary>
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