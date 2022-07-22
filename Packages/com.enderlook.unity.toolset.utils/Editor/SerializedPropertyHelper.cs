using System;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A set of helper functions for <see cref="SerializedProperty"/>.
    /// </summary>
    public static partial class SerializedPropertyHelper
    {
        // https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs

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

            if (!source.EndsWith(">k__BackingField"))
                return source;

            ReadOnlySpan<char> span = source.AsSpan(1);
            return span.Slice(0, span.Length - ">k__BackingField".Length).ToString();
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

            string propertyPath = source.propertyPath;
            return propertyPath[propertyPath.Length - 1] == ']';
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

            ReadOnlySpan<char> path = source.propertyPath.AsSpan();
            // Is array or list size.
            int i = path.LastIndexOf(".Array.size".AsSpan());
            if (i == -1)
            {
                // Is array element.
                i = path.LastIndexOf(".Array.data[".AsSpan());
            }

            if (i != -1)
                path = path.Slice(0, i);

            path = path.Slice(path.LastIndexOf('.') + 1);

            return path.ToString();
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
            if (source is null) Helper.ThrowArgumentNullException_Source();

            ReadOnlySpan<char> path = source.propertyPath.AsSpan();
            int j = path.LastIndexOf('.');
            if (j == -1)
                j = 0;

            ReadOnlySpan<char> lastPath = path.Slice(j + 1);
            j = lastPath.LastIndexOf('[');

            if (j == -1)
                Throw();

            ReadOnlySpan<char> part = lastPath.Slice(j + 1, lastPath.Length - j - 2);

            // TODO: In .Net Standard 2.1 the .ToString() can be avoided.
            return int.Parse(part.ToString());

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
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (name is null) Helper.ThrowArgumentNullException_Name();
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
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (name is null) Helper.ThrowArgumentNullException_Name();
            if (name.Length == 0) Helper.ThrowArgumentException_NameCannotBeEmpty();

            SerializedProperty serializedProperty = source.FindPropertyRelative(name);
            if (serializedProperty is null)
                serializedProperty = source.FindPropertyRelative(GetBackingFieldName(name));
            return serializedProperty;
        }
    }
}