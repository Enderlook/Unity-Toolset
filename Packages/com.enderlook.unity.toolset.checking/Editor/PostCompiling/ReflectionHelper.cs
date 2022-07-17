using Enderlook.Unity.Toolset.Utils;
using Enderlook.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking.PostCompiling
{
    internal static class ReflectionHelper
    {
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Static;

        public static IEnumerable<(Type type, T attribute)> GetAllAttributesWithCustomAttributeInPlayerAndEditorAssemblies<T>()
        {
            foreach (Type type in AssembliesHelper.GetAllAssembliesOfPlayerAndEditorAssemblies().SelectMany(e => e.GetTypes()))
                // Check if a class came from Attribute before using reflection because otherwise it would be a waste of performance
                if (type.IsSubclassOf(typeof(Attribute)) && type.GetCustomAttribute(typeof(T), true) is T attribute)
                    yield return (type, attribute);
        }

        private static IEnumerable<(T memberInfo, Type type, Attribute attribute)> GettAllAttributesOfMembersOf<T>(Type type, Func<Type, BindingFlags, T[]> getMembers) where T : MemberInfo
        {
            foreach (T memberInfo in getMembers(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Attribute _attribute = null;
                try
                {
                    foreach (Attribute attribute in memberInfo.GetCustomAttributes())
                        _attribute = attribute;
                    //yield return (memberInfo, type, attribute);
                }
                catch (BadImageFormatException) { } // https://github.com/mono/mono/issues/17278

                if (_attribute != null)
                    yield return (memberInfo, type, _attribute);
            }
        }

        public static IEnumerable<(MemberInfo memberInfo, Type type, Attribute attribute)> GettAllAttributesOfMembersOf(Type type) => GettAllAttributesOfMembersOf(type, (e, b) => e.GetMembers(b));

        public static IEnumerable<(FieldInfo fieldInfo, Type type, Attribute attribute)> GettAllAttributesOfFieldsOf(Type type) => GettAllAttributesOfMembersOf(type, (e, b) => e.GetFields(b));

        public static IEnumerable<(PropertyInfo propertyInfo, Type type, Attribute attribute)> GettAllAttributesOfPropertiesOf(Type type) => GettAllAttributesOfMembersOf(type, (e, b) => e.GetProperties(b));

        public static IEnumerable<(MethodInfo methodInfo, Type type, Attribute attribute)> GettAllAttributesOfMethodsOf(Type type) => GettAllAttributesOfMembersOf(type, (e, b) => e.GetMethods(b));

        /// <summary>
        /// Get all member names of <paramref name="class"/> which:
        /// <list type="bullet">
        ///     <item><description>If <see cref="MethodInfo"/>, its <see cref="MethodInfo.ReturnType"/> must be <typeparamref name="T"/> and it must not require mandatory parameters (can have optionals or params).</description></item>
        ///     <item><description>If <see cref="PropertyInfo"/>, its <see cref="PropertyInfo.PropertyType"/> must be <typeparamref name="T"/> and it must have a setter.</description></item>
        ///     <item><description>If <see cref="FieldInfo"/>, its <see cref="FieldInfo.FieldType"/> must be <typeparamref name="T"/>.</description></item>
        /// </list>
        /// </summary>
        /// <param name="class">Type where member are looked for.</param>
        /// <param name="return">Return type for criteria.</param>
        /// <returns>Member names which matches the criteria.</returns>
        public static IEnumerable<string> FieldsPropertiesAndMethodsWithReturnTypeOf(this Type @class, Type @return)
        {
            foreach (FieldInfo field in @class.GetFields(bindingFlags))
            {
                if (field.FieldType.IsCastableTo(@return))
                    yield return field.Name;
            }

            foreach (PropertyInfo property in @class.GetProperties(bindingFlags))
            {
                if (property.PropertyType.IsCastableTo(@return) && property.CanRead)
                    yield return property.Name;
            }

            foreach (MethodInfo method in @class.GetMethods(bindingFlags))
            {
                if (method.ReturnType.IsCastableTo(@return) && method.HasNoMandatoryParameters())
                    yield return method.Name;
            }
        }

        /// <summary>
        /// Get all member names of <paramref name="class"/> which:
        /// <list type="bullet">
        ///     <item><description>If <see cref="MethodInfo"/>, its <see cref="MethodInfo.ReturnType"/> must be <typeparamref name="T"/> and it must not require mandatory parameters (can have optionals or params).</description></item>
        ///     <item><description>If <see cref="PropertyInfo"/>, its <see cref="PropertyInfo.PropertyType"/> must be <typeparamref name="T"/> and it must have a setter.</description></item>
        ///     <item><description>If <see cref="FieldInfo"/>, its <see cref="FieldInfo.FieldType"/> must be <typeparamref name="T"/>.</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="T">Return type for criteria.</typeparam>
        /// <param name="class">Type where member are looked for.</param>
        /// <returns>Member names which matches the criteria.</returns>
        public static IEnumerable<string> FieldsPropertiesAndMethodsWithReturnTypeOf<T>(this Type @class) => @class.FieldsPropertiesAndMethodsWithReturnTypeOf(typeof(T));
    }
}