using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(ShowIfAttribute))]
    [CustomStackablePropertyDrawer(typeof(EnableIfAttribute))]
    internal sealed class ConditionalHelper : StackablePropertyDrawer
    {
        private static readonly MethodInfo DEBUG_LOG_ERROR_METHOD_INFO = typeof(Debug).GetMethod(nameof(Debug.LogError), new Type[] { typeof(object) });
        private static readonly MethodInfo OBJECT_EQUALS_METHOD_INFO = typeof(object).GetMethod("Equals", new Type[] { typeof(UnityEngine.Object) });
        private static readonly ParameterExpression OBJECT_PARAMETER = Expression.Parameter(typeof(object));
        private static readonly Expression TRUE_CONSTANT = Expression.Constant(true);
        private static readonly Expression NULL_CONSTANT = Expression.Constant(null);
        private static readonly object Zero = 0;
        private static readonly Type[][] CONVERSIONS = new Type[][]
        { // The first element of each array is the key.
            new Type[] { typeof(sbyte),     typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) },
            new Type[] { typeof(byte),      typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)},
            new Type[] { typeof(short),     typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)},
            new Type[] { typeof(ushort),    typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)},
            new Type[] { typeof(int),       typeof(long), typeof(float), typeof(double), typeof(decimal) },
            new Type[] { typeof(uint),      typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)},
            new Type[] { typeof(long),      typeof(float), typeof(double), typeof(decimal) },
            new Type[] { typeof(ulong),     typeof(float), typeof(double), typeof(decimal) },
            new Type[] { typeof(float),     typeof(double) },
        };
        private static readonly (Type key, Expression value)[] NUMERIC_TYPES = new (Type key, Expression value)[]
        {
            (typeof(sbyte), Expression.Constant((sbyte)0)),
            (typeof(byte), Expression.Constant((byte)0)),
            (typeof(short), Expression.Constant((short)0)),
            (typeof(ushort), Expression.Constant((ushort)0)),
            (typeof(int), Expression.Constant((int)0)),
            (typeof(uint), Expression.Constant((uint)0)),
            (typeof(long), Expression.Constant((long)0)),
            (typeof(ulong), Expression.Constant((ulong)0)),
            (typeof(float), Expression.Constant((float)0)),
            (typeof(double), Expression.Constant((double)0)),
            (typeof(decimal), Expression.Constant((decimal)0)),
        };
        private static readonly string[] TRY_NUMERIC_NAMES = new string[] { "Length", "Count", "GetCount", "GetLength" };
        private static readonly (string key, bool value)[] TRY_BOOLEAN_NAMES = new (string key, bool value)[] {
            ("HasValue", true), ("HasAny", true), ("IsDefault", false), ("IsDefaultOrEmpty", false), ("IsEmpty", false)
        };

        private static readonly Dictionary<(Type type, MemberInfo memberInfo), Func<object, bool>> members = new Dictionary<(Type type, MemberInfo memberInfo), Func<object, bool>>();
        private static ReadWriteLock membersLock = new ReadWriteLock();

        [DidReloadScripts]
        private static void Reset()
        {
            membersLock.WriteBegin();
            {
                members.Clear();
            }
            membersLock.WriteEnd();
        }

        private bool off;

        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            if (visible && (off = !IsActive(property)))
            {
                if (Attribute is EnableIfAttribute)
                    EditorGUI.BeginDisabledGroup(true);
                else
                    visible = false;
            }
        }

        protected internal override void AfterOnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren, bool visible)
        {
            if (off && Attribute is EnableIfAttribute)
                EditorGUI.EndDisabledGroup();
        }

        protected internal override void BeforeGetPropertyHeight(ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            if (visible && (off = !IsActive(property)) && Attribute is ShowIfAttribute)
                visible = false;
        }

        protected internal override float AfterGetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren, float height)
            => off && Attribute is ShowIfAttribute ? 0 : height;

        private bool IsActive(SerializedProperty property)
        {
            MemberInfo memberInfo = property.GetMemberInfo();
            object parent = property.GetParentTargetObject();
            Type originalType = parent.GetType();

            bool found;
            Func<object, bool> func;
            membersLock.ReadBegin();
            {
                found = members.TryGetValue((originalType, memberInfo), out func);
            }
            membersLock.ReadEnd();
            if (found)
                goto end;

            Expression convertedExpression = Expression.Convert(OBJECT_PARAMETER, originalType);
            Expression body = GetExpression(memberInfo, (IConditionalAttribute)Attribute);
            func = Expression.Lambda<Func<object, bool>>(body, OBJECT_PARAMETER).Compile();

            Func<object, bool> func2;
            membersLock.ReadBegin();
            {
                found = members.TryGetValue((originalType, memberInfo), out func2);
            }
            membersLock.ReadEnd();

            if (found)
                func = func2;
            else
            {
                membersLock.WriteBegin();
                {
                    members.Add((originalType, memberInfo), func);
                }
                membersLock.WriteEnd();
            }

            end:
            return func(parent);

            Expression GetExpression(MemberInfo member, IConditionalAttribute conditionalAttribute)
            {
                if (string.IsNullOrEmpty(conditionalAttribute.FirstProperty))
                    return DebugLogError($"Value of property '{nameof(conditionalAttribute.FirstProperty)}' is null or empty in attribute '{conditionalAttribute.GetType()}' in field '{member.Name}' of type '{member.ReflectedType}'.");

                (Expression expression, Type type, FieldInfo field) first = GetValue(member.ReflectedType, convertedExpression, conditionalAttribute.FirstProperty);
                if (first == default)
                    return DebugLogError($"No field, property (with Get method), or method with no mandatory parameters of name '{conditionalAttribute.FirstProperty}' for attribute '{conditionalAttribute.GetType()}' in field '{member.Name}' of type '{originalType}' was found in object of type '{member.ReflectedType}'.");

                switch (conditionalAttribute.Mode)
                {
                    case ConditionalMode.Single:
                    {
                        Expression result = null;
                        FieldInfo fieldInfo = null;

                        if (first.type == typeof(bool))
                        {
                            result = first.expression;
                            fieldInfo = first.field;
                            goto next;
                        }

                        foreach ((string key, bool value) kvp in TRY_BOOLEAN_NAMES)
                        {
                            (Expression subExpression, Type subType, FieldInfo fieldInfo) tuple = GetValue(first.type, first.expression, kvp.key);
                            if (tuple.subType == typeof(bool))
                            {
                                result = kvp.value ? tuple.subExpression : Expression.Not(tuple.subExpression);
                                fieldInfo = tuple.fieldInfo;
                                goto next;
                            }
                        }

                        foreach (string name in TRY_NUMERIC_NAMES)
                        {
                            (Expression subExpression, Type subType, FieldInfo fieldInfo) tuple = GetValue(first.type, first.expression, name);
                            if (tuple.subType is null)
                                continue;
                            for (int i = 0; i < NUMERIC_TYPES.Length; i++)
                            {
                                if (NUMERIC_TYPES[i].key == tuple.subType)
                                {
                                    result = Expression.GreaterThan(tuple.subExpression, NUMERIC_TYPES[i].value);
                                    fieldInfo = tuple.fieldInfo;
                                    goto next;
                                }
                            }
                        }

                        MethodInfo trueOperator = first.type.GetMethod("op_True");
                        if (!(trueOperator is null) && trueOperator.IsStatic && trueOperator.ReturnType == typeof(bool))
                        {
                            ParameterInfo[] parameterInfos = trueOperator.GetParameters();
                            if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsAssignableFrom(first.type))
                                result = Expression.IsTrue(first.expression);
                        }

                    next:
                        if (!first.type.IsValueType)
                        {
                            Expression notNull = Expression.NotEqual(first.expression, NULL_CONSTANT);
                            if (typeof(UnityEngine.Object).IsAssignableFrom(first.type))
                                notNull = Expression.AndAlso(notNull, Expression.Not(Expression.Call(first.expression, OBJECT_EQUALS_METHOD_INFO, NULL_CONSTANT)));
                            if (!(result is null))
                                result = Expression.AndAlso(notNull, result);
                            else
                                result = notNull;
                        }

                        if (result is null)
                            return DebugLogError($"Value of property '{nameof(conditionalAttribute.FirstProperty)}' in attribute' {conditionalAttribute.GetType()}' in field '{member.Name}' of type '{member.ReflectedType}' and no other property nor compared object was specified.\n" +
                                $"Valid types are '{typeof(Boolean)}', reference types, types that can be casted to '{typeof(Boolean)}', or any type with field, property (with Get method) or method with no mandatory parameters of name 'Length', 'Count', 'GetLength' or 'GetCount' that returns a numeric type or 'HasAny', 'IsEmpty', 'IsDefault' or 'IsDefaultOrEmpty' that returns '{typeof(Boolean)}'.");

                        IEnumerable<Attribute> enumeration = fieldInfo?.GetCustomAttributes();
                        if (!(enumeration is null))
                        {
                            foreach (Attribute attribute in enumeration)
                                if (attribute is IConditionalAttribute conditionalAttribute_)
                                    result = Expression.And(GetExpression(first.field, conditionalAttribute_), result);
                        }

                        return result;
                    }
                    case ConditionalMode.WithObject:
                    {
                        object compareTo = conditionalAttribute.CompareTo;
                        (Expression expression, Type type, FieldInfo field) second = (Expression.Constant(compareTo), compareTo?.GetType() ?? first.type, null);
                        return Compare(first, second, conditionalAttribute.Comparison);
                    }
                    case ConditionalMode.WithProperty:
                    {
                        (Expression expression, Type type, FieldInfo field) second = GetValue(originalType, convertedExpression, conditionalAttribute.SecondProperty);
                        if (second == default)
                            return DebugLogError($"No field, property (with Get method), or method with no mandatory parameters of name '{conditionalAttribute.SecondProperty}' in attribute '{conditionalAttribute.GetType()}' in field '{member.Name}' of type '{member.ReflectedType}' was found in object of type '{parent.GetType()}'.");
                        return Compare(first, second, conditionalAttribute.Comparison);
                    }
                    default:
                    {
                        Debug.Assert(false, "Imposible state.");
                        return null;
                    }
                }

                (Expression expression, Type type, FieldInfo field) GetValue(Type inputType, Expression expression, string name)
                {
                    Type type = inputType;
                    start:
                    foreach (MemberInfo element in type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (element.Name != name)
                            continue;

                        if (element is FieldInfo fieldInfo)
                            return (Expression.Field(fieldInfo.IsStatic ? null : expression, fieldInfo), fieldInfo.FieldType, conditionalAttribute.Chain ? fieldInfo : null);

                        if (element is PropertyInfo propertyInfo && propertyInfo.CanRead)
                            return (Expression.Property(propertyInfo.GetMethod.IsStatic ? null : expression, propertyInfo), propertyInfo.PropertyType, null);

                        if (element is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void) && methodInfo.HasNoMandatoryParameters())
                        {
                            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                            Expression[] expressions = new Expression[parameterInfos.Length];
                            for (int i = 0; i < parameterInfos.Length; i++)
                            {
                                ParameterInfo parameterInfo = parameterInfos[i];
                                object constant;
                                if (parameterInfo.IsDefined(typeof(ParamArrayAttribute)))
                                    constant = ReflectionHelper.EmptyArray(parameterInfo.ParameterType.GetElementType());
                                else
                                    constant = parameterInfos[i].DefaultValue;
                                expressions[i] = Expression.Constant(constant);
                            }
                            return (Expression.Call(methodInfo.IsStatic ? null : expression, methodInfo, expressions), methodInfo.ReturnType, null);
                        }
                    }

                    type = type.BaseType;
                    if (type == null)
                        return default;
                    goto start;
                }
            }

            Expression DebugLogError(string message) => Expression.Block(
                Expression.Call(null, DEBUG_LOG_ERROR_METHOD_INFO, Expression.Constant(message)),
                TRUE_CONSTANT);

            Expression Compare((Expression expression, Type type, FieldInfo field) first, (Expression expression, Type type, FieldInfo field) second, ComparisonMode mode)
            {
                if (first.type != second.type && first.type.IsValueType && second.type.IsValueType)
                {
                    foreach (Type[] types in CONVERSIONS)
                    {
                        if (Check(ref first, second, types))
                            break;
                        if (Check(ref second, first, types))
                            break;
                    }
                }

                Expression expression;
                try
                {
                    switch (mode)
                    {
                        case ComparisonMode.Equal:
                            expression = Expression.Equal(first.expression, second.expression);
                            break;
                        case ComparisonMode.NotEqual:
                            expression = Expression.NotEqual(first.expression, second.expression);
                            break;
                        case ComparisonMode.Greater:
                            expression = Expression.GreaterThan(first.expression, second.expression);
                            break;
                        case ComparisonMode.GreaterOrEqual:
                            expression = Expression.GreaterThanOrEqual(first.expression, second.expression);
                            break;
                        case ComparisonMode.Less:
                            expression = Expression.LessThan(first.expression, second.expression);
                            break;
                        case ComparisonMode.LessOrEqual:
                            expression = Expression.LessThanOrEqual(first.expression, second.expression);
                            break;
                        case ComparisonMode.And:
                            expression = Expression.And(first.expression, second.expression);
                            break;
                        case ComparisonMode.Or:
                            expression = Expression.Or(first.expression, second.expression);
                            break;
                        case ComparisonMode.HasFlag:
                        {
                            Type enumType = Enum.GetUnderlyingType(first.type);
                            expression = Expression.NotEqual(
                                Expression.Convert(
                                    Expression.Convert(
                                        Expression.And(
                                            Expression.Convert(first.expression, enumType),
                                            Expression.Convert(second.expression, enumType)),
                                        first.type),
                                    enumType),
                                Expression.Constant(Zero, enumType));
                            break;
                        }
                        case ComparisonMode.NotFlag:
                        {
                            Type enumType = Enum.GetUnderlyingType(first.type);
                            expression = Expression.Equal(
                                Expression.Convert(
                                    Expression.Convert(
                                        Expression.And(
                                            Expression.Convert(first.expression, enumType),
                                            Expression.Convert(second.expression, enumType)),
                                        first.type),
                                    enumType),
                                Expression.Constant(Zero, enumType));
                            break;
                        }
                        default:
                            expression = DebugLogError("Invalid comparison mode.");
                            break;
                    }
                }
                catch (InvalidOperationException exception)
                {
                    expression = DebugLogError(exception.Message);
                }

                IEnumerable<Attribute> enumeration = first.field?.GetCustomAttributes();
                if (!(enumeration is null))
                {
                    foreach (Attribute attribute in enumeration)
                        if (attribute is IConditionalAttribute conditionalAttribute)
                            expression = Expression.And(GetExpression(first.field, conditionalAttribute), expression);
                }

                enumeration = second.field?.GetCustomAttributes();
                if (!(enumeration is null))
                {
                    foreach (Attribute attribute in enumeration)
                        if (attribute is IConditionalAttribute conditionalAttribute)
                            expression = Expression.And(GetExpression(second.field, conditionalAttribute), expression);
                }

                return expression;

                bool Check(ref (Expression expression, Type type, FieldInfo field) a, (Expression expression, Type type, FieldInfo field) b, Type[] types)
                {
                    if (a.type == types[0])
                    {
                        for (int i = 1; i < types.Length; i++)
                        {
                            if (b.type == types[i])
                            {
                                a.expression = Expression.Convert(a.expression, b.type);
                                a.type = b.type;
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
        }
    }
}