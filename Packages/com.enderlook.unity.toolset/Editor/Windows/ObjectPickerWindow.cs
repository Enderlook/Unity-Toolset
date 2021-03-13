using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Windows
{
    internal class ObjectPickerWindow : EditorWindow
    {
        private static readonly GUIContent CONTEXT_PROPERTY_MENU = new GUIContent("Object Picker Menu", "Open the Object Picker Menu");
        private static readonly GUIContent TITLE_CONTENT = new GUIContent("Object Picker Menu");
        private static readonly GUIContent REFRESH_SEARCH_CONTENT = new GUIContent("Refesh Search", "Search again for objects");
        private static readonly GUIContent APPLY_CONTENT = new GUIContent("Apply", "Assign object to field");
        private static readonly GUIContent INCLUDE_ASSETS_CONTENT = new GUIContent("Include Assets", "Whenever it should also look for in asset files");

        private SerializedProperty property;
        private Accessors accessors;

        private RestrictTypeAttribute restrictTypeAttribute;

        private UnityObject[] objects;

        private string[] objectsLabel;

        private bool gatherFromAssets = false;

        private int index = -1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        [InitializeOnLoadMethod]
        private static void AddContextualPropertyMenu()
        {
            ContextualPropertyMenu.contextualPropertyMenu += (GenericMenu menu, SerializedProperty property) =>
            {
                if (property.propertyPath.EndsWith(".Array.Size"))
                    return;

                FieldInfo fieldInfo = property.GetFieldInfo();
                if (typeof(UnityObject).IsAssignableFrom(fieldInfo.FieldType))
                    menu.AddItem(
                        CONTEXT_PROPERTY_MENU,
                        false,
                        () => CreateWindow(property, fieldInfo)
                    );
            };
        }

        private static void CreateWindow(SerializedProperty property, FieldInfo fieldInfo)
        {
            ObjectPickerWindow window = GetWindow<ObjectPickerWindow>();

            window.property = property;
            window.accessors = property.GetTargetObjectAccessors();

            window.restrictTypeAttribute = fieldInfo.GetCustomAttribute<RestrictTypeAttribute>();

            window.RefeshObjects();
        }

        private void RefeshObjects()
        {
            UnityObject selected = null;
            if (index != -1)
                selected = objects[index];

            if (gatherFromAssets)
                objects = Resources.FindObjectsOfTypeAll(property.GetValueType());
            else
                objects = FindObjectsOfType(property.GetValueType());

            if (restrictTypeAttribute != null)
                objects = objects.Where(e => restrictTypeAttribute.CheckIfTypeIsAllowed(e.GetType())).ToArray();

            objectsLabel = objects.Select(e => e.ToString()).ToArray();

            if (index != -1)
            {
                index = Array.IndexOf(objects, selected);
                if (index == -1)
                    index = Array.IndexOf(objects, accessors.Get());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnGUI()
        {
            titleContent = TITLE_CONTENT;

            bool oldGatherFromAssets = gatherFromAssets;
            gatherFromAssets = EditorGUILayout.Toggle(INCLUDE_ASSETS_CONTENT, gatherFromAssets);
            if (oldGatherFromAssets != gatherFromAssets)
                RefeshObjects();

            if (GUILayout.Button(REFRESH_SEARCH_CONTENT))
                RefeshObjects();

            index = EditorGUILayout.Popup(index, objectsLabel);

            EditorGUI.BeginDisabledGroup(index == -1);
            if (GUILayout.Button(APPLY_CONTENT))
            {
                accessors.Set(objects[index]);
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}