using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Checking;
using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Testing
{
    internal static class ConditionalfIfTesting
    {
        private static readonly Dictionary<Type, List<(FieldInfo field, IConditionalAttribute attribute)>> typesAndAttributes = new Dictionary<Type, List<(FieldInfo field, IConditionalAttribute attribute)>>();

        [ExecuteOnEachFieldOfEachTypeWhenScriptsReloads(FieldSerialization.SerializableByUnity, 0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void GetFields(FieldInfo fieldInfo)
        {
            if (!fieldInfo.CheckIfShouldBeIgnored(typeof(ShowIfAttribute))
                && fieldInfo.GetCustomAttribute<ShowIfAttribute>() is ShowIfAttribute attribute1)
                Add(fieldInfo, attribute1);

            if (!fieldInfo.CheckIfShouldBeIgnored(typeof(EnableIfAttribute))
                && fieldInfo.GetCustomAttribute<EnableIfAttribute>() is EnableIfAttribute attribute2)
                Add(fieldInfo, attribute2);

            void Add(FieldInfo fieldInfo, IConditionalAttribute attribute)
            {
                Type type = fieldInfo.DeclaringType;
                if (!typesAndAttributes.TryGetValue(type, out List<(FieldInfo field, IConditionalAttribute attribute)> list))
                    typesAndAttributes.Add(type, list = new List<(FieldInfo field, IConditionalAttribute attribute)>());
                list.Add((fieldInfo, attribute));
            }
        }

        [ExecuteWhenScriptsReloads(1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void CheckFields()
        {
            // TODO: This could be optimized by deduplicating similar checkings.

            foreach (KeyValuePair<Type, List<(FieldInfo field, IConditionalAttribute attribute)>> kvp in typesAndAttributes)
            {
                foreach ((FieldInfo field, IConditionalAttribute attribute) in kvp.Value)
                {
                    string firstProperty = attribute.FirstProperty;
                    if (firstProperty is null)
                    {
                        Debug.LogError($"Value of property {nameof(attribute.FirstProperty)} is null or empty in attribute {attribute.GetType()} in field {field.Name} of type {field.ReflectedType.Name}.");
                        continue;
                    }

                    Type firstType = GetType(kvp.Key, firstProperty);
                    if (firstType is null)
                    {
                        Debug.LogError($"No field, property (with Get method), or method with no mandatory parameters of name '{attribute.FirstProperty}' in attribute {attribute.GetType()} in field {field.Name} of type {field.ReflectedType.Name} was found.");
                        continue;
                    }

                    switch (attribute.Mode)
                    {
                        case IConditionalAttribute.ConditionalMode.Single:
                        {
                            // TODO: Check if the comparison is legal.
                            break;
                        }
                        case IConditionalAttribute.ConditionalMode.WithObject:
                        {
                            // TODO: Check if the comparison is legal.
                            break;
                        }
                        case IConditionalAttribute.ConditionalMode.WithProperty:
                        {
                            string secondProperty = attribute.SecondProperty;
                            if (secondProperty is null)
                            {
                                Debug.LogError($"Value of property {nameof(attribute.SecondProperty)} is null or empty in attribute {attribute.GetType()} in field {field.Name} of type {field.ReflectedType.Name}.");
                                break;
                            }

                            Type secondType = GetType(kvp.Key, secondProperty);
                            if (secondType is null)
                            {
                                Debug.LogError($"No field, property (with Get method), or method with no mandatory parameters of name '{attribute.SecondProperty}' in attribute {attribute.GetType()} in field {field.Name} of type {field.ReflectedType.Name} was found.");
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
