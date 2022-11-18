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
                if (attribute.parameterNumber == 0 && attribute.parameterType != ParameterMode.Common && attribute.parameterType != ParameterMode.VoidOrNone)
                    Debug.LogError($"Invalid configuration in attribute '{nameof(AttributeUsageMethodAttribute)}'. When '{nameof(attribute.parameterNumber)}' is 0, '{nameof(attribute.parameterType)}' must be {nameof(ParameterMode.Common)} or {nameof(ParameterMode.VoidOrNone)}. Found {attribute.parameterType}.");
                if (attribute.parameterType != ParameterMode.Common && attribute.parameterType != ParameterMode.In && attribute.parameterType != ParameterMode.Ref && attribute.parameterType != ParameterMode.Out)
                    Debug.LogError($"Invalid configuration in attribute '{nameof(AttributeUsageMethodAttribute)}'. '{nameof(attribute.parameterType)}' must have a valid value of enum {nameof(ParameterMode)}. Found {attribute.parameterType}.");
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
                        // This values were got by concatenating the sum of the largest possible paths of appended constants in outer method.

                        int minCapacity = 147;
                        int minCapacity1 = attributeType.Name.Length + methodInfo.Name.Length + methodInfo.DeclaringType.ToString().Length;
                        int minCapacity2 = 1 + methodInfo.ReturnType.ToString().Length;
                        int minCapacity3 = 6 + 10 /* Maximum digits that can take array.Length to display it */ + Math.Max((int)Math.Ceiling(Math.Log10(attribute.parameterNumber - 1)), 1);
                        minCapacity += Math.Max(Math.Max(minCapacity1, minCapacity2), minCapacity3);

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
                        Debug.LogError(builder);
                    }

                    if (attribute.parameterNumber == 0)
                    {
                        // Check return type.
                        Type returnType = methodInfo.ReturnType;

                        if (attribute.parameterType == ParameterMode.VoidOrNone)
                        {
                            if (returnType == typeof(void))
                            {
                                Log(GetBuilder()
                                    .Append(" the return type must be ")
                                    .Append(typeof(void))
                                    .Append(". Return type is '")
                                    .Append(returnType)
                                    .Append("' type."));
                            }
                        }
                        else if (attribute.parameterType == ParameterMode.Common)
                        {
                            if (!AttributeUsageHelper.CheckContains(
                                attribute.basicTypes,
                                attribute.checkingFlags,
                                attribute.isBlackList,
                                false,
                                returnType
                            ))
                            {
                                Log(AttributeUsageHelper
                                    .AppendSupportedTypes(
                                        GetBuilder()
                                            .Append(" the return type is only valid if it "),
                                        attribute.basicTypes,
                                        attribute.checkingFlags,
                                        attribute.isBlackList,
                                        false)
                                    .Append(". Return type is '")
                                    .Append(returnType)
                                    .Append("' type."));
                            }
                        }
                    }
                    else
                    {
                        // Check a parameter.
                        ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                        if (attribute.parameterNumber - 1 >= parameterInfos.Length)
                        {
                            // Parameter doesn't exist, check if was on purpose.
                            if (attribute.parameterType != ParameterMode.VoidOrNone)
                            {
                                StringBuilder builder = GetBuilder()
                                    .Append(" the parameter at index ")
                                    .Append(attribute.parameterNumber - 1)
                                    .Append(" was expected. But method have ")
                                    .Append(parameterInfos.Length)
                                    .Append(" parameter");
                                if (parameterInfos.Length != 1)
                                    builder.Append("s.");
                                else
                                    builder.Append('.');
                                Log(builder);
                                continue;
                            }
                        }

                        ParameterInfo parameterInfo = parameterInfos[attribute.parameterNumber - 1];

                        // Check if should exist.
                        if (attribute.parameterType == ParameterMode.VoidOrNone)
                        {
                            Log(GetBuilder()
                                .Append(" has more parameters than allowed. The parameter at index ")
                                .Append(attribute.parameterNumber - 1)
                                .Append(" named ")
                                .Append(parameterInfo.Name)
                                .Append(" was not expected."));
                            continue;
                        }

                        // Get parameter keyword.
                        ParameterMode mode = ParameterMode.Common;
                        if (parameterInfo.ParameterType.IsByRef)
                        {
                            // Both `out` and `ref` has IsByRef = true, so we must check IsOut in order to split them.
                            // https://stackoverflow.com/a/38110036 from https://stackoverflow.com/questions/1551761/ref-parameters-and-reflection.
                            mode = parameterInfo.IsOut ? ParameterMode.Out : ParameterMode.Ref;
                        }
                        else if (parameterInfo.IsIn)
                            mode = ParameterMode.In;

                        // Check parameter keyword.
                        if (attribute.parameterType != mode)
                        {
                            string expected = null;
                            if (attribute.parameterType == ParameterMode.Out)
                                expected = "'out'";
                            else if (attribute.parameterType == ParameterMode.Ref)
                                expected = "'ref'";
                            else if (attribute.parameterType == ParameterMode.In)
                                expected = "'in'";
                            else if (attribute.parameterType == ParameterMode.Common)
                                expected = "no";
                            else
                                goto invalidParameterType;

                            string found = null;
                            if (mode == ParameterMode.Out)
                                found = "'out'";
                            else if (mode == ParameterMode.Ref)
                                found = "'ref'";
                            else if (mode == ParameterMode.In)
                                found = "'in'";
                            else if (mode == ParameterMode.Common)
                                found = "no";
                            else
                                System.Diagnostics.Debug.Fail("Impossible State.");

                            if (!(found is null))
                            {
                                Log(GetBuilder()
                                    .Append(" the parameter at index ")
                                    .Append(attribute.parameterNumber - 1)
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
                            attribute.basicTypes,
                            attribute.checkingFlags,
                            attribute.isBlackList,
                            false,
                            parameterInfo.ParameterType
                           ))
                        {
                            Log(AttributeUsageHelper.AppendSupportedTypes(
                                GetBuilder()
                                    .Append(" the parameter at index ")
                                    .Append(attribute.parameterNumber - 1)
                                    .Append(" named ")
                                    .Append(parameterInfo.Name)
                                    .Append(" expected to be of a type that "),
                                attribute.basicTypes,
                                attribute.checkingFlags,
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