﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A set of helper functions for <see cref="SerializedProperty"/>.
    /// </summary>
    public static partial class SerializedPropertyHelper
    {
        // https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs

        private static readonly Regex BACKING_FIELD_REGEX = new Regex("^<(.*)>k__BackingField", RegexOptions.Compiled);
        private static readonly Regex IS_ARRAY_REGEX = new Regex(@"Array.data\[\d+\]$", RegexOptions.Compiled);
        private static readonly string[] ARRAY_DATA_SEPARATOR = new string[] { ".Array.data[" };
        private static readonly char[] OPEN_BRACKET_SEPARATOR = new char[] { '[' }; // TODO: On .NET standard 2.1 use string.Split(char, StringSplitOptions) instead

        /// <summary>
        /// Get the name of the backing field of a property.
        /// </summary>
        /// <param name="source">Name of the property.</param>
        /// <returns>Name of the backing field.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static string GetBackingFieldName(string source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            return string.Concat("<", source, ">k__BackingField");
        }

        /// <summary>
        /// Get the name of the property of a backing field.
        /// </summary>
        /// <param name="source">Name of the backing field.</param>
        /// <returns>Name of the property field.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static string GetPropertyNameOfPropertyWithBackingField(string source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            Match match = BACKING_FIELD_REGEX.Match(source);
            if (match.Length == 0)
                return source;
            return match.Groups[1].Value;
        }

        /// <summary>
        /// Check if <paramref name="source"/> is an element from an array or list.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> to check.</param>
        /// <returns>Whenever it's an element of an array or list, or not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool IsArrayOrListElement(this SerializedProperty source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            return IS_ARRAY_REGEX.IsMatch(source.propertyPath);
        }

        /// <summary>
        /// Get the last field name of the <paramref name="source"/>.<br/>
        /// For array or list index or element it returns the field name instead of the index of the size property.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> where field name is gotten.</param>
        /// <returns>Last field name of <paramref name="source"/>.</returns>
        public static string GetFieldName(this SerializedProperty source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            string path;
            if (source.IsArrayOrListSize())
                path = source.propertyPath.Substring(0, source.propertyPath.Length - ".Array.size".Length);
            else if (source.IsArrayOrListElement())
            {
                string[] tmp = source.propertyPath.Split(ARRAY_DATA_SEPARATOR, StringSplitOptions.None);
                path = tmp[tmp.Length - 2];
            }
            else
                path = source.propertyPath;
            {
                string[] tmp = path.Split(Helper.DOT_SEPARATOR);
                return tmp[tmp.Length - 1];
            }
        }

        /// <summary>
        /// Check if <paramref name="source"/> is the array or list size.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> to check.</param>
        /// <returns>Whenever it's the array or list size, or not</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool IsArrayOrListSize(this SerializedProperty source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            return source.propertyPath.EndsWith("Array.size");
        }

        /// <summary>
        /// Get the index of the <paramref name="source"/> if it's an element of an array.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> element of array.</param>
        /// <returns>Its index.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> doesn't come from an array.</exception>
        public static int GetIndexFromArray(this SerializedProperty source)
        {
            if (source == null) Helper.ThrowArgumentNullException_Source();

            string part = source.propertyPath.Split(Helper.DOT_SEPARATOR).Last().Split(OPEN_BRACKET_SEPARATOR).LastOrDefault();
            if (part == default)
            {
                Throw();
                return 0;
            }
            else
                return int.Parse(part.Replace("]", ""));

            void Throw() => throw new ArgumentException("It doesn't come from an array", nameof(source));
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty"/> of the backing field of a property.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> where the <see cref="SerializedProperty"/> will be taken.</param>
        /// <param name="name">Name of the property which backing field will be get.</param>
        /// <returns><see cref="SerializedProperty"/> of the backing field of <paramref name="name"/> property.</returns>
        public static SerializedProperty FindRelativeBackingFieldOfProperty(this SerializedProperty source, string name)
        {
            if (source == null) Helper.ThrowArgumentNullException_Source();
            if (name == null) Helper.ThrowArgumentNullException_Name();
            if (name.Length == 0) Helper.ThrowArgumentException_NameCannotBeEmpty();

            return source.FindPropertyRelative(GetBackingFieldName(name));
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty"/> of the field or backing field of it property.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> where the <see cref="SerializedProperty"/> will be taken.</param>
        /// <param name="name">Name of the property to get.</param>
        /// <returns><see cref="SerializedProperty"/> of the field or backing field of <paramref name="name"/> property.</returns>
        public static SerializedProperty FindRelativePropertyOrBackingField(this SerializedProperty source, string name)
        {
            if (source == null) Helper.ThrowArgumentNullException_Source();
            if (name == null) Helper.ThrowArgumentNullException_Name();
            if (name.Length == 0) Helper.ThrowArgumentException_NameCannotBeEmpty();

            SerializedProperty serializedProperty = source.FindPropertyRelative(name);
            if (serializedProperty == null)
                serializedProperty = source.FindPropertyRelative(GetBackingFieldName(name));
            return serializedProperty;
        }
    }
}