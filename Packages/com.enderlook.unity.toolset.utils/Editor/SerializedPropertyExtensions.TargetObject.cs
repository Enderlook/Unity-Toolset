using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Utils
{
    public static partial class SerializedPropertyExtensions
    {
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Gets the target object hierarchy of <paramref name="source"/>. It does work for nested serialized properties.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="count">Depth in the hierarchy to travel.</param>
        /// <param name="throwIfError">Whenever it should throw if there is an error.</param>
        /// <param name="target">Hierarchy traveled to get the target object.</param>
        /// <returns>If <see langword="true"/> <paramref name="target"/> was found. Otherwise <paramref name="target"/> contains an undefined value.</returns>
        private static bool GetLoopTargetObjectOfProperty(this SerializedProperty source, int count, bool throwIfError, out object target)
        {
            if (source == null)
            {
                if (throwIfError)
                    ThrowSourceIsNull();
                target = null;
                return false;
            }

            string path = source.propertyPath.Replace(".Array.data[", "[");
            string[] pathSections = path.Split(dotSeparator);
            int total = pathSections.Length;

            if (count > total)
            {
                if (throwIfError)
                    ThrowCountMustBeLowerThanTotal();
                target = null;
                return false;
            }

            int remaining = total - count;
            target = source.serializedObject.targetObject;

            if (remaining-- == 0)
                return true;

            if (target == null)
            {
                if (throwIfError)
                    ThrowTargetObjectIsNull();
                return false;
            }

            for (int i = 0; i < pathSections.Length; i++)
            {
                string element = pathSections[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = int.Parse(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                    if (!SetObj(elementName, target, out target))
                        return false;

                    if (target is null)
                    {
                        if (throwIfError)
                            ThrowArrayDataIsNull();
                        return false;
                    }

                    void ThrowArrayDataIsNull()
                        => throw new ArgumentException($"source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[")}.{elementName}.Array.data");

                    if (target is IList list)
                    {
                        if (list.Count < index)
                        {
                            target = list[index];
                            goto next;
                        }
                        else if (throwIfError)
                            ThrowIndexMustBeLowerThanArraySize();
                        else
                            return false;
                    }

                    void ThrowIndexMustBeLowerThanArraySize()
                        => throw new ArgumentException($"source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[")}", $"Index {index} at 'source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[")}' must be lower than 'source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[")}.{elementName}.Array.arraySize' ({list.Count})");

                    if (target is IEnumerable enumerable)
                    {
                        IEnumerator enumerator = enumerable.GetEnumerator();

                        for (int j = 0; j <= index; j++)
                        {
                            if (!enumerator.MoveNext())
                            {
                                if (throwIfError)
                                    ThrowEnumerableExhausted();
                                return false;
                            }

                            void ThrowEnumerableExhausted()
                                => throw new ArgumentException($"source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[")}", $"Index {index} at 'source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[")}' must be lower than 'source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[")}.{elementName}.Array.arraySize' ({j})");
                        }

                        target = enumerator.Current;
                        goto next;
                    }
                    else
                        Debug.Assert(false, "Impossible state.");

                    next:;
                }
                else if (!SetObj(element, target, out target))
                    return false;

                if (remaining-- == 0)
                    return true;

                if (target == null)
                {
                    if (throwIfError)
                        ThrowTargetNull();
                    return false;

                    void ThrowTargetNull()
                        => throw new ArgumentException($"source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i + 1)).Replace("[", ".Array.data[")}");
                }

                bool SetObj(string name, object target_, out object target__)
                {
                    Type type = target_.GetType();

                    while (true)
                    {
                        FieldInfo fieldInfo = type.GetField(name, bindingFlags);
                        if (fieldInfo != null)
                        {
                            target__ = fieldInfo.GetValue(target_);
                            break;
                        }

                        PropertyInfo propertyInfo = type.GetProperty(name, bindingFlags);
                        if (propertyInfo != null)
                        {
                            target__ = propertyInfo.GetValue(target_, null);
                            break;
                        }

                        type = type.BaseType;

                        if (type is null)
                        {
                            if (throwIfError)
                                ThrowMemberNotFound();
                            target__ = default;
                            return false;
                        }
                    }

                    return true;

                    void ThrowMemberNotFound() => throw new InvalidOperationException($"From path 'source.serializedObject.targetObject.{source.propertyPath}', member '{name}' (at 'source.serializedObject.targetObject.{string.Join(".", pathSections.Take(i)).Replace("[", ".Array.data[")}.{name}') was not found.");
                }
            }

            Debug.Assert(false, "Impossible state.");
            return false;

            void ThrowSourceIsNull()
                => throw new ArgumentNullException(nameof(source));

            void ThrowCountMustBeLowerThanTotal()
                => throw new ArgumentOutOfRangeException(nameof(count), $"Path of '{source.displayName}' property ('{source.propertyPath}') contains {total} sections, but count was {count}, which is not a lower number.");

            void ThrowTargetObjectIsNull()
                => throw new ArgumentException("source.serializedObject.targetObject");
        }

        /// <summary>
        /// Gets the target object of <paramref name="source"/>. It does work for nested serialized properties.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="last">At which depth from last to first should return.</param>
        /// <param name="preferNullInsteadOfException">If <see langword="true"/>, it will return <see langword="null"/> instead of throwing exceptions if can't find objects.</param>
        /// <returns>Value of the <paramref name="source"/> as <see cref="object"/>.</returns>
        public static object GetTargetObjectOfProperty(this SerializedProperty source, int last = 0, bool preferNullInsteadOfException = true)
        {
            if (source.GetLoopTargetObjectOfProperty(last, preferNullInsteadOfException, out object target))
                return target;
            return null;
        }

        /// <summary>
        /// Gets the parent target object of <paramref name="source"/>. It does work for nested serialized properties.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="preferNullInsteadOfException">If <see langword="true"/>, it will return <see langword="null"/> instead of throwing exceptions if can't find objects.</param>
        /// <returns>Value of the <paramref name="source"/> as <see cref="object"/>.</returns>
        public static object GetParentTargetObjectOfProperty(this SerializedProperty source, bool preferNullInsteadOfException = true)
            => source.GetTargetObjectOfProperty(1, preferNullInsteadOfException);
    }
}