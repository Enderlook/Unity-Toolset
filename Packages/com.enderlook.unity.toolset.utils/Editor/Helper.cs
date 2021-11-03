using System;

namespace Enderlook.Unity.Toolset.Utils
{
    internal static class Helper
    {
        public static readonly char[] DOT_SEPARATOR = new char[] { '.' }; // TODO: On .NET standard 2.1 use string.Split(char, StringSplitOptions) instead

        public static void ThrowArgumentNullException_Asset()
            => throw new ArgumentNullException("asset");
        
        public static void ThrowArgumentNullException_Extension()
            => throw new ArgumentNullException("extension");

        public static void ThrowArgumentNullException_Name()
            => throw new ArgumentNullException("name");

        public static void ThrowArgumentNullException_NewPath()
            => throw new ArgumentNullException("newPath");

        public static void ThrowArgumentNullException_Path()
            => throw new ArgumentNullException("path");

        public static void ThrowArgumentNullException_Source()
            => throw new ArgumentNullException("source");

        public static void ThrowArgumentNullException_SubAsset()
            => throw new ArgumentNullException("subAsset");

        public static void ThrowArgumentNullException_ObjectToAdd()
            => throw new ArgumentNullException("objectToAdd");

        public static void ThrowArgumentException_ExtensionCannotBeEmpty()
            => throw new ArgumentException("Can't be empty", "extension");

        public static void ThrowArgumentException_NameCannotBeEmpty()
            => throw new ArgumentException("Can't be empty", "name");

        public static void ThrowArgumentException_NewPathCannotBeEmpty()
            => throw new ArgumentException("Can't be empty", "newPath");

        public static void ThrowArgumentException_PathCannotBeEmpty()
            => throw new ArgumentException("Can't be empty", "path");

        public static void ThrowArgumentException_SourceCannotBeEmpty()
            => throw new ArgumentException("Can't be empty", "source");
    }
}
