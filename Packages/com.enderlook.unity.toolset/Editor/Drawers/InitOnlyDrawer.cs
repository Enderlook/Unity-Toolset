﻿using Enderlook.Unity.Toolset.Attributes;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomStackablePropertyDrawer(typeof(InitOnlyAttribute))]
    internal sealed class InitializationFieldDrawer : StackablePropertyDrawer
    {
        protected internal override void BeforeOnGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, ref bool visible)
        {
            if (Application.IsPlaying(property.serializedObject.targetObject))
                EditorGUI.BeginDisabledGroup(true);
        }

        protected internal override void AfterOnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren, bool visible)
        {
            if (Application.IsPlaying(property.serializedObject.targetObject))
                EditorGUI.EndDisabledGroup();
        }
    }
}