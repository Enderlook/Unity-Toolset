using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// Helper methods for reflection related works.
    /// </summary>
    public static class ReflectionHelper
    {
        private static readonly Type[] UNITY_BUILT_IN_SUPPORTED_TYPES = new Type[]
        {
            typeof(Vector2), typeof(Vector3), typeof(Vector4),
            typeof(Rect), typeof(Quaternion), typeof(Matrix4x4),
            typeof(Color), typeof(Color32), typeof(LayerMask),
            typeof(AnimationCurve), typeof(Gradient), typeof(RectOffset), typeof(GUIStyle),
        };

        private static readonly Type[] VALID_ENUM_TYPES = new Type[]
        {
            // Can't be larger than 32-bits.
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(float),
            typeof(char)
        };

        private static readonly Dictionary<Type, Array> zeroArray = new Dictionary<Type, Array>();
        private static ReadWriteLock zeroArrayLock = new ReadWriteLock();

        /// <summary>
        /// Check if the given type can be serialized by Unity.<br/>
        /// Note that in some cases this may give misleading results: if you have a <see cref="FieldInfo"/>, it's better to call <see cref="CanBeSerializedByUnity(FieldInfo)"/> instead as some cases are affected by the field itself and the type which declares that field.
        /// </summary>
        /// <param name="type">Type to check.</param>
#if UNITY_2019_3_OR_NEWER
        /// <param name="isSerializedByReference">If <see langword="true"/>, consider that type is being serialized with <see cref="SerializeReference"/>.<br/>
#endif
        /// Otherwise it's considered as using <see cref="SerializeField"/>.</param>
        /// <returns>Whenever the field can be serialized by Unity or not.</returns>
        public static bool CanBeSerializedByUnity(this Type type
#if UNITY_2019_3_OR_NEWER
            , bool isSerializedByReference = false
#endif
            )
        {
            // https://docs.unity3d.com/Manual/script-Serialization.html

            if (type.IsArray)
            {
                if (type.GetArrayRank() > 1)
                    return false;
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                    type = type.GetGenericArguments()[0];
#if !UNITY_2020_1_OR_NEWER
                else
                    return false;
#endif
            }
            else
                goto skip;

#if UNITY_2020_1_OR_NEWER
            if (type.IsArray)
#else
            if (type.IsArray || type.IsGenericType)
#endif
                return false;

#if UNITY_2020_1_OR_NEWER
            if (type.IsGenericParameter)
                return true;
#endif

        skip:
#if UNITY_2019_3_OR_NEWER
            if (!isSerializedByReference)
            {
#endif
                if (type.IsPrimitive || type.IsSubclassOf(typeof(UnityObject))
                    || (type.IsValueType && UNITY_BUILT_IN_SUPPORTED_TYPES.Contains(type)))
                    return true;

                if (type.IsAbstract || type.IsInterface)
                    return false;

                if (type.IsEnum)
                    return VALID_ENUM_TYPES.Contains(Enum.GetUnderlyingType(type));

                if (type.IsDefined(typeof(SerializableAttribute)))
                    return true;

                return false;
#if UNITY_2019_3_OR_NEWER
            }
            else
            {
                if (type.IsPrimitive || type.IsValueType || type.IsSubclassOf(typeof(UnityObject)))
                    return false;

                return true;
            }
#endif
        }

        /// <summary>
        /// Check if the given field can be serialized by Unity.
        /// </summary>
        /// <param name="fieldInfo">Field to check.</param>
        /// <returns>Whenever the field can be serialized by Unity or not.</returns>
        public static bool CanBeSerializedByUnity(this FieldInfo fieldInfo)
        {
            // https://docs.unity3d.com/Manual/script-Serialization.html

#if UNITY_2019_3_OR_NEWER
            bool serializeReference = fieldInfo.IsDefined(typeof(SerializeReference));
            if ((!serializeReference && !fieldInfo.IsPublic && !fieldInfo.IsDefined(typeof(SerializeField)))
                || fieldInfo.IsStatic || fieldInfo.IsInitOnly || fieldInfo.IsLiteral || fieldInfo.IsDefined(typeof(NonSerializedAttribute))
                // SerializeReference don't support Animation fields on specific cases:
                // https://docs.unity3d.com/ScriptReference/SerializeReference.html
                || (serializeReference && fieldInfo.FieldType == typeof(Animation)
                    && (fieldInfo.DeclaringType.IsSubclassOf(typeof(ScriptableObject))
                        || fieldInfo.DeclaringType.IsSubclassOf(typeof(UnityEditor.AssetImporters.ScriptedImporter)))))
                return false;
            return fieldInfo.FieldType.CanBeSerializedByUnity(serializeReference);
#else
            if ((!fieldInfo.IsPublic && !fieldInfo.IsDefined(typeof(SerializeField)))
                || fieldInfo.IsStatic || fieldInfo.IsInitOnly || fieldInfo.IsLiteral || fieldInfo.IsDefined(typeof(NonSerializedAttribute)))
                return false;
            return fieldInfo.FieldType.CanBeSerializedByUnity();
#endif
        }

        /// <summary>
        /// Get an empty array of element type <paramref name="elementType"/>.
        /// </summary>
        /// <param name="elementType">Element type of array.</param>
        /// <returns>Zero-sized array.</returns>
        public static Array EmptyArray(Type elementType)
        {
            if (elementType is null) Helper.ThrowArgumentNullException_ElementType();

            bool value;
            Array array;
            zeroArrayLock.ReadBegin();
            {
                value = zeroArray.TryGetValue(elementType, out array);
            }
            zeroArrayLock.ReadEnd();

            if (value)
            {
                zeroArrayLock.WriteBegin();
                {
                    if (!zeroArray.TryGetValue(elementType, out array))
                        zeroArray.Add(elementType, array = Array.CreateInstance(elementType, 0));
                }
                zeroArrayLock.WriteEnd();
            }
            return array;
        }

        /// <summary>
        /// Get the field name recursively through the inheritance hierarchy of <paramref name="source"/> regardless of field accessibility.<br/>
        /// Returns the first match.
        /// </summary>
        /// <param name="source">Initial <see cref="Type"/> used to get the field.</param>
        /// <param name="name">Name of the field to get.</param>
        /// <param name="bindingFlags">Binding flags used to look for the field.</param>
        /// <returns>The first field that matches the given name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> are <see langword="null"/>.</exception>
        public static FieldInfo GetFieldExhaustive(this Type source, string name, ExhaustiveBindingFlags bindingFlags)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (name is null) Helper.ThrowArgumentNullException_Name();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
            if ((bindingFlags & ExhaustiveBindingFlags.Instance) != 0)
                flags |= BindingFlags.Instance;
            if ((bindingFlags & ExhaustiveBindingFlags.Static) != 0)
                flags |= BindingFlags.Static;

            FieldInfo fieldInfo = source.GetField(name, flags | BindingFlags.FlattenHierarchy);
            while (fieldInfo is null)
            {
                source = source.BaseType;
                if (source is null)
                    return fieldInfo;
                fieldInfo = source.GetField(name, flags);
            }
            return fieldInfo;
        }

        /// <summary>
        /// Get the field name recursively through the inheritance hierarchy of <paramref name="source"/> regardless of field accessibility.<br/>
        /// Returns the first match.
        /// </summary>
        /// <param name="source">Initial <see cref="Type"/> used to get the field.</param>
        /// <param name="name">Name of the field to get.</param>
        /// <param name="includeStatics">Whenever it should include static fields.</param>
        /// <returns>The first field that matches the given name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="name"/> are <see langword="null"/>.</exception>
        public static FieldInfo[] GetFieldsExhaustive(this Type source, ExhaustiveBindingFlags bindingFlags)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
            if ((bindingFlags & ExhaustiveBindingFlags.Instance) != 0)
                flags |= BindingFlags.Instance;
            if ((bindingFlags & ExhaustiveBindingFlags.Static) != 0)
                flags |= BindingFlags.Static;

            FieldInfo[] firsts = source.GetFields(flags | BindingFlags.FlattenHierarchy);
            object[] tmp = null;
            int count = 0;
            source = source.BaseType;
            while (!(source is null))
            {
                FieldInfo[] current = source.GetFields(flags);
                int currentLength = current.Length;
                if (currentLength > 0)
                {
                    if (tmp is null)
                    {
                        int firstLength = firsts.Length;
                        ArrayPool<object> shared = ArrayPool<object>.Shared;
                        tmp = shared.Rent(firstLength + currentLength);
                        Array.Copy(firsts, tmp, firstLength);
                        firsts = null;
                        Array.Copy(current, 0, tmp, firstLength, tmp.Length);
                        count = firstLength + tmp.Length;
                    }
                    else
                    {
                        int newCount = count + currentLength;
                        if (unchecked((uint)newCount >= (uint)tmp.Length))
                        {
                            object[] newTmp = ArrayPool<object>.Shared.Rent(count + currentLength);
                            Array.Copy(tmp, newTmp, count);
                            Array.Clear(tmp, 0, count);
                            ArrayPool<object>.Shared.Return(tmp);
                            tmp = newTmp;
                        }
                        Array.Copy(current, 0, tmp, count, currentLength);
                        count += currentLength;
                    }
                }
                source = source.BaseType;
            }

            if (!(firsts is null))
                return firsts;

            FieldInfo[] result = new FieldInfo[count];
            tmp.AsSpan(0, count).CopyTo(result);
            Array.Clear(tmp, 0, count);
            ArrayPool<object>.Shared.Return(tmp);
            return result;
        }

        /// <summary>
        /// Returns the first member of <paramref name="source"/> which:
        /// <list type="bullet">
        ///     <item><description>If <see cref="FieldInfo"/>, its <see cref="FieldInfo.FieldType"/> must be of type <paramref name="memberType"/>.</description></item>
        ///     <item><description>If <see cref="PropertyInfo"/>, its <see cref="PropertyInfo.PropertyType"/> must be of type <paramref name="memberType"/> and it must have a getter.</description></item>
        ///     <item><description>If <see cref="MethodInfo"/>, its <see cref="MethodInfo.ReturnType"/> must be of type <paramref name="memberType"/> and apart from the <see langword="this"/> parameter (if instance method), all other parameters (if any) must be optional, has default value or has <see cref="ParamArrayAttribute"/> attribute.</description></item>
        /// </list>
        /// This method looks recursively through the inheritance hierarchy of <paramref name="source"/> regardless of members accessibility.
        /// </summary>
        /// <param name="source">Type to look for <see cref="MemberInfo"/> and results.</param>
        /// <param name="name">Name of the <see cref="MemberInfo"/> looked for.</param>
        /// <param name="resultType">Expected type of the field, property, or result type of the method.</param>
        /// <returns>First <see cref="MemberInfo"/> of <paramref name="source"/> in match the criteria. <see langword="null"/> if found no match.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/>, <paramref name="name"/> or <paramref name="resultType"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty.</exception>
        public static MemberInfo GetFirstMemberInfoThatMatchesResultTypeExhaustive(this Type source, string name, Type resultType, ExhaustiveBindingFlags bindingFlags)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (name is null) Helper.ThrowArgumentNullException_Name();
            if (resultType is null) Helper.ThrowArgumentNullException_ResultType();
            if (name.Length == 0) Helper.ThrowArgumentException_NameCannotBeEmpty();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
            if ((bindingFlags & ExhaustiveBindingFlags.Instance) != 0)
                flags |= BindingFlags.Instance;
            if ((bindingFlags & ExhaustiveBindingFlags.Static) != 0)
                flags |= BindingFlags.Static;

            do
            {
                foreach (MemberInfo memberInfo in source.GetMembers(flags))
                {
                    if (memberInfo.Name != name)
                        continue;
                    switch (memberInfo.MemberType)
                    {
                        case MemberTypes.Field:
                            FieldInfo fieldInfo = (FieldInfo)memberInfo;
                            if (fieldInfo.FieldType == resultType)
                                return fieldInfo;
                            break;
                        case MemberTypes.Property:
                            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                            if (propertyInfo.CanRead && propertyInfo.GetMethod.ReturnType == resultType)
                                return propertyInfo;
                            break;
                        case MemberTypes.Method:
                            MethodInfo methodInfo = (MethodInfo)memberInfo;
                            if (methodInfo.ReturnType == resultType && methodInfo.HasNoMandatoryParameters())
                                return methodInfo;
                            break;
                    }
                }
            } while (!((source = source.BaseType) is null));

            return null;
        }

        /// <summary>
        /// Returns the result value of the first member of <paramref name="source"/> which:
        /// <list type="bullet">
        ///     <item><description>If <see cref="FieldInfo"/>, its <see cref="FieldInfo.FieldType"/> must be of type <paramref name="memberType"/>.</description></item>
        ///     <item><description>If <see cref="PropertyInfo"/>, its <see cref="PropertyInfo.PropertyType"/> must be of type <paramref name="memberType"/> and it must have a getter.</description></item>
        ///     <item><description>If <see cref="MethodInfo"/>, its <see cref="MethodInfo.ReturnType"/> must be of type <paramref name="memberType"/> and apart from the <see langword="this"/> parameter (if instance method), all other parameters (if any) must be optional, has default value or has <see cref="ParamArrayAttribute"/> attribute.</description></item>
        /// </list>
        /// This method looks recursively through the inheritance hierarchy of <paramref name="source"/> regardless of members accessibility.
        /// </summary>
        /// <param name="source">Type to look for <see cref="MemberInfo"/> and results.</param>
        /// <param name="name">Name of the <see cref="MemberInfo"/> looked for.</param>
        /// <param name="resultType">Expected type of the field, property, or result type of the method.</param>
        /// <returns>Result value of first <see cref="MemberInfo"/> of <paramref name="source"/> in match the criteria.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/>, <paramref name="name"/> or <paramref name="resultType"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty.</exception>
        public static T GetValueFromFirstMemberInfoThatMatchesResultTypeExhaustive<T>(this object source, string name, ExhaustiveBindingFlags bindingFlags)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (name is null) Helper.ThrowArgumentNullException_Name();
            if (name.Length == 0) Helper.ThrowArgumentException_NameCannotBeEmpty();

            MemberInfo memberInfo = GetFirstMemberInfoThatMatchesResultTypeExhaustive(source.GetType(), name, typeof(T), bindingFlags);
            if (memberInfo is null)
                Throw();

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)memberInfo;
                    return (T)fieldInfo.GetValue(fieldInfo.IsStatic ? null : source);
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                    return (T)propertyInfo.GetValue(propertyInfo.GetMethod.IsStatic ? null : source);
                case MemberTypes.Method:
                    MethodInfo methodInfo = (MethodInfo)memberInfo;
                    return (T)methodInfo.InvokeWithDefaultArguments(methodInfo.IsStatic ? null : source);
                default:
                    Debug.Assert(false, "Impossible state.");
                    return default;
            }

            void Throw() => throw new MatchingMemberNotFoundException(name, source.GetType(), typeof(T), bindingFlags);
        }

        /// <summary>
        /// Determines if the <paramref name="source"/> only has optional or params parameters.
        /// </summary>
        /// <param name="source"><see cref="MethodInfo"/> to check.</param>
        /// <returns>Whenever the <paramref name="source"/> only has optional or params parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool HasNoMandatoryParameters(this MethodInfo source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            foreach (ParameterInfo parameter in source.GetParameters())
                if (!parameter.IsOptional && !parameter.IsDefined(typeof(ParamArrayAttribute)))
                    return false;

            return true;
        }

        /// <summary>
        /// Determines if the <paramref name="source"/> only has optional or params parameters.
        /// </summary>
        /// <param name="source"><see cref="MethodInfo"/> to check.</param>
        /// <param name="parameters">If returns <see langword="true"/>, this holds the value that can be passed as arguments to invoke through reflection the method that is owner of this parameter.</param>
        /// <returns>Whenever the <paramref name="source"/> only has optional or params parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool HasNoMandatoryParameters(this MethodInfo source, out object[] parameters)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            ParameterInfo[] parameters_ = source.GetParameters();
            parameters = null;
            int num = 0;
            ParameterInfo[] array = parameters_;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].IsOptionalOrParam(out object parameter))
                {
                    if (parameters is null)
                        parameters = new object[parameters_.Length];
                    parameters[num++] = parameter;
                    continue;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Invokes <paramref name="source"/> using <paramref name="obj"/> as its class instance and without any parameter (expect optionals).
        /// </summary>
        /// <param name="methodInfo">Method to invoke.</param>
        /// <param name="obj">Instance of the class to invoke.</param>
        /// <returns>Result of the method invoked.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="HasMandatoryParametersException">Throw when method has a parameter (except the <see langword="this"/> parameter of instance methods) that is not optional, has not default value nor has the attribute <see cref="ParamArrayAttribute"/>.</exception>
        public static object InvokeWithDefaultArguments(this MethodInfo source, object obj)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            if (!source.HasNoMandatoryParameters(out object[] parameters))
                Throw();

            return source.Invoke(obj, parameters);

            void Throw() => throw new HasMandatoryParametersException(source);
        }

        /// <summary>
        /// Determines type is an array or instance of <see cref="List{T}"/>.
        /// </summary>
        /// <param name="source">Type to check.</param>
        /// <returns>Whenever type is an array or instance of <see cref="List{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool IsArrayOrList(this Type source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            return source.IsArray || (source.IsGenericType && source.GetGenericTypeDefinition() == typeof(List<>));
        }

        /// <summary>
        /// Determines type is an array or instance of <see cref="List{T}"/>.
        /// </summary>
        /// <param name="source">Type to check.</param>
        /// <param name="elementType">Element type of the collection if returns <see langword="true"/>.</param>
        /// <returns>Whenever type is an array or instance of <see cref="List{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool IsArrayOrList(this Type source, out Type elementType)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            if (source.IsArray)
                elementType = source.GetElementType();
            else if (source.IsGenericType && source.GetGenericTypeDefinition() == typeof(List<>))
                elementType = source.GetGenericArguments()[0];
            else
            {
                elementType = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if <paramref name="source"/> is optional, has default value or is a param parameter.
        /// </summary>
        /// <param name="source"></param>
        /// <returns><see langword="true"/> if <paramref name="source"/> is optional, has default value or is a param parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool IsOptionalOrParam(this ParameterInfo source)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            return source.IsOptional || source.HasDefaultValue || source.IsDefined(typeof(ParamArrayAttribute));
        }

        /// <summary>
        /// Determines if <paramref name="source"/> is optional, has default value or is a param parameter.
        /// </summary>
        /// <param name="source"></param>
        /// <returns><see langword="true"/> if <paramref name="source"/> is optional, has default value or is a param parameter.</returns>
        /// <param name="parameter">If returns <see langword="true"/>, this holds the value that can be passed as argument to invoke through reflection the method that is owner of this parameter.</param>
        /// <param name="parameter">If returns <see langword="true"/>, this holds the value that can be passed as argument to invoke through reflection the method that is owner of this parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
        public static bool IsOptionalOrParam(this ParameterInfo source, out object parameter)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();

            if (source.IsOptional || source.HasDefaultValue)
            {
                parameter = Type.Missing;
                return true;
            }

            if (source.IsDefined(typeof(ParamArrayAttribute)))
            {
                parameter = EmptyArray(source.ParameterType.GetElementType());
                return true;
            }

            parameter = null;
            return false;
        }
    }
}