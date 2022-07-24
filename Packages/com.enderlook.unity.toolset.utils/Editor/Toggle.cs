using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    internal struct Toggle
    {
        public struct Yes { }

        public struct No { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsToggled<T>()
        {
            Debug.Assert(typeof(T) == typeof(Yes) || typeof(T) == typeof(No));
            return typeof(T) == typeof(Yes);
        }
    }
}