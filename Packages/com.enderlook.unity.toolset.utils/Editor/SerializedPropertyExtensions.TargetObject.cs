using System;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEditor;

namespace Enderlook.Unity.Toolset.Utils
{
    public static partial class SerializedPropertyExtensions
    {
        /// <summary>
        /// Gets the target object hierarchy of <paramref name="source"/>. It does work for nested serialized properties.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="preferNullInsteadOfException">If <see langword="true"/>, it will return <see langword="null"/> instead of throwing exceptions if can't find objects.</param>
        /// <returns>Hierarchy traveled to get the target object.</returns>
        private static object GetLoopTargetObjectOfProperty(this SerializedProperty source, int count, bool preferNullInsteadOfException = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            string path = source.propertyPath.Replace(".Array.data[", "[");
            string[] pathSections = path.Split(dotSeparator);

            int total = pathSections.Length;
            int remaining = total - count;

            if (count > total)
                throw new ArgumentOutOfRangeException(nameof(count), "Value was too large for this path.");

            object obj = source.serializedObject.targetObject;

            if (remaining-- == 0)
                return obj;

            string GetNotFoundMessage(string element) => $"The element {element} was not found in {obj?.GetType().ToString() ?? "<NULL>"} from {source.name} in path {path}.";

            foreach (string element in pathSections)
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = int.Parse(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    try
                    {
                        obj = obj.GetValue(elementName, index);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        if (!preferNullInsteadOfException)
                            throw new IndexOutOfRangeException($"The element {element} has no index {index} in {obj.GetType()} from {source.name} in path {path}.", e);
                        else
                            obj = null;
                    }

                    if (obj == null)
                    {
                        if (!preferNullInsteadOfException)
                            throw new KeyNotFoundException(GetNotFoundMessage(element));
                        else
                            return null;
                    }

                    if (remaining-- == 0)
                        return obj;
                }
                else
                {
                    obj = obj.GetValue(element);

                    if (obj == null)
                    {
                        if (!preferNullInsteadOfException)
                            throw new KeyNotFoundException(GetNotFoundMessage(element));
                        else
                            return null;
                    }

                    if (remaining-- == 0)
                        return obj;
                }
            }

            Debug.Assert(false, "Impossible state.");
            return null;
        }

        /// <summary>
        /// Gets the target object hierarchy of <paramref name="source"/>. It does work for nested serialized properties.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="includeItself">If <see langword="true"/> the first returned element will be <c><paramref name="source"/>.serializedObject.targetObject</c>.</param>
        /// <param name="preferNullInsteadOfException">If <see langword="true"/>, it will return <see langword="null"/> instead of throwing exceptions if can't find objects.</param>
        /// <returns>Hierarchy traveled to get the target object.</returns>
        private static IEnumerable<object> GetEnumerableTargetObjectOfProperty(this SerializedProperty source, bool includeItself = true, bool preferNullInsteadOfException = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return Method();

            IEnumerable<object> Method()
            {
                string path = source.propertyPath.Replace(".Array.data[", "[");

                object obj = source.serializedObject.targetObject;

                if (includeItself)
                    yield return obj;

                string GetNotFoundMessage(string element) => $"The element {element} was not found in {obj.GetType()} from {source.name} in path {path}.";

                foreach (string element in path.Split(dotSeparator))
                {
                    if (element.Contains("["))
                    {
                        string elementName = element.Substring(0, element.IndexOf("["));
                        int index = int.Parse(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                        try
                        {
                            obj = obj.GetValue(elementName, index);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            if (!preferNullInsteadOfException)
                                throw new IndexOutOfRangeException($"The element {element} has no index {index} in {obj.GetType()} from {source.name} in path {path}.", e);
                            else
                                obj = null;
                        }
                        if (obj == null && !preferNullInsteadOfException)
                            throw new KeyNotFoundException(GetNotFoundMessage(element));
                        yield return obj;
                    }
                    else
                    {
                        obj = obj.GetValue(element);
                        if (obj == null && !preferNullInsteadOfException)
                            throw new KeyNotFoundException(GetNotFoundMessage(element));
                        yield return obj;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the target object of <paramref name="source"/>. It does work for nested serialized properties.
        /// </summary>
        /// <param name="source"><see cref="SerializedProperty"/> whose value will be get.</param>
        /// <param name="last">At which depth from last to first should return.</param>
        /// <param name="preferNullInsteadOfException">If <see langword="true"/>, it will return <see langword="null"/> instead of throwing exceptions if can't find objects.</param>
        /// <returns>Value of the <paramref name="source"/> as <see cref="object"/>.</returns>
        public static object GetTargetObjectOfProperty(this SerializedProperty source, int last = 0, bool preferNullInsteadOfException = true)
            => source.GetLoopTargetObjectOfProperty(last, preferNullInsteadOfException);

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