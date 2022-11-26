using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(InlineAttribute), true)]
    internal sealed class InlineDrawer : FoldoutDrawer
    {
        protected override bool HAS_FOLDOUT => false;

        protected override ButtonDisplayMode SHOW_OPEN_IN_WINDOW_BUTTON => ButtonDisplayMode.None;

        protected override bool SHOW_SCRIPT_FIELD => false;

        protected override BoxColorMode BOX_COLOR => BoxColorMode.None;

        protected override int INDENT_WIDTH => 0;

        protected override bool SHOW_FIELD
        {
            get
            {
                if (!FieldInfo.FieldType.IsArrayOrList(out Type elementType))
                    elementType = FieldInfo.FieldType;
                return typeof(UnityEngine.Object).IsAssignableFrom(elementType);
            }
        }

        protected override DrawMode CanDraw(SerializedProperty property, bool log, out string textBoxMessage)
        {
            textBoxMessage = null;
            Type type = property.GetPropertyType();
            if (type.IsPrimitive)
            {
#if UNITY_2020_1_OR_NEWER
                MemberInfo member = property.GetMemberInfo();
                Type declaringType = member.DeclaringType;
                if (declaringType.IsGenericType)
                {
                    int metadataToken = member.MetadataToken;
                    foreach (MemberInfo genericMember in declaringType.GetGenericTypeDefinition().GetMember(member.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (genericMember.MetadataToken == metadataToken)
                        {
                            Type type_ = genericMember is FieldInfo genericField ? genericField.FieldType : ((PropertyInfo)genericMember).PropertyType;
                            if (type_.IsArrayOrList(out Type elementType))
                                type_ = elementType;
                            if (type_.IsGenericParameter)
                                return DrawMode.DrawSimpleField;
                            break;
                        }
                    }
                }
#endif
                if (log)
                    Debug.LogError($"{nameof(InlineAttribute)} can not be used on types that are primitive, or array or lists of types whose element types are. Property '{property.name}' from '{property.GetParentTargetObject()}' (path '{property.propertyPath}') is of type {type}.");

                textBoxMessage = $"{nameof(InlineAttribute)} can not be used on types that are primitive, or array or lists of types whose element types are. Property '{property.name}' is of type {type}.";
                return DrawMode.DrawError;
            }

            return DrawMode.DrawComplexField;
        }
    }
}