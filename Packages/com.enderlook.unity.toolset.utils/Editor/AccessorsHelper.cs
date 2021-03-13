using System;
using System.Collections;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Utils
{
    internal static class AccessorsHelper
    {
        public const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static T GetValue<T>(this object source, string name)
        {
            Type type = typeof(T);
            object value = source.GetValue(name);
            if (value is T value_)
                return value_;
            else
                throw new ArgumentException($"Memeber {nameof(name)} isn't of type {typeof(T)}, but {type}.");
        }

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

        public static T GetValue<T>(this object source, string name, int index)
        {
            Type type = typeof(T);
            object value = source.GetValue(name, index);
            if (value is T value_)
                return value_;
            else
                throw new ArgumentException($"Memeber {nameof(name)} isn't of type {typeof(T)}, but {type}.");
        }

        public static object GetValue(this object source, string name, int index)
        {
            object obj = source.GetValue(name);
            if (obj is Array array)
                return array.GetValue(index);

            if (!(obj is IEnumerable enumerable))
                return null;

            IEnumerator enumerator = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
                if (!enumerator.MoveNext())
                    throw new ArgumentOutOfRangeException($"{name} field from {source.GetType()} doesn't have an element at index {index}.");

            return enumerator.Current;
        }
    }
}