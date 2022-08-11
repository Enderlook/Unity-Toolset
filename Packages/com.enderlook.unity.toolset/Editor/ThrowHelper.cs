using System;

namespace Enderlook.Unity.Toolset
{
    internal static class ThrowHelper
    {
        public static void ThrowArgumentNullExceptionProperty()
            => throw new ArgumentNullException("property");
    }
}