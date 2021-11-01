using Enderlook.Unity.Toolset.Utils;

using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Windows
{
    internal sealed class ExpandableWindow : EditorWindow
    {
        private static readonly GUIContent CONTEXT_PROPERTY_MENU = new GUIContent("Open in Window", "Open the Expandable Window");

        // Cached scriptable object editor
        private Editor editor;

        private SerializedProperty property;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        [InitializeOnLoadMethod]
        private static void AddContextualPropertyMenu() => ContextualPropertyMenu.contextualPropertyMenu += (GenericMenu menu, SerializedProperty property) =>
        {
            if (property.IsArrayOrListSize())
                return;

            Type type = property.GetPropertyType();
            if (!(type.IsPrimitive || type == typeof(decimal) || type == typeof(string)))
                menu.AddItem(
                    CONTEXT_PROPERTY_MENU,
                    false,
                    () => CreateWindow(property)
                );
        };

        public static void CreateWindow(SerializedProperty property)
        {
            ExpandableWindow window = GetWindow<ExpandableWindow>();

            window.property = property;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnGUI()
        {
            titleContent = new GUIContent("Expandable Window " + property.displayName);

            if (editor == null)
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

            // Check again because it may not be created by the Editor.CreateChachedEditor
            if (editor != null)
            {
                EditorGUI.BeginChangeCheck();
                editor.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
