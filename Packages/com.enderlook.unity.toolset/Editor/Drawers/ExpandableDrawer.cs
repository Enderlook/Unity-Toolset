using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(ExpandableAttribute), true)]
    internal sealed class ExpandableDrawer : FoldoutDrawer
    {
        protected override bool HAS_FOLDOUT => true;

        protected override ButtonDisplayMode SHOW_OPEN_IN_WINDOW_BUTTON => ButtonDisplayMode.Inline;

        protected override bool SHOW_SCRIPT_FIELD => true;

        protected override BoxColorMode BOX_COLOR => BoxColorMode.Darker;

        protected override int INDENT_WIDTH => 8; // TODO: This is wrong.

        protected override bool SHOW_FIELD => true;

        protected override DrawMode CanDraw(SerializedProperty property, bool log, out string textBoxMessage)
        {
            textBoxMessage = null;
            Type type = property.GetPropertyType();
            if (!typeof(UnityEngine.Object).IsAssignableFrom(type))
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
                    Debug.LogError($"{nameof(ExpandableAttribute)} can only be used on types assignables to {typeof(UnityEngine.Object)}, or array or lists of types whose element types are assignables to it. Property '{property.name}' from '{property.GetParentTargetObject()}' (path '{property.propertyPath}') is of type {type}.");

                textBoxMessage = $"{nameof(ExpandableAttribute)} can only be used on types assignables to {typeof(UnityEngine.Object)}, or array or lists of types whose element types are assignables to it. Property '{property.name}' is of type {type}.";
                return DrawMode.DrawError;
            }

            return DrawMode.DrawComplexField;
        }
    }
}