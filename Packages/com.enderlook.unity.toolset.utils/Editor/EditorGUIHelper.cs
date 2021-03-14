using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A set of helper functions for <see cref="EditorGUI"/> and <see cref="EditorGUILayout"/>.
    /// </summary>
    public static class EditorGUIHelper
    {
        private static readonly GUIStyle HORIZONTALA_LINE;

        static EditorGUIHelper()
        {
            HORIZONTALA_LINE = new GUIStyle();
            HORIZONTALA_LINE.normal.background = EditorGUIUtility.whiteTexture;
            HORIZONTALA_LINE.margin = new RectOffset(0, 0, 4, 4);
            HORIZONTALA_LINE.fixedHeight = 1;
        }

        /// <summary>
        /// Add a header.
        /// </summary>
        /// <param name="text">Text of header.</param>
        public static void Header(string text) =>
            // https://www.reddit.com/r/Unity3D/comments/3b43pf/unity_editor_scripting_how_can_i_draw_a_header_in/
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);

        /// <summary>
        /// Add a header.
        /// </summary>
        /// <param name="text1">Text of header.</param>
        /// <param name="text2">Additional text of header.</param>
        public static void Header(string text1, string text2) =>
            EditorGUILayout.LabelField(text1, text2, EditorStyles.boldLabel);

        /// <summary>
        /// Add a header.
        /// </summary>
        /// <param name="content">Content of header.</param>
        public static void Header(GUIContent content) =>
            EditorGUILayout.LabelField(content, EditorStyles.boldLabel);

        /// <summary>
        /// Add a header.
        /// </summary>
        /// <param name="content1">Content of header.</param>
        /// <param name="content2">Additional content of header.</param>
        public static void Header(GUIContent content1, GUIContent content2) =>
            EditorGUILayout.LabelField(content1, content2, EditorStyles.boldLabel);

        /// <summary>
        /// Draw an horizontal line of the specified color.
        /// </summary>
        /// <param name="color">Color of line.</param>
        public static void DrawHorizontalLine(Color color)
        {
            // https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3514941
            Color old = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, HORIZONTALA_LINE);
            GUI.color = old;
        }

        /// <summary>
        /// Draw a gray horizontal line.
        /// </summary>
        public static void DrawHorizontalLine()
            => DrawHorizontalLine(Color.gray);

        /// <summary>
        /// Draw the grey script field of the target script of this custom editor.
        /// </summary>
        /// <param name="source">Custom editor whose target script field will be draw.</param>
        public static void DrawScriptField(this Editor source)
        {
            // https://answers.unity.com/questions/550829/how-to-add-a-script-field-in-custom-inspector.html
            GUI.enabled = false;
            object target = Convert.ChangeType(source.target, source.target.GetType());
            MonoScript script;
            if (source.target.GetType().IsSubclassOf(typeof(MonoBehaviour)))
                script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            else if (source.target.GetType().IsSubclassOf(typeof(ScriptableObject)))
                script = MonoScript.FromScriptableObject((ScriptableObject)target);
            else
                throw new InvalidCastException($"Only support {typeof(MonoBehaviour)} or {typeof(ScriptableObject)}");
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            GUI.enabled = true;
        }
    }
}