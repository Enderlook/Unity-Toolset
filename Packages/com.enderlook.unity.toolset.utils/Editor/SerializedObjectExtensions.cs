using Enderlook.Reflection;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A set of helper functions for <see cref="SerializedObject"/>s.
    /// </summary>
    public static class SerializedObjectExtensions
    {
        /// <summary>
        /// Get the <see cref="SerializedProperty"/> of the backing field of a property.
        /// </summary>
        /// <param name="source"><see cref="SerializedObject"/> where the <see cref="SerializedProperty"/> will be taken.</param>
        /// <param name="name">Name of the property which backing field will be get.</param>
        /// <returns><see cref="SerializedProperty"/> of the backing field of <paramref name="name"/> property.</returns>
        public static SerializedProperty FindBackingFieldOfProperty(this SerializedObject source, string name)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (name is null) Helper.ThrowArgumentNullException_Name();
            if (name.Length == 0) Helper.ThrowArgumentException_NameCannotBeEmpty();

            return source.FindProperty(ReflectionExtensions.GetBackingFieldName(name));
        }

        /// <summary>
        /// Get the <see cref="SerializedProperty"/> of the field or backing field of it property.
        /// </summary>
        /// <param name="source"><see cref="SerializedObject"/> where the <see cref="SerializedProperty"/> will be taken.</param>
        /// <param name="name">Name of the property to get.</param>
        /// <returns><see cref="SerializedProperty"/> of the field or backing field of <paramref name="name"/> property.</returns>
        public static SerializedProperty FindPropertyOrBackingField(this SerializedObject source, string name)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (name is null) Helper.ThrowArgumentNullException_Name();
            if (name.Length == 0) Helper.ThrowArgumentException_NameCannotBeEmpty();

            SerializedProperty serializedProperty = source.FindProperty(name);
            if (serializedProperty is null)
                serializedProperty = source.FindProperty(ReflectionExtensions.GetBackingFieldName(name));
            return serializedProperty;
        }
    }
}