using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking
{
    internal static class AttributeUsageMethodTesting
    {
        private static readonly Dictionary<Type, AttributeUsageMethodAttribute> checkers = new Dictionary<Type, AttributeUsageMethodAttribute>();

        private static StringBuilder stringBuilder;

        [ExecuteWhenCheck(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void Reset() => checkers.Clear();

        [ExecuteOnEachTypeWhenCheck(TypeFlags.IsNonEnum, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributesAndTypes(Type type)
        {
            if (type.IsSubclassOf(typeof(Attribute))
                && type.GetCustomAttribute<AttributeUsageMethodAttribute>(true) is AttributeUsageMethodAttribute attribute)
            {
                if (attribute.parameterIndex == -1)
                    Debug.LogError($"Invalid configuration in attribute '{nameof(AttributeUsageMethodAttribute)}'. When '{nameof(attribute.parameterIndex)}' can't be negative.");
                if (attribute.parameterModifier != ParameterModifier.None && attribute.parameterModifier != ParameterModifier.In && attribute.parameterModifier != ParameterModifier.Ref && attribute.parameterModifier != ParameterModifier.Out)
                    Debug.LogError($"Invalid configuration in attribute '{nameof(AttributeUsageMethodAttribute)}'. '{nameof(attribute.parameterModifier)}' must have a valid value of enum {nameof(ParameterModifier)}. Found {attribute.parameterModifier}.");
                checkers.Add(type, attribute);
            }
        }

        [ExecuteOnEachMethodOfEachTypeWhenCheck(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckMethods(MethodInfo methodInfo)
        {
            foreach (Attribute attribute_ in methodInfo.GetCustomAttributes())
            {
                Type attributeType = attribute_.GetType();

                if (checkers.TryGetValue(attributeType, out AttributeUsageMethodAttribute attribute)
                    && !methodInfo.CheckIfShouldBeIgnored(attributeType))
                {
                    StringBuilder GetBuilder()
                    {
                        // This values were got by concatenating the sum of the largest possible paths of appended constants in the outer method.

                        int minCapacity = 86 + attributeType.Name.Length + methodInfo.Name.Length + methodInfo.DeclaringType.ToString().Length;
                        if (!(attribute.parameterIndex is null))
                            minCapacity += 62 + (attribute.types is null ? 0 : AttributeUsageHelper.GetMaximumRequiredCapacity(attribute.types)) + methodInfo.ReturnType.ToString().Length;
                        else
                            minCapacity += 85;

                        StringBuilder builder = Interlocked.Exchange(ref stringBuilder, null);
                        if (builder is null)
                            builder = new StringBuilder(minCapacity);
                        else
                            builder.EnsureCapacity(minCapacity);

                        return builder
                            .Append("According to attribute '" + nameof(AttributeUsageMethodAttribute) + "', the attribute '")
                            .Append(attributeType.Name)
                            .Append("' on method ")
                            .Append(methodInfo.Name)
                            .Append(" in")
                            .Append(methodInfo.DeclaringType);
                    }

                    void Log(StringBuilder builder)
                    {
                        string result = builder.ToString();
                        builder.Clear();
                        stringBuilder = builder;
                        Debug.LogError(result);
                    }

                    if (!(attribute.parameterIndex is int parameterIndex))
                    {
                        // Check return type.

                        if (!AttributeUsageHelper.CheckContains(
                                attribute.types,
                                attribute.typeRelationship,
                                attribute.isBlackList,
                                false,
                                methodInfo.ReturnType
                            ))
                        {
                            Log(AttributeUsageHelper
                                .AppendSupportedTypes(
                                    GetBuilder()
                                        .Append(" the return type is only valid if it "),
                                    attribute.types,
                                    attribute.typeRelationship,
                                    attribute.isBlackList,
                                    false)
                                .Append(". Return type is '")
                                .Append(methodInfo.ReturnType)
                                .Append("' type."));
                        }
                    }
                    else
                    {
                        // Check a parameter.
                        ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                        if (attribute.parameterIndex >= parameterInfos.Length)
                        {
                            // Parameter doesn't exist, check if was on purpose.
                            if (attribute.types != null)
                            {
                                StringBuilder builder = GetBuilder()
                                    .Append(" the parameter at index ")
                                    .Append(attribute.parameterIndex)
                                    .Append(" was expected. But method have ")
                                    .Append(parameterInfos.Length)
                                    .Append(" parameter");
                                if (parameterInfos.Length != 1)
                                    builder.Append("s.");
                                else
                                    builder.Append('.');
                                Log(builder);
                            }
                            continue;
                        }

                        ParameterInfo parameterInfo = parameterInfos[parameterIndex];

                        // Check if should exist.
                        if (attribute.types is null)
                        {
                            Log(GetBuilder()
                                .Append(" has more parameters than allowed. The parameter at index ")
                                .Append(attribute.parameterIndex - 1)
                                .Append(" named ")
                                .Append(parameterInfo.Name)
                                .Append(" was not expected."));
                            continue;
                        }

                        // Get parameter keyword.
                        ParameterModifier mode = ParameterModifier.None;
                        if (parameterInfo.ParameterType.IsByRef)
                        {
                            // Both `out` and `ref` has IsByRef = true, so we must check IsOut in order to split them.
                            // https://stackoverflow.com/a/38110036 from https://stackoverflow.com/questions/1551761/ref-parameters-and-reflection.
                            mode = parameterInfo.IsOut ? ParameterModifier.Out : ParameterModifier.Ref;
                        }
                        else if (parameterInfo.IsIn)
                            mode = ParameterModifier.In;

                        // Check parameter keyword.
                        if (attribute.parameterModifier != mode)
                        {
                            string expected = null;
                            if (attribute.parameterModifier == ParameterModifier.Out)
                                expected = "'out'";
                            else if (attribute.parameterModifier == ParameterModifier.Ref)
                                expected = "'ref'";
                            else if (attribute.parameterModifier == ParameterModifier.In)
                                expected = "'in'";
                            else if (attribute.parameterModifier == ParameterModifier.None)
                                expected = "no";
                            else
                                goto invalidParameterType;

                            string found = null;
                            if (mode == ParameterModifier.Out)
                                found = "'out'";
                            else if (mode == ParameterModifier.Ref)
                                found = "'ref'";
                            else if (mode == ParameterModifier.In)
                                found = "'in'";
                            else if (mode == ParameterModifier.None)
                                found = "no";
                            else
                                System.Diagnostics.Debug.Fail("Impossible State.");

                            if (!(found is null))
                            {
                                Log(GetBuilder()
                                    .Append(" the parameter at index ")
                                    .Append(attribute.parameterIndex)
                                    .Append(" named ")
                                    .Append(parameterInfo.Name)
                                    .Append(" has ")
                                    .Append(found)
                                    .Append("keyword. Expected ")
                                    .Append(expected)
                                    .Append("keyword"));
                                continue;
                            }
                        invalidParameterType:;
                        }

                        if (AttributeUsageHelper.CheckContains(
                            attribute.types,
                            attribute.typeRelationship,
                            attribute.isBlackList,
                            false,
                            parameterInfo.ParameterType
                           ))
                        {
                            Log(AttributeUsageHelper.AppendSupportedTypes(
                                GetBuilder()
                                    .Append(" the parameter at index ")
                                    .Append(attribute.parameterIndex)
                                    .Append(" named ")
                                    .Append(parameterInfo.Name)
                                    .Append(" expected to be of a type that "),
                                attribute.types,
                                attribute.typeRelationship,
                                attribute.isBlackList,
                                false
                            ));
                        }
                    }
                }
            }
        }
    }
}