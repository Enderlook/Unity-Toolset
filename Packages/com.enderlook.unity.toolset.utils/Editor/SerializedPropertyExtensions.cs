using Enderlook.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A set of helper functions for <see cref="SerializedProperty"/>.
    /// </summary>
    public static partial class SerializedPropertyExtensions
    {
        // https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs

        private static readonly Regex isArrayRegex = new Regex(@"Array.data\[\d+\]$");
        private static readonly string[] arrayDataSeparator = new string[] { ".Array.data[" };
        private static readonly char[] dotSeparator = new char[] { '.' }; // TODO: On .NET standard 2.1 use string.Split(char, StringSplitOptions) instead
        private static readonly char[] openBracketSeparator = new char[] { '[' }; // TODO: On .NET standard 2.1 use string.Split(char, StringSplitOptions) instead

        /// <summary>
        /// Check if <paramref name="source"/> is an element from an array or list.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> to check.</param>
        /// <returns>Whenever it's an element of an array or list, or not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool IsArrayOrListElement(this SerializedProperty source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return isArrayRegex.IsMatch(source.propertyPath);
        }

        /// <summary>
        /// Get the last field name of the <paramref name="source"/>.<br/>
        /// For array or list index or element it returns the field name instead of the index of the size property.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> where field name is gotten.</param>
        /// <returns>Last field name of <paramref name="source"/>.</returns>
        public static string GetFieldName(this SerializedProperty source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            string path;
            if (source.IsArrayOrListSize())
                path = source.propertyPath.Substring(0, source.propertyPath.Length - ".Array.size".Length);
            else if (source.IsArrayOrListElement())
            {
                string[] tmp = source.propertyPath.Split(arrayDataSeparator, StringSplitOptions.None);
                path = tmp[tmp.Length - 2];
            }
            else
                path = source.propertyPath;
            {
                string[] tmp = path.Split(dotSeparator);
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
            if (source is null) throw new ArgumentNullException(nameof(source));

            return source.propertyPath.EndsWith("Array.size");
        }

        /// <summary>
        /// Get the getter and setter of <paramref name="source"/>. It does work for nested serialized properties.<br/>
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose getter and setter will be get.</param>
        /// <returns>Getter and setter of the <paramref name="source"/>.</returns>
        public static Accessors GetTargetObjectAccessors(this SerializedProperty source)
        {
            object parent = source.GetParentTargetObjectOfProperty();
            Type parentType = parent.GetType();

            string element = source.propertyPath.Replace(".Array.data[", "[").Split(dotSeparator).Last();
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = int.Parse(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                Accessors accessors = new Accessors(parent, elementName, index);
                try
                {
                    accessors.Get();
                }
                catch (ArgumentOutOfRangeException e)
                {
                    throw new IndexOutOfRangeException($"The element {element} has no index {index} in {parentType} from {source.name} in path {element}.", e);
                }
                return accessors;
            }
            else
                return new Accessors(parent, element);
        }

        /// <inheritdoc cref="GetTargetObjectAccessors(SerializedProperty)"/>
        public static Accessors<T> GetTargetObjectAccessors<T>(this SerializedProperty source)
            => new Accessors<T>(GetTargetObjectAccessors(source));

        /// <summary>
        /// Get the <see cref="FieldInfo"/> of <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose <see cref="FieldInfo"/> will be get.</param>
        /// <param name="includeInheritedPrivate">Whenever it should also search private fields of supper-classes.</param>
        /// <returns><see cref="FieldInfo"/> of <paramref name="source"/>.</returns>
        public static FieldInfo GetFieldInfo(this SerializedProperty source, bool includeInheritedPrivate = true)
        {
            Type type = source.GetParentTargetObjectOfProperty(false).GetType();
            string name = source.GetFieldName();

            if (includeInheritedPrivate)
                return type.GetInheritedField(name, AccessorsHelper.bindingFlags);
            else
                return type.GetField(name, AccessorsHelper.bindingFlags);
        }

        /// <summary>
        /// Get the <see cref="Type"/> of the property <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose <see cref="Type"/> will be get.</param>
        /// <param name="includeInheritedPrivate">Whenever it should also search private fields of supper-classes.</param>
        /// <returns><see cref="Type"/> of <paramref name="source"/>.</returns>
        public static Type GetPropertyType(this SerializedProperty source, bool includeInheritedPrivate = true)
        {
            Type type = source.GetFieldInfo(includeInheritedPrivate).FieldType;
            if (type.TryGetElementTypeOfArrayOrList(out Type elementType))
                return elementType;
            else
                return type;
        }

        /// <summary>
        /// Get the index of the <paramref name="source"/> if it's an element of an array.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> element of array.</param>
        /// <returns>Its index.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> doesn't come from an array.</exception>
        public static int GetIndexFromArray(this SerializedProperty source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            string part = source.propertyPath.Split(dotSeparator).Last().Split(openBracketSeparator).LastOrDefault();
            if (part == default)
                throw new ArgumentException("It doesn't come from an array", nameof(source));
            else
                return int.Parse(part.Replace("]", ""));
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty"/> of the backing field of a property.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> where the <see cref="SerializedProperty"/> will be taken.</param>
        /// <param name="name">Name of the property which backing field will be get.</param>
        /// <returns><see cref="SerializedProperty"/> of the backing field of <paramref name="name"/> property.</returns>
        public static SerializedProperty FindRelativeBackingFieldOfProperty(this SerializedProperty source, string name)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Length == 0) throw new ArgumentException("Can't be empty.", nameof(name));

            return source.FindPropertyRelative(ReflectionExtensions.GetBackingFieldName(name));
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty"/> of the field or backing field of it property.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> where the <see cref="SerializedProperty"/> will be taken.</param>
        /// <param name="name">Name of the property to get.</param>
        /// <returns><see cref="SerializedProperty"/> of the field or backing field of <paramref name="name"/> property.</returns>
        public static SerializedProperty FindRelativePropertyOrBackingField(this SerializedProperty source, string name)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Length == 0) throw new ArgumentException("Can't be empty.", nameof(name));

            SerializedProperty serializedProperty = source.FindPropertyRelative(name);
            if (serializedProperty == null)
                serializedProperty = source.FindPropertyRelative(ReflectionExtensions.GetBackingFieldName(name));
            return serializedProperty;
        }
    }
}