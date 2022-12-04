using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [InitializeOnLoad]
    internal static class DrawVectorRelativeToTransformEditor
    {
        private const string MENU_NAME_DRAW = "Enderlook/Toolset/Draw Vector Relative To Transform/Enable Visualization";
        private const string MENU_NAME_DRAW_CONFIG = "Enderlook/Toolset/DrawVectorRelativeToTransform/EnableVisualization";
        private const string MENU_NAME_EDIT = "Enderlook/Toolset/Draw Vector Relative To Transform/Enable Scene GUI Editing";
        private const string MENU_NAME_EDIT_CONFIG = "Enderlook/Toolset/DrawVectorRelativeToTransform/EnableSceneGUIEditing";

        private static readonly Handles.CapFunction HANDLE_CAP = Handles.SphereHandleCap;

        private static readonly string TYPES_RESTRICTED = $" doesn't have a type assignable to: {typeof(Vector2)}, {typeof(Vector2Int)}, {typeof(Vector3)}, {typeof(Vector3Int)}, {typeof(Vector4)}, {typeof(GameObject)}, {typeof(Transform)} or {typeof(Component)}.";

        private static readonly char[] SPLIT_BY_BRACKET = new char[] { '[' };
        private static readonly char[] SPLIT_BY_DOT = new char[] { '.' };

        private static readonly GUIContent CLOSE_BUTTON = new GUIContent("Close", "Closes this panel.");
        private static readonly GUIContent OBJECT_CONTENT = new GUIContent("Object", "From which object this property cames.");
        private static readonly GUIContent PATH_CONTENT = new GUIContent("Path", "Path of the property to be edited.");
        private static readonly GUIContent PROPERTY_CONTENT = new GUIContent("Property", "Name of the property to be edited.");
        private static readonly GUIContent ABSOLUTE_POSITION_CONTENT = new GUIContent("Absolute Position", "Determines the absolute position of this property.");
        private static readonly GUIContent RELATIVE_POSITION_CONTENT = new GUIContent("Relative Position", "Determines the relative position of this property.");
        private static readonly GUIContent REFERENCE_POSITION_CONTENT = new GUIContent("Reference Position", "Determines the reference position of this property.");

        private static (SerializedProperty serializedProperty, Vector3 position, DrawVectorRelativeToTransformAttribute drawVectorRelativeToTransform, MemberInfo memberInfo) selected;

        private static bool showButton;

        private static bool enableVisualization;
        private static bool enableGUI;

        private static Vector3 scrollPosition;

        static DrawVectorRelativeToTransformEditor()
        {
            SceneView.duringSceneGui += RenderSceneGUI;
            enableVisualization = EditorPrefs.GetBool(MENU_NAME_DRAW_CONFIG, true);
            enableGUI = EditorPrefs.GetBool(MENU_NAME_EDIT_CONFIG, true);
            EditorApplication.delayCall += () => SetFeature(enableVisualization, enableGUI);
        }

        [MenuItem(MENU_NAME_DRAW)]
        private static void ToggleVisualize() => SetFeature(!enableVisualization, enableGUI);

        [MenuItem(MENU_NAME_EDIT)]
        private static void ToggleGUI() => SetFeature(enableVisualization, !enableGUI);

        [MenuItem(MENU_NAME_EDIT, true)]
        private static bool ToggleGUIValidation() => enableVisualization;

        private static void SetFeature(bool visualization, bool gui)
        {
            enableVisualization = visualization;
            enableGUI = gui;
            Menu.SetChecked(MENU_NAME_DRAW, visualization);
            EditorPrefs.SetBool(MENU_NAME_DRAW_CONFIG, visualization);
            Menu.SetChecked(MENU_NAME_EDIT, gui);
            EditorPrefs.SetBool(MENU_NAME_EDIT_CONFIG, gui);
            if (!gui)
                showButton = false;
        }

        private static Vector3 DrawHandle(Vector3 position, bool usePositionHandle)
        {
            if (showButton)
                return position;

            return usePositionHandle
                ? Handles.PositionHandle(position, Quaternion.identity)
                : Handles.FreeMoveHandle(position, Quaternion.identity, GetSize(position), Vector2.one, HANDLE_CAP);
        }

        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static LogBuilder GetLogger(SerializedProperty serializedProperty)
        {
            string propertyName = serializedProperty.name;
            string propertyPath = serializedProperty.propertyPath;
            string targetObjectName = serializedProperty.serializedObject.targetObject.name;
            // This value was got by concatenating the sum of the largest possible path of appended constants that can happen in methods of this class,
            // and an approximate length of variables.
            int minCapacity = 360 + 80 + propertyName.Length + propertyPath.Length + targetObjectName.Length;
            return LogBuilder.GetLogger(minCapacity)
                .Append($"Invalid use {nameof(DrawVectorRelativeToTransformAttribute)} ")
                .Append(" on serialized property '")
                .Append(propertyName)
                .Append("' at path '")
                .Append(propertyPath)
                .Append("' on object at name '")
                .Append(targetObjectName)
                .Append("':");
        }

        private static bool IsValidType(Type type) => type != typeof(Vector2)
            && type != typeof(Vector2Int)
            && type != typeof(Vector3)
            && type != typeof(Vector3Int)
            && type != typeof(Vector4)
            && type != typeof(GameObject)
            && typeof(Transform).IsAssignableFrom(type)
            && typeof(Component).IsAssignableFrom(type);

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
                // Get position from transform of current monobehaviour.
                return ((Component)serializedObject.targetObject).transform.position;

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
                        if (methodInfo != null && methodInfo.HasNoMandatoryParameters(out object[] parameters))
                        {
                            // Get reference by method.
                            Type methodReturnType = methodInfo.ReturnType;
                            if (!IsValidType(methodReturnType))
                                return InvalidType("return type of method", methodInfo.Name, methodReturnType);

                            return CastToVector3(methodInfo.Invoke(targetObject, parameters), methodReturnType);
                        }
                        else
                        {
                            GetLogger(serializedProperty)
                                .Append(" no field, property (with Get method), or method with no mandatory parameters of name '")
                                .Append(referenceName)
                                .Append("' was found in serialized object.")
                                .LogError();
                            return Vector2.zero;
                        }
                    }
                    else
                    {
                        // Get reference by property.

                        Type propertyType = propertyInfo.PropertyType;
                        if (!IsValidType(propertyType))
                            return InvalidType("property", propertyInfo.Name, propertyType);

                        return CastToVector3(propertyInfo.GetValue(targetObject), propertyType);
                    }
                }
                else
                {
                    // Get reference by field.

                    Type fieldType = fieldInfo.FieldType;
                    if (!IsValidType(fieldType))
                        return InvalidType("field", fieldInfo.Name, fieldType);

                    return CastToVector3(fieldInfo.GetValue(targetObject), fieldType);
                }
            }
            else
            {
                // Get reference by serialized field.
                switch (referenceProperty.propertyType)
                {
                    case SerializedPropertyType.Vector3:
                        return referenceProperty.vector3Value;
                    case SerializedPropertyType.Vector3Int:
                        return referenceProperty.vector3IntValue;
                    case SerializedPropertyType.Vector2:
                        return referenceProperty.vector2Value;
                    case SerializedPropertyType.Vector2Int:
                        return (Vector2)referenceProperty.vector2IntValue;
                    case SerializedPropertyType.Vector4:
                        return referenceProperty.vector4Value;
                    case SerializedPropertyType.ObjectReference:
                        object reference = referenceProperty.objectReferenceValue;
                        if (reference is Transform transform)
                            return transform.position;
                        if (reference is GameObject gameObject)
                            return gameObject.transform.position;
                        if (reference is Component component)
                            return component.transform.position;
                        Type type = referenceProperty.GetPropertyType();
                        if (typeof(Transform).IsAssignableFrom(type) || typeof(Component).IsAssignableFrom(type) || type == typeof(GameObject))
                            return Vector2.zero;
                        goto default;
                    default:
                        return InvalidType("serialized property", referenceProperty.name, referenceProperty.GetPropertyType());
                }
            }

            Vector3 InvalidType(string member, string name, Type type)
            {
                GetLogger(serializedProperty)
                    .Append(member)
                    .Append(" named ")
                    .Append(name)
                    .Append(TYPES_RESTRICTED)
                    .Append(" Is of type ")
                    .Append(type)
                    .Append('.')
                    .LogError();
                return Vector2.zero;
            }
        }

        private static Vector3? ToAbsolutePosition(SerializedProperty serializedProperty, Vector3 reference)
        {
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    return serializedProperty.vector2Value + (Vector2)reference;
                case SerializedPropertyType.Vector2Int:
                    return (Vector2)(serializedProperty.vector2IntValue + ToVector2Int(reference));
                case SerializedPropertyType.Vector3:
                    return serializedProperty.vector3Value + reference;
                case SerializedPropertyType.Vector3Int:
                    return serializedProperty.vector3IntValue + ToVector3Int(reference);
                case SerializedPropertyType.Vector4:
                    return (Vector3)serializedProperty.vector4Value + reference;
                case SerializedPropertyType.ObjectReference:
                    object referenceObject = serializedProperty.objectReferenceValue;
                    if (referenceObject is Transform transform)
                        return transform.position + reference;
                    if (referenceObject is GameObject gameObject)
                        return gameObject.transform.position + reference;
                    if (referenceObject is Component component)
                        return component.transform.position + reference;
                    Type type = serializedProperty.GetPropertyType();
                    if (typeof(Transform).IsAssignableFrom(type) || typeof(Component).IsAssignableFrom(type) || type == typeof(GameObject))
                        return null;
                    goto default;
                default:
                    GetLogger(serializedProperty)
                        .Append(" property")
                        .Append(TYPES_RESTRICTED)
                        .Append(". Is of type '")
                        .Append(serializedProperty.GetPropertyType())
                        .Append('.')
                        .LogError();
                    return null;
            }
        }

        private static void SetFromAbsolutePosition(SerializedProperty serializedProperty, Vector3 world, Vector3 reference)
        {
            Vector3 value = world - reference;
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    serializedProperty.vector2Value = value;
                    break;
                case SerializedPropertyType.Vector2Int:
                    serializedProperty.vector2IntValue = ToVector2Int(value);
                    break;
                case SerializedPropertyType.Vector3:
                    serializedProperty.vector3Value = value;
                    break;
                case SerializedPropertyType.Vector3Int:
                    serializedProperty.vector3IntValue = ToVector3Int(value);
                    break;
                case SerializedPropertyType.Vector4:
                    serializedProperty.vector4Value = new Vector4(value.x, value.y, value.z, serializedProperty.vector4Value.w);
                    break;
                case SerializedPropertyType.ObjectReference:
                    object referenceObject = serializedProperty.objectReferenceValue;
                    if (referenceObject is Transform transform)
                        transform.position = value;
                    if (referenceObject is GameObject gameObject)
                        gameObject.transform.position = value;
                    if (referenceObject is Component component)
                        component.transform.position = value;
#if DEBUG
                    Type type = serializedProperty.GetPropertyType();
                    if (typeof(Transform).IsAssignableFrom(type) || typeof(Component).IsAssignableFrom(type) || type == typeof(GameObject))
                        break;
                    goto default;
#else
                    break;
#endif
                default:
                    System.Diagnostics.Debug.Fail("Already checked before.");
                    break;
            }
        }

        private static void RenderSingleSerializedProperty(SerializedProperty serializedProperty, DrawVectorRelativeToTransformAttribute drawVectorRelativeToTransform, Vector3 reference, MemberInfo memberInfo)
        {
            Vector3? absolutePosition_ = ToAbsolutePosition(serializedProperty, reference);
            if (absolutePosition_ is Vector3 absolutePosition)
            {
                absolutePosition = DrawHandle(absolutePosition, drawVectorRelativeToTransform.usePositionHandler);
                SetFromAbsolutePosition(serializedProperty, absolutePosition, reference);

                if (!showButton && !string.IsNullOrEmpty(drawVectorRelativeToTransform.icon))
                {
                    Texture2D texture = (Texture2D)EditorGUIUtility.Load(drawVectorRelativeToTransform.icon);
                    if (texture == null)
                        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(drawVectorRelativeToTransform.icon);
                    if (texture == null)
                        texture = Resources.Load<Texture2D>(drawVectorRelativeToTransform.icon);

                    if (texture == null)
                        GetLogger(serializedProperty)
                            .Append("the texture '")
                            .Append(drawVectorRelativeToTransform.icon)
                            .Append("' could not be found.")
                            .LogError();
                    else
                        Handles.Label(absolutePosition, texture);
                }

                CheckForSelection(absolutePosition, serializedProperty, drawVectorRelativeToTransform, memberInfo);
            }
        }

        private static void CheckForSelection(Vector3 position, SerializedProperty serializedProperty, DrawVectorRelativeToTransformAttribute drawVectorRelativeToTransform, MemberInfo memberInfo)
        {
            if (!showButton)
                return;

            float size = GetSize(position);

            if (Handles.Button(position, Quaternion.identity, size, size, HANDLE_CAP))
                selected = (serializedProperty.Copy(), position, drawVectorRelativeToTransform, memberInfo);
        }

        private static float GetSize(Vector3 position) => HandleUtility.GetHandleSize(position) * .1f;

        private static void RenderSceneGUI(SceneView sceneview)
        {
            if (!enableVisualization)
                return;

            if (enableGUI && Event.current != null)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.control)
                {
                    showButton = true;
                    scrollPosition = Vector3.zero;
                }
                else if (Event.current.type == EventType.KeyUp)
                    showButton = false;
            }

            foreach (Editor editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                SerializedProperty serializedProperty = editor.serializedObject.GetIterator();
                while (serializedProperty.Next(true))
                {
                    // Used to skip missing components.
                    if (serializedProperty.serializedObject.targetObject == null)
                        continue;
                    // Used to catch all properties with errors, such as Unity-related fields that aren't as (like those fields which starts with `m_`)
                    if (!serializedProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                        continue;
                    if (!(memberInfo.GetCustomAttribute(typeof(DrawVectorRelativeToTransformAttribute), true) is DrawVectorRelativeToTransformAttribute attribute))
                        continue;

                    serializedProperty.serializedObject.Update();
                    Vector3 reference = GetReference(serializedProperty, attribute.reference);

                    if (serializedProperty.isArray)
                    {
                        for (int i = 0; i < serializedProperty.arraySize; i++)
                        {
                            SerializedProperty item = serializedProperty.GetArrayElementAtIndex(i);
                            RenderSingleSerializedProperty(item, attribute, reference, memberInfo);
                        }
                    }
                    else
                        RenderSingleSerializedProperty(serializedProperty, attribute, reference, memberInfo);

                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            TryDrawPanel();
        }

        private static void TryDrawPanel()
        {
            if (!(selected.serializedProperty is null))
            {
                Handles.BeginGUI();
                Rect screenRect = new Rect(0, 0, 300, (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 10 + (EditorGUIUtility.standardVerticalSpacing * 2));
                GUILayout.BeginArea(screenRect);
                if (Event.current.type == EventType.Repaint)
                    GUI.skin.box.Draw(screenRect, GUIContent.none, false, true, true, false);
                EditorGUI.BeginChangeCheck();
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                try
                {
                    EditorGUILayout.LabelField(OBJECT_CONTENT, new GUIContent(selected.serializedProperty.serializedObject.targetObject.name));

                    EditorGUILayout.LabelField(PATH_CONTENT, new GUIContent($"{selected.memberInfo.DeclaringType.Name}.{selected.serializedProperty.propertyPath.Replace(".Array.data", "")}"));

                    string propertyName;
                    if (selected.serializedProperty.propertyPath.EndsWith("]"))
                    {
                        string[] parts = selected.serializedProperty.propertyPath.Replace(".Array.data", "").Split(SPLIT_BY_BRACKET);
                        string rawNumber = parts[parts.Length - 1];
                        string number = rawNumber.Substring(0, rawNumber.Length - 1);
                        string[] prefixes = parts[parts.Length - 2].Split(SPLIT_BY_DOT);
                        string prefix = prefixes[prefixes.Length - 1];
                        propertyName = $"{ObjectNames.NicifyVariableName(prefix)}[{number}]";
                    }
                    else
                        propertyName = selected.serializedProperty.displayName;
                    EditorGUILayout.LabelField(PROPERTY_CONTENT, new GUIContent(propertyName));

                    if (selected.serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        object obj = selected.serializedProperty.objectReferenceValue;
                        if (obj != null)
                        {
                            if (!(obj is Transform transform))
                            {
                                if (obj is Component component)
                                    transform = component.transform;
                                else if (obj is GameObject gameObject)
                                    transform = gameObject.transform;
                                else
                                    goto drawProperty;
                            }

                            transform.position = EditorGUILayout.Vector3Field(RELATIVE_POSITION_CONTENT, transform.position);
                            goto afterDrawProperty;
                        }
                    }

                drawProperty:
                    EditorGUILayout.PropertyField(selected.serializedProperty, RELATIVE_POSITION_CONTENT);
                afterDrawProperty:

                    Vector3 reference = GetReference(selected.serializedProperty, selected.drawVectorRelativeToTransform.reference);
                    Vector3? absolutePosition_ = ToAbsolutePosition(selected.serializedProperty, reference);
                    if (absolutePosition_ is Vector3 absolutePosition)
                    {
                        absolutePosition = EditorGUILayout.Vector3Field(ABSOLUTE_POSITION_CONTENT, absolutePosition);
                        SetFromAbsolutePosition(selected.serializedProperty, absolutePosition, reference);
                    }
                    else
                        EditorGUILayout.HelpBox($"Property {selected.serializedProperty.name} {TYPES_RESTRICTED}", MessageType.Error);

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
                    EditorGUILayout.EndScrollView();
                    if (EditorGUI.EndChangeCheck() && selected != default)
                        selected.serializedProperty.serializedObject.ApplyModifiedProperties();
                    GUILayout.EndArea();
                    Handles.EndGUI();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2Int ToVector2Int(Vector2 source) => new Vector2Int((int)source.x, (int)source.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3Int ToVector3Int(Vector3 source) => new Vector3Int((int)source.x, (int)source.y, (int)source.z);
    }
}