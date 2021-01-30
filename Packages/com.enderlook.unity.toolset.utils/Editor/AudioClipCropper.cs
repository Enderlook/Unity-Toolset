using Enderlook.Unity.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    public class AudioClipCropper : EditorWindow
    {
        private static readonly GUIContent TITLE_CONTENT = new GUIContent("Audio Clip Cropper");
        private static readonly GUIContent ADD_AUDIO_CLIP_CONTENT = EditorGUIUtility.IconContent("d_SVN_AddedLocal", "Add new Audio Clip.");
        private static readonly GUIContent REMOVE_ALL_CONTENT = new GUIContent("Remove All");
        private static readonly GUIContent CROP_CONTENT = new GUIContent("Crop Audio Clips", "Creates new  Audio Clips with the specified ranges.");
        private static GUIContent ADD_CROP_CONTENT;
        private static GUIContent REMOVE_CONTENT;

        private List<AudioClipEdit> clips = new List<AudioClipEdit>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnGUI()
        {
            titleContent = TITLE_CONTENT;

            EditorGUI.BeginDisabledGroup(!clips.Any(e => e != null && e.crops.Count > 0));
            if (GUILayout.Button(CROP_CONTENT))
            {
                Crop();
                clips.Clear();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(clips.Count == 0);
            if (GUILayout.Button(REMOVE_ALL_CONTENT))
                clips.Clear();
            EditorGUI.EndDisabledGroup();

            if (clips.Count > 0)
                for (int i = clips.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(REMOVE_CONTENT, EditorStyles.miniButton))
                        clips.RemoveAt(i);

                    AudioClipEdit audioClipEdit = clips[i];
                    audioClipEdit.clip = (AudioClip)EditorGUILayout.ObjectField(clips[i].clip, typeof(AudioClip), allowSceneObjects: false);

                    GUILayout.Label($"{ToTime(audioClipEdit.clip.length)}", EditorStyles.miniLabel);

                    EditorGUILayout.EndHorizontal();

                    List<Vector2> crops = audioClipEdit.crops;
                    EditorGUI.BeginDisabledGroup(audioClipEdit.clip == null);
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginDisabledGroup(crops.Count == 0);
                    if (GUILayout.Button(REMOVE_ALL_CONTENT, EditorStyles.miniButton))
                        crops.Clear();
                    EditorGUI.EndDisabledGroup();

                    if (crops.Count > 0)
                        for (int j = crops.Count - 1; j >= 0; j--)
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(REMOVE_CONTENT, EditorStyles.miniButton))
                                crops.RemoveAt(j);

                            Vector2 vector2 = crops[j];

                            vector2.x = FloatField(vector2.x);
                            vector2.y = FloatField(vector2.y);

                            if (vector2.x < 0)
                                vector2.x = 0;
                            if (vector2.y < 0)
                                vector2.y = 0;
                            if (audioClipEdit.clip != null)
                            {
                                if (vector2.x > audioClipEdit.clip.length)
                                    vector2.x = audioClipEdit.clip.length;
                                if (vector2.y > audioClipEdit.clip.length)
                                    vector2.y = audioClipEdit.clip.length;
                            }

                            GUILayout.Label($"{ToTime(vector2.y - vector2.x)}", EditorStyles.miniLabel);

                            crops[i] = vector2;
                            EditorGUILayout.EndHorizontal();
                        }

                    if (GUILayout.Button(ADD_CROP_CONTENT))
                        crops.Add(new Vector2(0, audioClipEdit.clip.length));

                    EditorGUI.indentLevel--;
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndVertical();
                }

            if (GUILayout.Button(ADD_AUDIO_CLIP_CONTENT))
                clips.Add(new AudioClipEdit());

            float FloatField(float value)
            {
                string old = value.ToString();
                string @new = GUILayout.TextField(old);
                if (old == @new)
                    return value;
                return float.Parse(string.Concat(@new.Where(e => char.IsNumber(e) || e == '.' || e == ',')));
            }
        }

        private void Crop()
        {
            foreach (AudioClipEdit clip in clips)
            {
                foreach (Vector2 crop in clip.crops)
                {
                    AudioClip audioClip = clip.clip;
                    AudioClip newClip = audioClip.CreateSlice(crop.x, crop.y - crop.x);

                    AssetDatabaseHelper.CreateAsset(
                        newClip,
                        AssetDatabaseHelper.GetAssetDirectory(audioClip)
                        + '/'
                        + AssetDatabaseHelper.GetAssetFileNameWithoutExtension(audioClip)
                        + $"_start_{ToTime(crop.x)}_end_{ToTime(crop.y)}".Replace(':', '_')
                        + AssetDatabaseHelper.GetAssetExtension(audioClip)
                    );
                }
            }
        }

        [MenuItem("Assets/Enderlook/Crop AudioClip")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private static void CreateWindow()
        {
            AudioClipCropper window = GetWindow<AudioClipCropper>();
            window.clips = Selection.GetFiltered<AudioClip>(SelectionMode.DeepAssets)
                .Select(e => new AudioClipEdit(e)).ToList();

            // Can't be called in the static constructor
            if (ADD_CROP_CONTENT is null)
                ADD_CROP_CONTENT = EditorGUIUtility.IconContent("d_SVN_AddedLocal", "Add new crop.");
            if (REMOVE_CONTENT is null)
                REMOVE_CONTENT = EditorGUIUtility.IconContent("winbtn_win_close_h", "Remove");
        }

        [MenuItem("Assets/Enderlook/Crop AudioClip", true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private static bool CreateWindowValidator()
            => Selection.GetFiltered<AudioClip>(SelectionMode.DeepAssets).Length > 0;

        private static string ToTime(float source) => TimeSpan.FromSeconds(source).ToString(@"hh\:mm\:ss\:fff");

        private class AudioClipEdit
        {
            public AudioClip clip;
            public List<Vector2> crops;

            public AudioClipEdit() : this(null) { }

            public AudioClipEdit(AudioClip clip)
            {
                this.clip = clip;
                crops = new List<Vector2>() { new Vector2(0, clip?.length ?? 0) };
            }
        }
    }
}
