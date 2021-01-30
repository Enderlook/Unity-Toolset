using Enderlook.Reflection;

using System;
using System.Reflection;

using UnityEditor;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Utils
{
    public readonly struct SerializedPropertyWrapper
    {
        public SerializedProperty Property { get; }

        public FieldInfo FieldInfo { get; }

        public IAccessors<object> Accessors { get; }

        public Type Type {
            get {
                if (Property.objectReferenceValue)
                    return Property.GetFieldType();
                else if (FieldInfo.FieldType.TryGetElementTypeOfArrayOrList(out Type type))
                    return type;
                else
                    return FieldInfo.FieldType;
            }
        }

        public SerializedPropertyWrapper(SerializedProperty property, FieldInfo fieldInfo)
        {
            Property = property;
            FieldInfo = fieldInfo;

            /* If the property came from an array and the element is null this will be null which is a problem for us.
             * This is also null if the property isn't array but the field is empty (null). That is also a problem. */
            if (property.objectReferenceValue)
                Accessors = property.GetTargetObjectAccessors();
            else
            {
                UnityObject targetObject = property.serializedObject.targetObject;
                Type fieldType = fieldInfo.FieldType;
                // Just confirming that it's an array
                if (fieldType.IsArray)
                {
                    int index = property.GetIndexFromArray();

                    if (fieldInfo.GetValue(targetObject) is Array array)
                    {
                        /* Until an element is in-Inspector dragged to the array element field, it seems that Unity doesn't rebound the array
                         * So if the array is empty and it doesn't have space for us, we make a new array and inject it. */
                        if (array.Length == 0)
                        {
                            array = Array.CreateInstance(fieldType.GetElementType(), 1);
                            fieldInfo.SetValue(targetObject, array);
                            Accessors = new Accessors(array, index);
                        }
                        else
                            Accessors = new Accessors(array, index);
                    }
                    else
                        throw new InvalidCastException();
                }
                else
                    Accessors = new PropertyAccessor(property, fieldInfo);
            }
        }

        public void ApplyModifiedProperties() => Property.serializedObject.ApplyModifiedProperties();
    }
}
