using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System.Threading;

using UnityEditor;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(RestrictTypeAttribute))]
    internal sealed class RestrictTypeDrawer : StackablePropertyDrawer
    {
        private static GUIContent guiContent;

        private const int MODE_FIRST_TIME = 0;
        private const int MODE_NOT_DRAWN = 1;
        private const int MODE_DRAWN = 2;

        private int mode = MODE_FIRST_TIME;
        private float height;
        private string message;

        private static int i = 1;
        private int id = Interlocked.Increment(ref i);

        protected internal override bool RequestMain => true;

        protected internal override void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            RestrictTypeAttribute attribute = (RestrictTypeAttribute)Attribute;
            if (!attribute.CheckRestrictionFeasibility(property.GetPropertyType(), out string errorMessage))
            {
                mode = MODE_NOT_DRAWN;
                Debug.LogError($"Field {property.name} from {property.propertyPath} error. {errorMessage}");
                string message = $"Field {property.name} error. {errorMessage}";
                float height = SetHeight(message);
                Rect box = new Rect(position.x, position.y, position.width, height);
                EditorGUI.HelpBox(box, message, MessageType.Error);
                return;
            }

            EditorGUI.BeginChangeCheck();
            position.height -= height;
            EditorGUI.PropertyField(position, property, label);

            if (EditorGUI.EndChangeCheck() || mode != MODE_DRAWN)
            {
                mode = MODE_DRAWN;
                UnityObject result = property.objectReferenceValue;
                if (result != null && !attribute.CheckIfTypeIsAllowed(result.GetType(), out errorMessage))
                {
                    Debug.LogError($"Field {property.name} from {property.propertyPath} error. {errorMessage}");
                    message = errorMessage;
                    property.objectReferenceValue = null;
                }
                else
                    message = null;
            }

            if (!(message is null))
            {
                float height = SetHeight(message);
                Rect box = new Rect(position.x, position.y + position.height, position.width, height);
                EditorGUI.HelpBox(box, message, MessageType.Error);
            }
            else
                height = 0;

            float SetHeight(string message)
            {
                GUIContent gui = Interlocked.Exchange(ref guiContent, null) ?? new GUIContent();
                gui.text = message;
                height = GUI.skin.box.CalcHeight(gui, position.width);
                gui.text = null;
                guiContent = gui;
                return height;
            }
        }

        protected internal override float GetPropertyHeight(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            return height + (mode == MODE_NOT_DRAWN ? 0 : base.GetPropertyHeight(property, label, includeChildren));
        }
    }
}