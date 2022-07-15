using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    public sealed class SerializedPropertyInfo
    {
        private static readonly object Dummy = new object();

        private readonly SerializedProperty serializedProperty;
        public SerializedProperty SerializedProperty => serializedProperty;

        private MemberInfo memberInfo;
        public MemberInfo MemberInfo
        {
            get
            {
                if (memberInfo is null)
                    memberInfo = serializedProperty.GetMemberInfo();
                return memberInfo;
            }
        }

        public Type MemberType
        {
            get
            {
                MemberInfo memberInfo = MemberInfo;
                if (memberInfo is FieldInfo fieldInfo)
                    return fieldInfo.FieldType;
                if (memberInfo is PropertyInfo propertyInfo)
                    return propertyInfo.PropertyType;
                if (memberInfo is MethodInfo methodInfo)
                    return methodInfo.DeclaringType;
                Debug.Assert(false, "Unexpected member type");
                return null;
            }
        }

        private object parentTargetObject = Dummy;
        public object ParentTargetObject
        {
            get
            {
                if (parentTargetObject == Dummy)
                    parentTargetObject = serializedProperty.GetParentTargetObject();
                return parentTargetObject;
            }
        }

        public SerializedPropertyInfo(SerializedProperty serializedProperty)
        {
            this.serializedProperty = serializedProperty;
            memberInfo = null;
        }

        internal SerializedPropertyInfo(SerializedProperty serializedProperty, MemberInfo memberInfo)
        {
            this.serializedProperty = serializedProperty;
            this.memberInfo = memberInfo;
        }
    }
}