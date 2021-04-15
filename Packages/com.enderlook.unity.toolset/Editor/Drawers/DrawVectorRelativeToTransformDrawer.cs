using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;
using Enderlook.Unity.Utils.Math;

using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(DrawVectorRelativeToTransformAttribute)), InitializeOnLoad]
    internal sealed class DrawVectorRelativeToTransformEditor : SmartPropertyDrawer
    {
        private static readonly Handles.CapFunction handleCap = Handles.SphereHandleCap;

        private static readonly string vectorTypes = $"{nameof(Vector3)}, {nameof(Vector3Int)}, {nameof(Vector2)}, {nameof(Vector2Int)}, {nameof(Vector4)}";

        private static readonly char[] splitByBracket = new char[] { '[' };
        private static readonly char[] splitByDot = new char[] { '.' };

        private static readonly GUIContent CLOSE_BUTTON = new GUIContent("Close", "Closes this panel.");
        private static readonly GUIContent OBJECT_CONTENT = new GUIContent("Object", "From which object this property cames.");
        private static readonly GUIContent PATH_CONTENT = new GUIContent("Path", "Path of the property to be edited.");
        private static readonly GUIContent PROPERTY_CONTENT = new GUIContent("Property", "Name of the property to be edited.");
        private static readonly GUIContent ABSOLUTE_POSITION_CONTENT = new GUIContent("Absolute Position", "Determines the absolute position of this property.");
        private static readonly GUIContent RELATIVE_POSITION_CONTENT = new GUIContent("Relative Position", "Determines the relative position of this property.");
        private static readonly GUIContent REFERENCE_POSITION_CONTENT = new GUIContent("Reference Position", "Determines the reference position of this property.");

        private static (SerializedProperty serializedProperty, Vector3 position, DrawVectorRelativeToTransformAttribute drawVectorRelativeToTransform, FieldInfo field) selected;

        private static readonly GUIContent OBJECT_LABEL = new GUIContent();
        private static readonly GUIContent PROPERTY_PATH_LABEL = new GUIContent();
        private static readonly GUIContent PROPERTY_NAME_LABEL = new GUIContent();

        private static bool showButton;

        protected override void OnGUISmart(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        static DrawVectorRelativeToTransformEditor() => SceneView.duringSceneGui += RenderSceneGUI;

        private static Vector3 DrawHandle(Vector3 position, bool usePositionHandle)
        {
            if (showButton)
                return position;

            return usePositionHandle
                ? Handles.PositionHandle(position, Quaternion.identity)
                : Handles.FreeMoveHandle(position, Quaternion.identity, GetSize(position), Vector2.one, handleCap);
        }

        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static Vector3 GetPositionByTransform(SerializedObject serializedObject) => ((Component)serializedObject.targetObject).transform.position;

        private static void DisplayErrorReference(string name) => throw new Exception($"The serialized property reference {name} isn't neither {vectorTypes} nor {nameof(Transform)}.");

        private static Vector3 GetVector3ValueOf(SerializedProperty serializedProperty)
        {
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector3:
                    return serializedProperty.vector3Value;
                case SerializedPropertyType.Vector3Int:
                    return serializedProperty.vector3IntValue;
                case SerializedPropertyType.Vector2:
                    return serializedProperty.vector2Value;
                case SerializedPropertyType.Vector2Int:
                    return (Vector2)serializedProperty.vector2IntValue;
                case SerializedPropertyType.Vector4:
                    return serializedProperty.vector4Value;
                case SerializedPropertyType.ObjectReference:
                    if (serializedProperty.objectReferenceValue is Transform transform)
                        return transform.position;
                    else if (serializedProperty.GetFieldInfo().FieldType == typeof(Transform))
                        return Vector3.zero;
                    DisplayErrorReference(serializedProperty.name);
                    break;
                default:
                    DisplayErrorReference(serializedProperty.name);
                    break;
            }
            Debug.LogError("Impossible State");
            return default;
        }

        private static Vector3 CastToVector3(object source, Type sourceType)
        {
            if (sourceType == typeof(Vector3))
                return (Vector3)source;
            if (sourceType == typeof(Vector3Int))
                return (Vector3Int)source;
            if (sourceType == typeof(Vector2))
                return (Vector2)source;
            if (sourceType == typeof(Vector2Int))
                return (Vector2)(Vector2Int)source;
            if (sourceType == typeof(Vector4))
                return (Vector4)source;
            // If everything fails, perform a last 99% error-warranted salvage cast
            return (Vector3)source;
        }

        private static Vector3 GetReference(SerializedProperty serializedProperty, string referenceName)
        {
            SerializedObject serializedObject = serializedProperty.serializedObject;

            if (string.IsNullOrEmpty(referenceName))
                return GetPositionByTransform(serializedObject);

            SerializedProperty referenceProperty = serializedObject.FindProperty(referenceName);
            if (referenceProperty == null)
            {
                UnityEngine.Object targetObject = serializedObject.targetObject;
                Type type = targetObject.GetType();

                FieldInfo fieldInfo = type.GetField(referenceName, bindingFlags);
                if (fieldInfo == null)
                {
                    PropertyInfo propertyInfo = type.GetProperty(referenceName, bindingFlags);
                    if (propertyInfo == null)
                    {
                        MethodInfo methodInfo = type.GetMethod(referenceName, bindingFlags);
                        if (methodInfo != null)
                        {
                            if (methodInfo.HasNoMandatoryParameters(out object[] parameters))
                                // Get reference by method
                                return CastToVector3(methodInfo.Invoke(targetObject, parameters), methodInfo.ReturnType);
                            else
                                Debug.LogError($"Found method {methodInfo.Name} on {type.Name} based on reference name {referenceName} in {serializedObject.targetObject.name} requested by property {serializedProperty.propertyPath}.");
                        }
                        // If everything fails, use zero
                        return Vector3.zero;
                    }
                    else
                        // Get reference by property
                        return CastToVector3(propertyInfo.GetValue(targetObject), propertyInfo.PropertyType);
                }
                else
                    // Get reference by field
                    return CastToVector3(fieldInfo.GetValue(targetObject), fieldInfo.FieldType);
            }
            else
                // Get reference by serialized field
                return GetVector3ValueOf(referenceProperty);
        }

        private static Vector3 ToAbsolutePosition(SerializedProperty serializedProperty, Vector3 reference)
        {
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    return serializedProperty.vector2Value + (Vector2)reference;
                case SerializedPropertyType.Vector2Int:
                    return (Vector2)(serializedProperty.vector2IntValue + reference.ToVector2Int());
                case SerializedPropertyType.Vector3:
                    return serializedProperty.vector3Value + reference;
                case SerializedPropertyType.Vector3Int:
                    return serializedProperty.vector3IntValue + reference.ToVector3Int();
                default:
                    Debug.LogError($"The attribute {nameof(DrawVectorRelativeToTransformAttribute)} is only allowed in types of {nameof(Vector2)}, {nameof(Vector2Int)}, {nameof(Vector3)} and {nameof(Vector3Int)}.");
                    return Vector3.zero;
            }
        }

        private static void SetFromAbsolutePosition(SerializedProperty serializedProperty, Vector3 world, Vector3 reference)
        {
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    serializedProperty.vector2Value = world - reference;
                    break;
                case SerializedPropertyType.Vector2Int:
                    serializedProperty.vector2IntValue = ((Vector2)world).ToVector2Int() - reference.ToVector2Int();
                    break;
                case SerializedPropertyType.Vector3:
                    serializedProperty.vector3Value = world - reference;
                    break;
                case SerializedPropertyType.Vector3Int:
                    serializedProperty.vector3IntValue = world.ToVector3Int() - reference.ToVector3Int();
                    break;
                default:
                    Debug.LogError($"The attribute {nameof(DrawVectorRelativeToTransformAttribute)} is only allowed in types of {nameof(Vector2)}, {nameof(Vector2Int)}, {nameof(Vector3)} and {nameof(Vector3Int)}.");
                    break;
            }
        }

        private static void RenderSingleSerializedProperty(SerializedProperty serializedProperty, DrawVectorRelativeToTransformAttribute drawVectorRelativeToTransform, Vector3 reference, FieldInfo field)
        {
            Vector3 absolutePosition = ToAbsolutePosition(serializedProperty, reference);
            absolutePosition = DrawHandle(absolutePosition, drawVectorRelativeToTransform.usePositionHandler);
            SetFromAbsolutePosition(serializedProperty, absolutePosition, reference);

            if (!string.IsNullOrEmpty(drawVectorRelativeToTransform.icon))
            {
                Texture2D texture = (Texture2D)EditorGUIUtility.Load(drawVectorRelativeToTransform.icon);
                if (texture == null)
                    texture = AssetDatabase.LoadAssetAtPath<Texture2D>(drawVectorRelativeToTransform.icon);
                if (texture == null)
                    texture = Resources.Load<Texture2D>(drawVectorRelativeToTransform.icon);

                if (texture == null)
                    Debug.LogError($"The Texture '{drawVectorRelativeToTransform.icon}' used by '{serializedProperty.propertyPath}' could not be found.");
                else
                    Handles.Label(absolutePosition, texture);
            }

            CheckForSelection(absolutePosition, serializedProperty, drawVectorRelativeToTransform, field);
        }

        private static void CheckForSelection(Vector3 position, SerializedProperty serializedProperty, DrawVectorRelativeToTransformAttribute drawVectorRelativeToTransform, FieldInfo field)
        {
            if (!showButton)
                return;

            float size = GetSize(position);

            if (Handles.Button(position, Quaternion.identity, size, size, handleCap))
                selected = (serializedProperty.Copy(), position, drawVectorRelativeToTransform, field);
        }

        private static float GetSize(Vector3 position) => HandleUtility.GetHandleSize(position) * .1f;

        private static void RenderSceneGUI(SceneView sceneview)
        {
            if (Event.current != null)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.control)
                    showButton = true;
                else if (Event.current.type == EventType.KeyUp)
                    showButton = false;
            }

            foreach ((SerializedProperty serializedProperty, FieldInfo field, DrawVectorRelativeToTransformAttribute drawVectorRelativeToTransform, Editor editor) in PropertyDrawerHelper.FindAllSerializePropertiesInActiveEditorWithTheAttribute<DrawVectorRelativeToTransformAttribute>())
            {
                serializedProperty.serializedObject.Update();
                Vector3 reference = GetReference(serializedProperty, drawVectorRelativeToTransform.reference);

                if (serializedProperty.isArray)
                    for (int i = 0; i < serializedProperty.arraySize; i++)
                    {
                        SerializedProperty item = serializedProperty.GetArrayElementAtIndex(i);
                        RenderSingleSerializedProperty(item, drawVectorRelativeToTransform, reference, field);
                    }
                else
                    RenderSingleSerializedProperty(serializedProperty, drawVectorRelativeToTransform, reference, field);

                serializedProperty.serializedObject.ApplyModifiedProperties();
            }

            TryDrawPanel();
        }

        private static void TryDrawPanel()
        {
            if (!(selected.serializedProperty is null))
            {
                try
                {
                    Handles.BeginGUI();
                    Rect screenRect = new Rect(0, 0, 300, 175);
                    GUILayout.BeginArea(screenRect);
                    if (Event.current.type == EventType.Repaint)
                        GUI.skin.box.Draw(screenRect, GUIContent.none, false, true, true, false);
                    EditorGUI.BeginChangeCheck();

                    OBJECT_LABEL.text = selected.serializedProperty.serializedObject.targetObject.name;
                    EditorGUILayout.LabelField(OBJECT_CONTENT, OBJECT_LABEL);

                    PROPERTY_PATH_LABEL.text = $"{selected.field.DeclaringType.Name}.{selected.serializedProperty.propertyPath.Replace(".Array.data", "")}";
                    EditorGUILayout.LabelField(PATH_CONTENT, PROPERTY_PATH_LABEL);

                    if (selected.serializedProperty.propertyPath.EndsWith("]"))
                    {
                        string[] parts = selected.serializedProperty.propertyPath.Replace(".Array.data", "").Split(splitByBracket);
                        string rawNumber = parts[parts.Length - 1];
                        string number = rawNumber.Substring(0, rawNumber.Length - 1);
                        string[] prefixes = parts[parts.Length - 2].Split(splitByDot);
                        string prefix = prefixes[prefixes.Length - 1];
                        PROPERTY_NAME_LABEL.text = $"{ObjectNames.NicifyVariableName(prefix)}[{number}]"; 
                    }
                    else
                        PROPERTY_NAME_LABEL.text = selected.serializedProperty.displayName;
                    EditorGUILayout.LabelField(PROPERTY_CONTENT, PROPERTY_NAME_LABEL);

                    EditorGUILayout.PropertyField(selected.serializedProperty, RELATIVE_POSITION_CONTENT);

                    Vector3 reference = GetReference(selected.serializedProperty, selected.drawVectorRelativeToTransform.reference);
                    Vector3 absolutePosition = ToAbsolutePosition(selected.serializedProperty, reference);
                    absolutePosition = EditorGUILayout.Vector3Field(ABSOLUTE_POSITION_CONTENT, absolutePosition);
                    SetFromAbsolutePosition(selected.serializedProperty, absolutePosition, reference);

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Vector3Field(REFERENCE_POSITION_CONTENT, reference);
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button(CLOSE_BUTTON))
                        selected = default;
                }
                catch
                {
                    selected = default;
                }
                finally
                {
                    if (EditorGUI.EndChangeCheck() && selected != default)
                        selected.serializedProperty.serializedObject.ApplyModifiedProperties();
                    GUILayout.EndArea();
                    Handles.EndGUI();
                }
            }
        }
    }
}