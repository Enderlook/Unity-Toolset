using Enderlook.Reflection;
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
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    internal sealed class ShowIfDrawer : SmartPropertyDrawer
    {
        private static readonly Dictionary<(Type type, FieldInfo fieldInfo), Func<object, bool>> members = new Dictionary<(Type type, FieldInfo fieldInfo), Func<object, bool>>();
        private static readonly MethodInfo emptyArrayMethodInfo = typeof(Array).GetMethod(nameof(Array.Empty));
        private static readonly MethodInfo debugLogErrorMethodInfo = typeof(Debug).GetMethod(nameof(Debug.LogError), new Type[] { typeof(object) });
        private static readonly ParameterExpression parameter = Expression.Parameter(typeof(object));
        private static readonly Expression trueConstant = Expression.Constant(true);
        private static readonly Type[] type1 = new Type[1];
        private static readonly object zero = 0;
        private static readonly Type[][] conversions = new Type[][]
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

        [DidReloadScripts]
        private static void Reset() => members.Clear();

        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            object parent = serializedProperty.GetParentTargetObjectOfProperty();
            DisplayMode mode = showIfAttribute.displayMode;

            if (mode == DisplayMode.ShowHide)
            {
                if (IsActive(showIfAttribute, parent))
                    DrawField();
            }
            else if (mode == DisplayMode.EnableDisable)
            {
                EditorGUI.BeginDisabledGroup(IsActive(showIfAttribute, parent));
                DrawField();
                EditorGUI.EndDisabledGroup();
            }

            void DrawField()
            {
                GUIContentHelper.GetGUIContent(property, ref label);
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        protected override float GetPropertyHeightSmart(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            object parent = serializedProperty.GetParentTargetObjectOfProperty();
            if (IsActive(showIfAttribute, parent) || showIfAttribute.displayMode == DisplayMode.EnableDisable)
                return EditorGUI.GetPropertyHeight(property, label, true);
            return 0;
        }

        private bool IsActive(ShowIfAttribute attribute, object parent)
        {
            Type originalType = parent.GetType();

            if (members.TryGetValue((originalType, fieldInfo), out Func<object, bool> func))
                goto end;

            Expression convertedExpression = Expression.Convert(parameter, originalType);
            Expression body = GetExpression(fieldInfo, attribute);
            func = Expression.Lambda<Func<object, bool>>(body, parameter).Compile();
            members.Add((originalType, fieldInfo), func);

            end:
            return func(parent);

            Expression GetExpression(FieldInfo field, ShowIfAttribute showIfAttribute)
            {
                if (string.IsNullOrEmpty(showIfAttribute.firstProperty))
                    return DebugLogError($"Value of property {nameof(showIfAttribute.firstProperty)} is null or empty in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name}.");

                (Expression expression, Type type, FieldInfo field) first = GetValue(showIfAttribute.firstProperty);
                if (first == default)
                    return DebugLogError($"No field, property (with Get method), or method with no mandatory parameters of name '{showIfAttribute.firstProperty}' in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name} was found in object of type {parent.GetType()}.");

                switch (showIfAttribute.mode)
                {
                    case ShowIfAttribute.Mode.Single:
                    {
                        if (first.type != typeof(bool))
                            return DebugLogError($"Value of property {nameof(showIfAttribute.firstProperty)} in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name} is not boolean, but no other property nor compared object was specified.");
                        return first.expression;
                    }
                    case ShowIfAttribute.Mode.WithObject:
                    {
                        object compareTo = showIfAttribute.compareTo;
                        (Expression expression, Type type, FieldInfo field) second = (Expression.Constant(compareTo), compareTo?.GetType() ?? first.type, null);
                        return Compare(first, second, showIfAttribute.comparison);
                    }
                    case ShowIfAttribute.Mode.WithProperty:
                    {
                        (Expression expression, Type type, FieldInfo field) second = GetValue(showIfAttribute.secondProperty);
                        if (second == default)
                            return DebugLogError($"No field, property (with Get method), or method with no mandatory parameters of name '{showIfAttribute.secondProperty}' in attribute {nameof(ShowIfAttribute)} in field {field.Name} of type {field.ReflectedType.Name} was found in object of type {parent.GetType()}.");
                        return Compare(first, second, showIfAttribute.comparison);
                    }
                    default:
                    {
                        Debug.Assert(false, "Imposible state.");
                        return null;
                    }
                }

                (Expression expression, Type type, FieldInfo field) GetValue(string name)
                {
                    Type type = originalType;
                    start:
                    foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (memberInfo.Name != name)
                            continue;

                        if (memberInfo is FieldInfo fieldInfo)
                            return (Expression.Field(fieldInfo.IsStatic ? null : convertedExpression, fieldInfo), fieldInfo.FieldType, showIfAttribute.chain ? fieldInfo : null);

                        if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                            return (Expression.Property(propertyInfo.GetMethod.IsStatic ? null : convertedExpression, propertyInfo), propertyInfo.PropertyType, null);

                        if (memberInfo is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void) && methodInfo.HasNoMandatoryParameters())
                        {
                            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                            Expression[] expressions = new Expression[parameterInfos.Length];
                            for (int i = 0; i < parameterInfos.Length; i++)
                            {
                                ParameterInfo parameterInfo = parameterInfos[i];
                                object constant;
                                if (parameterInfo.IsDefined(typeof(ParamArrayAttribute)))
                                {
                                    type1[0] = parameterInfo.ParameterType;
                                    constant = emptyArrayMethodInfo.MakeGenericMethod(type1).Invoke(null);
                                }
                                else
                                    constant = parameterInfos[i].DefaultValue;
                                expressions[i] = Expression.Constant(constant);
                            }
                            return (Expression.Call(methodInfo.IsStatic ? null : convertedExpression, methodInfo, expressions), methodInfo.ReturnType, null);
                        }
                    }

                    type = type.BaseType;
                    if (type == null)
                        return default;
                    goto start;
                }
            }

            Expression DebugLogError(string message) => Expression.Block(
                Expression.Call(null, debugLogErrorMethodInfo, Expression.Constant(message)),
                trueConstant);

            Expression Compare((Expression expression, Type type, FieldInfo field) first, (Expression expression, Type type, FieldInfo field) second, ComparisonMode mode)
            {
                if (first.type != second.type && first.type.IsValueType && second.type.IsValueType)
                {
                    foreach (Type[] types in conversions)
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
                                Expression.Constant(zero, enumType));
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
                                Expression.Constant(zero, enumType));
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

                if (first.field?.GetCustomAttribute<ShowIfAttribute>() is ShowIfAttribute firstAttribute)
                    expression = Expression.And(GetExpression(first.field, firstAttribute), expression);

                if (second.field?.GetCustomAttribute<ShowIfAttribute>() is ShowIfAttribute secondAttribute)
                    expression = Expression.And(GetExpression(second.field, secondAttribute), expression);

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