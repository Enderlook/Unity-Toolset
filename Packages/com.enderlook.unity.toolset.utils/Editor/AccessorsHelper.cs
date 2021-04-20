using System;
using System.Collections;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Utils
{
    internal static class AccessorsHelper
    {
        public const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static object GetValue(this object source, string name)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo fieldInfo = type.GetField(name, bindingFlags);
                if (fieldInfo != null)
                    return fieldInfo.GetValue(source);

                PropertyInfo propertyInfo = type.GetProperty(name, bindingFlags);
                if (propertyInfo != null)
                    return propertyInfo.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        public static object GetValue(this object source, string name, int index, bool preferNullInsteadOfThrowIfValueIsNull = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            object obj = source.GetValue(name);

            if (obj is null)
            {
                if (preferNullInsteadOfThrowIfValueIsNull)
                    return null;
                throw new ArgumentNullException($"Value name ({name}) in source ({source}).");
            }

            if (obj is Array array)
                return array.GetValue(index);

            if (!(obj is IEnumerable enumerable))
                throw new NotSupportedException("Can only get values for collections that implements " + nameof(IEnumerable) + ".");

            IEnumerator enumerator = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
                if (!enumerator.MoveNext())
                    throw new ArgumentOutOfRangeException($"{name} field from {source.GetType()} doesn't have an element at index {index}.");

            return enumerator.Current;
        }
    }
}