using Enderlook.Unity.Toolset.Utils;

using System;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Enderlook.Unity.Toolset.Windows
{
    internal sealed class ExpandableWindow : EditorWindow
    {
        private static readonly GUIContent CONTEXT_PROPERTY_MENU = new GUIContent("Open in Window", "Open the Expandable Window");

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
        private void OnEnable()
        {
            rootVisualElement.schedule.Execute(() =>
            {
                titleContent = new GUIContent("Expandable Window " + property.displayName);

                GUIContent gui = property.GetGUIContent();
                ObjectField field = new ObjectField(gui.text);
                {
                    field.tooltip = gui.tooltip;
                    field.BindProperty(property);
                    field.SetEnabled(false);
                }
                rootVisualElement.Add(field);

                Editor editor = null;
                IMGUIContainer imgui = new IMGUIContainer(() =>
                {
                    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
                    editor.DrawDefaultInspector();
                });
                rootVisualElement.Add(imgui);
            });
        }
    }
}
