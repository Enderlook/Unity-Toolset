using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class ShowfIfTesting
    {
        private static readonly Type[] type2 = new Type[2];
        private static readonly Dictionary<Type, List<(FieldInfo field, ShowIfAttribute attribute)>> typesAndAttributes = new Dictionary<Type, List<(FieldInfo field, ShowIfAttribute attribute)>>();

        [ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization.SerializableByUnity, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (fieldInfo.CheckIfShouldBeIgnored(typeof(ShowIfAttribute)))
                return;
            if (fieldInfo.GetCustomAttribute<ShowIfAttribute>() is ShowIfAttribute attribute)
            {
                Type type = fieldInfo.DeclaringType;
                if (!typesAndAttributes.TryGetValue(type, out List<(FieldInfo field, ShowIfAttribute attribute)> list))
                    typesAndAttributes.Add(type, (list = new List<(FieldInfo field, ShowIfAttribute attribute)>()));
                list.Add((fieldInfo, attribute));
            }
        }

        [ExecuteWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            // TODO: This could be optimized by deduplicating similar checkings.

            foreach (KeyValuePair<Type, List<(FieldInfo field, ShowIfAttribute attribute)>> kvp in typesAndAttributes)
            {
                foreach ((FieldInfo field, ShowIfAttribute attribute) in kvp.Value)
                {
                    string firstProperty = attribute.firstProperty;
                    if (firstProperty is null)
                    {
                        Debug.LogError($"Value of property {nameof(attribute.firstProperty)} is null or empty in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name}.");
                        continue;
                    }

                    Type firstType = GetType(kvp.Key, firstProperty);
                    if (firstType is null)
                    {
                        Debug.LogError($"No field, property (with Get method), or method with no mandatory parameters of name '{attribute.firstProperty}' in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name} was found.");
                        continue;
                    }

                    switch (attribute.mode)
                    {
                        case ShowIfAttribute.Mode.Single:
                        {
                            // TODO: Check if the comparison is legal.
                            break;
                        }
                        case ShowIfAttribute.Mode.WithObject:
                        {
                            // TODO: Check if the comparison is legal.
                            break;
                        }
                        case ShowIfAttribute.Mode.WithProperty:
                        {
                            string secondProperty = attribute.secondProperty;
                            if (secondProperty is null)
                            {
                                Debug.LogError($"Value of property {nameof(attribute.secondProperty)} is null or empty in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name}.");
                                break;
                            }

                            Type secondType = GetType(kvp.Key, secondProperty);
                            if (secondType is null)
                            {
                                Debug.LogError($"No field, property (with Get method), or method with no mandatory parameters of name '{attribute.secondProperty}' in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name} was found.");
                                break;
                            }

                            // TODO: Check if the comparison is legal.
                            break;
                        }
                        default:
                        {
                            Debug.Assert(false, "Impossible state.");
                            break;
                        }
                    }
                }
            }
        }

        private static Type GetType(Type originalType, string name)
        {
            Type type = originalType;
            start:
            foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (memberInfo.Name != name)
                    continue;

                if (memberInfo is FieldInfo fieldInfo)
                    return fieldInfo.FieldType;

                if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                    return propertyInfo.PropertyType;

                if (memberInfo is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void) && methodInfo.HasNoMandatoryParameters())
                    return methodInfo.ReturnType;
            }

            type = type.BaseType;
            if (type == null)
                return default;
            goto start;
        }
    }
}
