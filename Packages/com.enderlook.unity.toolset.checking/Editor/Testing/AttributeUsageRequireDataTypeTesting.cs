using Enderlook.Unity.Toolset.Checking.PostCompiling.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enderlook.Unity.Toolset.Checking
{
    internal static class AttributeUsageRequireDataTypeTesting
    {
        private static readonly Dictionary<Type, (AttributeTargets targets, AttributeUsageRequireDataTypeAttribute attribute)> checkers = new Dictionary<Type, (AttributeTargets targets, AttributeUsageRequireDataTypeAttribute attribute)>();

        [ExecuteWhenCheck(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper")]
        private static void Reset() => checkers.Clear();

        [ExecuteOnEachTypeWhenCheck(TypeFlags.IsNonEnum, 1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void GetAttributesAndTypes(Type type)
        {
            if (type.IsSubclassOf(typeof(Attribute)) && type.GetCustomAttribute<AttributeUsageRequireDataTypeAttribute>(true) is AttributeUsageRequireDataTypeAttribute attribute)
            {
                AttributeUsageAttribute attributeUsageAttribute = type.GetCustomAttribute<AttributeUsageAttribute>();
                checkers.Add(type, (attributeUsageAttribute?.ValidOn ?? AttributeTargets.All, attribute));
            }
        }

        private static void CheckSomething(IEnumerable<Attribute> attributes, IEnumerable<Type> toIgnore, Type type, AttributeTargets checkIf, object memberInfoOrClass)
        {
            foreach (Attribute attribute in attributes)
            {
                Type attributeType = attribute.GetType();
                if (!toIgnore.Contains(attributeType)
                    && checkers.TryGetValue(attributeType, out (AttributeTargets targets, AttributeUsageRequireDataTypeAttribute attribute) tuple)
                    && (tuple.targets & checkIf) != 0
                    && !AttributeUsageHelper.CheckContains(
                        tuple.attribute.types,
                        tuple.attribute.typeRelationship,
                        tuple.attribute.isBlackList,
                        tuple.attribute.supportEnumerables,
                        type
                    ))
                {
                    int capacity = 181 // This value was got by concatenating the sum of the largest path of appended constants in this method.
                        + attributeType.Name.Length
                        + (memberInfoOrClass is MemberInfo memberInfo_ ? memberInfo_.Name.Length + memberInfo_.DeclaringType.ToString().Length : memberInfoOrClass.ToString().Length)
                        + type.ToString().Length
                        + AttributeUsageHelper.GetMaximumRequiredCapacity(tuple.attribute.types);
                    LogBuilder builder = LogBuilder.GetLogger(capacity);

                    builder
                        .Append($"According to attribute '{nameof(AttributeUsageRequireDataTypeAttribute)}', the attribute '")
                        .Append(attributeType.Name)
                        .Append("' on ");

                    if (memberInfoOrClass is MemberInfo memberInfo)
                    {
                        switch (memberInfo.MemberType)
                        {
                            case MemberTypes.Field:
                                builder.Append("field ");
                                break;
                            case MemberTypes.Property:
                                builder.Append("property ");
                                break;
                            case MemberTypes.Method:
                                builder.Append("method ");
                                break;
                        }

                        builder
                            .Append(memberInfo.Name)
                            .Append(" in ")
                            .Append(memberInfo.DeclaringType);
                    }
                    else
                    {
                        builder
                            .Append("class ")
                            .Append(memberInfoOrClass);
                    }

                    builder.Append(" is not valid. The attribute is supported on ");

                    if (memberInfoOrClass is MemberInfo memberInfo2)
                    {
                        switch (memberInfo2.MemberType)
                        {
                            case MemberTypes.Field:
                                builder.Append("fields whose type");
                                break;
                            case MemberTypes.Property:
                                builder.Append("properties whose type");
                                break;
                            case MemberTypes.Method:
                                builder.Append("method whose return's type");
                                break;
                        }
                    }
                    else
                        builder.Append("classes whose type");

                    AttributeUsageHelper.AppendSupportedTypes(
                        builder,
                        tuple.attribute.types,
                        tuple.attribute.typeRelationship,
                        tuple.attribute.isBlackList,
                        tuple.attribute.supportEnumerables
                    );

                    builder
                        .Append(" Type is '")
                        .Append(type)
                        .Append("'.")
                        .LogError();
                }
            }
        }

        private static void CheckSomething(MemberInfo memberInfo, Type type, AttributeTargets checkIf)
            => CheckSomething(memberInfo.GetCustomAttributes(), memberInfo.GetAttributeTypesThatShouldBeIgnored(), type, checkIf, memberInfo);

        [ExecuteOnEachTypeWhenCheck(TypeFlags.IsNonEnum, 2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckClasses(Type type)
        {
            IEnumerable<Type> toIgnore = type.GetAttributeTypesThatShouldBeIgnored();
            if (!toIgnore.Contains(typeof(AttributeUsageMethodAttribute)))
                CheckSomething(type.GetCustomAttributes(), toIgnore, type, AttributeTargets.Class, type);
        }

        [ExecuteOnEachFieldOfEachTypeWhenCheck(FieldSerialization.AnyField, 2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckFields(FieldInfo fieldInfo) => CheckSomething(fieldInfo, fieldInfo.FieldType, AttributeTargets.Field);

        [ExecuteOnEachPropertyOfEachTypeWhenCheck(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckProperties(PropertyInfo propertyInfo) => CheckSomething(propertyInfo, propertyInfo.PropertyType, AttributeTargets.Property);

        [ExecuteOnEachMethodOfEachTypeWhenCheck(2)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by PostCompilingAssembliesHelper.")]
        private static void CheckMethodReturns(MethodInfo methodInfo) => CheckSomething(methodInfo, methodInfo.ReturnType, AttributeTargets.Method);
    }
}
