using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Enderlook.Unity.Toolset.Windows
{
    internal sealed partial class ObjectWindow
    {
        private class Field : VisualElement
        {
            private readonly ObjectField objectField;
            private readonly TextField nameField;
            private readonly PopupField<HideFlags> hideFlags;
            private readonly ScrollView scroll;
            private InspectorElement inspector;

            private Object selected;

            public Field(string label, SerializedProperty property) : this(label, property.displayName, false)
            {
                objectField.BindProperty(property);
                Set(property.objectReferenceValue);
                nameField.RegisterValueChangedCallback(e => property.objectReferenceValue.name = e.newValue);
            }

            public Field(string label, string objectLabel) : this(label, objectLabel, false)
            {
                Set(null);
                nameField.RegisterValueChangedCallback(e => selected.name = e.newValue);
            }

            private Field(string label, string objectFieldLabel, bool _)
            {
                Label label_ = new Label(label);
                {
                    label_.style.alignSelf = Align.Center;
                    label_.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                Add(label_);

                objectField = new ObjectField(objectFieldLabel);
                {
                    objectField.style.flexDirection = FlexDirection.Row;
                }
                Add(objectField);

                VisualElement div = new VisualElement();
                {
                    nameField = new TextField("Name");
                    {
                        nameField.tooltip = "Determines the name of the object.";
                    }
                    div.Add(nameField);

                    hideFlags = new PopupField<HideFlags>("Hide Flags", HIDE_FLAGS, default(HideFlags));
                    {
                        hideFlags.tooltip = "Determines the visiblity of this object in the editor.";
                    }
                    div.Add(hideFlags);
                }
                Add(div);

                Box box = new Box();
                {
                    box.style.flexGrow = 1f;
                    scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                    box.Add(scroll);
                }
                Add(box);
            }

            public void Set(Object objectReferenceValue)
            {
                if (!objectField.IsBound())
                {
                    selected = objectReferenceValue;
                    objectField.value = objectReferenceValue;
                }

                if (objectReferenceValue != null)
                {
                    nameField.style.display = DisplayStyle.Flex;
                    hideFlags.style.display = DisplayStyle.Flex;

                    nameField.value = objectReferenceValue.name;
                    hideFlags.value = objectReferenceValue.hideFlags;

                    if (!(inspector is null))
                        inspector.RemoveFromHierarchy();

                    inspector = new InspectorElement(new SerializedObject(objectReferenceValue));
                    scroll.Add(inspector);
                }
                else
                {
                    nameField.style.display = DisplayStyle.None;
                    hideFlags.style.display = DisplayStyle.None;
                }
            }
        }
    }
}