using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Checking
{
    public sealed class AttributeUsageMethodHelper
    {
        private readonly AttributeUsageMethodAttribute attribute;

        private HashSet<Type> Types => types ?? (types = AttributeUsageHelper.GetHashsetTypes(attribute.basicTypes, false));
        private HashSet<Type> types;

        private string allowedTypes;
        private string AllowedTypes => allowedTypes ?? (allowedTypes = AttributeUsageHelper.GetTextTypes(types, attribute.checkingFlags, false));

        private const string MESSAGE_BASE = "According to " + nameof(AttributeUsageMethodAttribute) + ",";

        public AttributeUsageMethodHelper(AttributeUsageMethodAttribute attribute)
        {
            this.attribute = attribute;
            types = null;
            allowedTypes = null;
        }

        /// <summary>
        /// Unity Editor only.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="attributeName"></param>
        /// <remarks>Only use in Unity Editor.</remarks>
        public void CheckAllowance(MethodInfo methodInfo, string attributeName)
        {
            string GetMessageInit() => $"{MESSAGE_BASE} {attributeName} require a method with";

            if (attribute.parameterNumber == 0)
            {
                // Check return type
                Type type = methodInfo.ReturnType;

                if (attribute.parameterType == ParameterMode.VoidOrNone)
                {
                    if (type == typeof(void))
                        Debug.LogError($"{GetMessageInit()} a return type must be {typeof(void).Name}. {methodInfo.Name} is {type} type.");
                }
                else if (attribute.parameterType == ParameterMode.Common)
                    AttributeUsageHelper.CheckContains(
                        nameof(AttributeUsageMethodAttribute),
                        Types,
                        attribute.checkingFlags,
                        false,
                        AllowedTypes,
                        type,
                        attributeName,
                        "Return Type"
                    );
                else
                    Debug.LogError($"{nameof(AttributeUsageMethodAttribute)} can only have as {nameof(attribute.parameterType)} {nameof(ParameterMode.Common)} or {nameof(ParameterMode.VoidOrNone)} if used with {nameof(attribute.parameterNumber)} 0. Attribute in {attributeName} has a {nameof(attribute.parameterNumber)} 0 but {nameof(attribute.parameterType)} {attribute.parameterType}.");
            }
            else
            {
                // Check parameter
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                ParameterInfo parameterInfo;
                try
                {
                    parameterInfo = parameterInfos[attribute.parameterNumber - 1];
                }
                catch (IndexOutOfRangeException)
                {
                    // Parameter doesn't exist, check if was on purpose
                    if (attribute.parameterType != ParameterMode.VoidOrNone)
                        Debug.LogError($"{GetMessageInit()} a {attribute.parameterNumber}º parameter. {methodInfo.Name} only have {parameterInfos.Length} parameter{(parameterInfos.Length > 0 ? "s" : "")}.");
                    return;
                }

                // Check if should exist
                if (attribute.parameterType == ParameterMode.VoidOrNone)
                {
                    Debug.LogError($"{GetMessageInit()} only {attribute.parameterNumber - 1} parameter{(attribute.parameterNumber - 1 > 0 ? "s" : "")}."); // - 1 because the last parameter is ParameterMode.VoidOrNone
                    return;
                }

                // Get parameter keyword
                ParameterMode mode = ParameterMode.Common;
                if (parameterInfo.ParameterType.IsByRef)
                {
                    // Both `out` and `ref` has IsByRef = true, so we must check IsOut in order to split them.
                    // https://stackoverflow.com/a/38110036 from https://stackoverflow.com/questions/1551761/ref-parameters-and-reflection
                    if (parameterInfo.IsOut)
                        mode = ParameterMode.Out;
                    else
                        mode = ParameterMode.Out;
                }
                else if (parameterInfo.IsIn)
                    mode = ParameterMode.In;

                // Check parameter keyword
                void RaiseKeyWordError(string keywordString)
                    => Debug.LogError($"{GetMessageInit()} a parameter at {parameterInfo.Position} position named {parameterInfo.Name} that has {keywordString} keyword, according to {nameof(attribute.parameterType)} {nameof(ParameterMode)} {mode}. Instead has {attribute.parameterType}.");
                if (attribute.parameterType != mode)
                {
                    if (attribute.parameterType != ParameterMode.Out)
                    {
                        RaiseKeyWordError("'out'");
                        return;
                    }
                    else if (attribute.parameterType != ParameterMode.Ref)
                    {
                        RaiseKeyWordError("'ref'");
                        return;
                    }
                    else if (attribute.parameterType != ParameterMode.In)
                    {
                        RaiseKeyWordError("'in'");
                        return;
                    }
                    else if (attribute.parameterType != ParameterMode.Common)
                    {
                        RaiseKeyWordError("no");
                        return;
                    }
                    else
                        System.Diagnostics.Debug.Fail("Impossible State.");
                }

                if (Types.Count != 0) // It 0, any type is allowed
                    AttributeUsageHelper.CheckContains(
                        nameof(AttributeUsageMethodAttribute),
                        Types,
                        attribute.checkingFlags,
                        false,
                        AllowedTypes,
                        parameterInfo.ParameterType,
                        $"{attributeName} parameter {parameterInfo.Name} in position {parameterInfo.Position}",
                        "Parameter"
                    );
            }
        }
    }
}