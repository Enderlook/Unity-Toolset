using Enderlook.Reflection;
using Enderlook.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

using UnityObject = UnityEngine.Object;

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

        private static readonly Type[] unityDefaultNonPrimitiveSerializables = new Type[]
        {
            typeof(Vector2), typeof(Vector3), typeof(Vector4),
            typeof(Rect), typeof(Quaternion), typeof(Matrix4x4),
            typeof(Color), typeof(Color32), typeof(LayerMask),
            typeof(AnimationCurve), typeof(Gradient), typeof(RectOffset), typeof(GUIStyle)
        };

        /// <summary>
        /// Check if the given type can be serialized by Unity.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>Whenever the field can be serialized by Unity of not.</returns>
        public static bool CanBeSerializedByUnity(this Type type)
        {
            if (type.IsArray)
                type = type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                type = type.GetGenericArguments()[0];

            if (type.IsSubclassOf(typeof(UnityObject)))
                return true;

            if (type.IsAbstract || type.IsGenericType || type.IsInterface)
                return false;

            if (type.IsPrimitive || type.IsEnum || type.IsValueType || unityDefaultNonPrimitiveSerializables.Contains(type))
                return true;

            if (type.IsDefined(typeof(SerializableAttribute)))
                return true;

            return false;
        }

        /// <summary>
        /// Check if the given field can be serialized by Unity.
        /// </summary>
        /// <param name="fieldInfo">Field to check.</param>
        /// <returns>Whenever the field can be serialized by Unity of not.</returns>
        public static bool CanBeSerializedByUnity(this FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPublic || fieldInfo.IsDefined(typeof(SerializeField)))
            {
                if (fieldInfo.IsStatic || fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
                    return false;

                return fieldInfo.FieldType.CanBeSerializedByUnity();
            }
            return false;
        }

        /// <summary>
        /// Check if the given type can be serialized by Unity.
        /// </summary>
        /// <param name="typeInfo">Typeinfo of type to check.</param>
        /// <returns>Whenever the field can be serialized by Unity of not.</returns>
        public static bool CanBeSerializedByUnity(this TypeInfo typeInfo) => typeInfo.GetType().CanBeSerializedByUnity();

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
        public static IEnumerable<string> FieldsPropertiesAndMethodsWithReturnTypeOf(this Type @class, Type @return) => @class
                .GetFields(bindingFlags)
                .Where(field => field.FieldType.IsCastableTo(@return) && field.CanBeSerializedByUnity())
                .Cast<MemberInfo>()
                .Concat(
                    @class
                        .GetProperties(bindingFlags)
                        .Where(property => property.PropertyType.IsCastableTo(@return) && property.CanRead)
                        .Cast<MemberInfo>()
                )
                .Concat(
                    @class
                        .GetMethods(bindingFlags)
                        .Where(method => method.ReturnType.IsCastableTo(@return) && method.HasNoMandatoryParameters())
                        .Cast<MemberInfo>()
                )
                .Select(member => member.Name);

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