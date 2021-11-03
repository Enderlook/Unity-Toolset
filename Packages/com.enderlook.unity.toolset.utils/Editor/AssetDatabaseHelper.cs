using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A set of helper functions for easy usage of <see cref="AssetDatabase"/>.
    /// </summary>
    public static class AssetDatabaseHelper
    {
        private static readonly char[] slashSeparator = new char[] { '/' }; // TODO: On .NET standard 2.1 use string.Split(char, StringSplitOptions) instead
        private const string NOT_FOUND_ASSET = "Not found asset";

        private static string GetPathFromAssets(string path)
        {
            path = path.Replace('\\', '/');
            return path.StartsWith("Assets/") || path.StartsWith("Assets//") ? path : "Assets/" + path;
        }

        private static void CreateDirectoryIfDoesNotExist(string path)
        {
            string directory = Path.GetDirectoryName(path);
            try
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            catch (ArgumentException)
            {
                string[] paths = directory.Split(slashSeparator);
                for (int i = 0; i < paths.Length - 1; i++)
                    AssetDatabase.CreateFolder(string.Join("/", paths.Take(i + 1)), paths[i + 1]);
            }
        }

        /// <summary>
        /// Save asset to path, creating the necessaries directories.<br/>
        /// It automatically add "Assets/" to the <paramref name="path"/> if it doesn't have.
        /// </summary>
        /// <param name="asset">Asset to save.</param>
        /// <param name="path">Path to save file</param>
        /// <param name="generateUniquePath">If <see language="true"/> it will change the name of file to avoid name collision.</param>
        /// <return>Path to created file.</return>
        public static string CreateAsset(UnityObject asset, string path, bool generateUniquePath = false)
        {
            if (asset == null) Helper.ThrowArgumentNullException_Asset();
            if (path is null) Helper.ThrowArgumentNullException_Path();
            if (path.Length == 0) Helper.ThrowArgumentException_PathCannotBeEmpty();

            path = GetPathFromAssets(path);
            CreateDirectoryIfDoesNotExist(path);
            if (generateUniquePath)
                path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            return path;
        }

        /// <summary>
        /// Save asset to path, creating the necessaries directories.<br/>
        /// All assets are stored in the same file.<br/>
        /// It automatically add "Assets/" to the <paramref name="path"/> if it doesn't have.
        /// </summary>
        /// <param name="objects">Assets to save.</param>
        /// <param name="path">Path to save file</param>
        /// <param name="generateUniquePath">If <see language="true"/> it will change the name of file to avoid name collision.</param>
        /// <return>Path to created file.</return>
        public static void CreateAssetFromObjects(IEnumerable<UnityObject> objects, string path, bool generateUniquePath = false)
        {
            if (objects == null) Helper.ThrowArgumentNullException_Asset();
            if (path is null) Helper.ThrowArgumentNullException_Path();
            if (path.Length == 0) Helper.ThrowArgumentException_PathCannotBeEmpty();

            path = CreateAsset(objects.First(), path, generateUniquePath);
            foreach (UnityObject @object in objects.Skip(1))
            {
                AssetDatabase.AddObjectToAsset(@object, path);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Add object to asset in path, creating the necessaries directories.<br/>
        /// It automatically add "Assets/" to the <paramref name="path"/> if it doesn't have.
        /// </summary>
        /// <param name="objectToAdd">Asset to add.</param>
        /// <param name="path">Path to save file</param>
        /// <param name="createIfNotExist">If <see language="true"/> it will create the asset if it doesn't exist.</param>
        /// <return>Path to created or modified file.</return>
        public static string AddObjectToAsset(UnityObject objectToAdd, string path, bool createIfNotExist = false)
        {
            if (objectToAdd == null) Helper.ThrowArgumentNullException_ObjectToAdd();
            if (path is null) Helper.ThrowArgumentNullException_Path();
            if (path.Length == 0) Helper.ThrowArgumentException_PathCannotBeEmpty();

            path = GetPathFromAssets(path);
            CreateDirectoryIfDoesNotExist(path);

            if (File.Exists(path))
                AssetDatabase.AddObjectToAsset(objectToAdd, path);
            else
            {
                if (createIfNotExist)
                    AssetDatabase.CreateAsset(objectToAdd, path);
                else
                    Throw();
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            return path;

            void Throw() => throw new FileNotFoundException(NOT_FOUND_ASSET, path);
        }

        /// <summary>
        /// Add objects to asset in path, creating the necessaries directories.<br/>
        /// It automatically add "Assets/" to the <paramref name="path"/> if it doesn't have.
        /// </summary>
        /// <param name="objectsToAdd">Objects to add to asset to add.</param>
        /// <param name="path">Path to save file</param>
        /// <param name="createIfNotExist">If <see language="true"/> it will create the asset if it doesn't exist.</param>
        /// <return>Path to created or modified file.</return>
        public static string AddObjectToAsset(IEnumerable<UnityObject> objectsToAdd, string path, bool createIfNotExist = false)
        {
            if (objectsToAdd == null) Helper.ThrowArgumentNullException_ObjectToAdd();
            if (path is null) Helper.ThrowArgumentNullException_Path();
            if (path.Length == 0) Helper.ThrowArgumentException_PathCannotBeEmpty();

            foreach (UnityObject objectToAdd in objectsToAdd)
                path = AddObjectToAsset(objectToAdd, path, createIfNotExist);

            return path;
        }

        /// <summary>
        /// Get the asset path of <paramref name="object"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param name="object">Object to get asset path.</param>
        /// <returns>Asset path of object, if any.</returns>
        public static string GetAssetPath(UnityObject @object)
        {
            // TODO: PrefabUtility.GetPrefabInstanceHandle may be useful here

            // Check for 99% of objects
            string path = AssetDatabase.GetAssetPath(@object);

            // Handle GameObjects
            if (string.IsNullOrEmpty(path))
            {
#pragma warning disable UNT0007, UNT0008 // "as" isn't a Unity feature, this is a real null
                // Check if @object is a GameObject or Component of one
                GameObject gameObject = @object as GameObject ?? (@object as Component)?.gameObject;
                // Check if that GameObject is in an scene
                path = gameObject?.scene.path;
#pragma warning restore UNT0007, UNT0008
                if (string.IsNullOrEmpty(path) && gameObject != null)
                {
                    // Check if it's in a prefab file
                    PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                    if (!(prefabStage is null))
                        // Object was in a prefab
#if UNITY_2020_1_OR_NEWER
                        path = prefabStage.assetPath;
#else
                        path = prefabStage.prefabAssetPath;
#endif
                }
            }
            return path;
        }

        /// <summary>
        /// Get the asset path of <paramref name="serializedObject"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedObject="object"><see cref="SerializedObject"/> to get asset path.</param>
        /// <returns>Asset path of object, if any.</returns>
        public static string GetAssetPath(SerializedObject serializedObject)
            => GetAssetPath(serializedObject.targetObject);

        /// <summary>
        /// Get the asset path of <paramref name="serializedProperty"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedProperty="object"><see cref="SerializedProperty"/> to get asset path.</param>
        /// <returns>Asset path of object, if any.</returns>
        public static string GetAssetPath(SerializedProperty serializedProperty)
            => GetAssetPath(serializedProperty.serializedObject);

        /// <summary>
        /// Get the asset directory path of <paramref name="object"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the directory of the file where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param name="object">Object to get asset path.</param>
        /// <returns>Asset directory path of object, if any.</returns>
        public static string GetAssetDirectory(UnityObject @object)
            => Path.GetDirectoryName(GetAssetPath(@object));

        /// <summary>
        /// Get the asset path of <paramref name="serializedObject"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the directory of the file where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedObject="object"><see cref="SerializedObject"/> to get asset path.</param>
        /// <returns>Asset directory path of object, if any.</returns>
        public static string GetAssetDirectory(SerializedObject serializedObject)
            => Path.GetDirectoryName(GetAssetPath(serializedObject.targetObject));

        /// <summary>
        /// Get the asset path of <paramref name="serializedProperty"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the directory of the file where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedProperty="object"><see cref="SerializedProperty"/> to get asset path.</param>
        /// <returns>Asset directory path of object, if any.</returns>
        public static string GetAssetDirectory(SerializedProperty serializedProperty)
            => Path.GetDirectoryName(GetAssetPath(serializedProperty.serializedObject));

        /// <summary>
        /// Get the asset file name of <paramref name="object"/>.<br/>
        /// For <see cref="GameObject"/>s it does return file name where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param name="object">Object to get asset path.</param>
        /// <returns>Asset file name of object, if any.</returns>
        public static string GetAssetFileName(UnityObject @object)
            => Path.GetFileName(GetAssetPath(@object));

        /// <summary>
        /// Get the asset file name of <paramref name="serializedObject"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file name where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedObject="object"><see cref="SerializedObject"/> to get asset path.</param>
        /// <returns>Asset file name of object, if any.</returns>
        public static string GetAssetFileName(SerializedObject serializedObject)
            => Path.GetFileName(GetAssetPath(serializedObject.targetObject));

        /// <summary>
        /// Get the asset file name of <paramref name="serializedProperty"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file name where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedProperty="object"><see cref="SerializedProperty"/> to get asset path.</param>
        /// <returns>Asset file name of object, if any.</returns>
        public static string GetAssetFileName(SerializedProperty serializedProperty)
            => Path.GetFileName(GetAssetPath(serializedProperty.serializedObject));

        /// <summary>
        /// Get the asset file name without extension of <paramref name="object"/>.<br/>
        /// For <see cref="GameObject"/>s it does return file name without extension where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param name="object">Object to get asset path.</param>
        /// <returns>Asset file name without extension of object, if any.</returns>
        public static string GetAssetFileNameWithoutExtension(UnityObject @object)
            => Path.GetFileNameWithoutExtension(GetAssetPath(@object));

        /// <summary>
        /// Get the asset file name without extension of <paramref name="serializedObject"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file name without extension where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedObject="object"><see cref="SerializedObject"/> to get asset path.</param>
        /// <returns>Asset file name without extension of object, if any.</returns>
        public static string GetAssetFileNameWithoutExtension(SerializedObject serializedObject)
            => Path.GetFileNameWithoutExtension(GetAssetPath(serializedObject.targetObject));

        /// <summary>
        /// Get the asset file name without extension of <paramref name="serializedProperty"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file name without extension where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedProperty="object"><see cref="SerializedProperty"/> to get asset path.</param>
        /// <returns>Asset file name without extension of object, if any.</returns>
        public static string GetAssetFileNameWithoutExtension(SerializedProperty serializedProperty)
            => Path.GetFileNameWithoutExtension(GetAssetPath(serializedProperty.serializedObject));

        /// <summary>
        /// Get the asset file extension of <paramref name="object"/>.<br/>
        /// For <see cref="GameObject"/>s it does return file extension where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param name="object">Object to get asset path.</param>
        /// <returns>Asset file extension of object, if any.</returns>
        public static string GetAssetExtension(UnityObject @object)
            => Path.GetExtension(GetAssetPath(@object));

        /// <summary>
        /// Get the asset file extension of <paramref name="serializedObject"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file extension where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedObject="object"><see cref="SerializedObject"/> to get asset path.</param>
        /// <returns>Asset file extension of object, if any.</returns>
        public static string GetAssetExtension(SerializedObject serializedObject)
            => Path.GetExtension(GetAssetPath(serializedObject.targetObject));

        /// <summary>
        /// Get the asset file extension of <paramref name="serializedProperty"/>.<br/>
        /// For <see cref="GameObject"/>s it does return the file extension where it's being save, which can be an scene or prefab file.
        /// </summary>
        /// <param serializedProperty="object"><see cref="SerializedProperty"/> to get asset path.</param>
        /// <returns>Asset file extension of object, if any.</returns>
        public static string GetAssetExtension(SerializedProperty serializedProperty)
            => Path.GetExtension(GetAssetPath(serializedProperty.serializedObject));

        /// <summary>
        /// Extract a sub asset from an asset file to <paramref name="newPath"/>.
        /// </summary>
        /// <param name="subAsset">Sub asset to extract. Can't be main asset.</param>
        /// <param name="newPath">Path to new asset file.</param>
        /// <returns>New sub asset. <see langword="null"/> if <paramref name="subAsset"/> was a main asset.</returns>
        public static UnityObject ExtractSubAsset(UnityObject subAsset, string newPath)
        {
            if (subAsset == null) Helper.ThrowArgumentNullException_SubAsset();
            if (newPath is null) Helper.ThrowArgumentNullException_NewPath();
            if (newPath.Length == 0) Helper.ThrowArgumentException_NewPathCannotBeEmpty();

            string path = AssetDatabase.GetAssetPath(subAsset);
            if (AssetDatabase.LoadMainAssetAtPath(path) != subAsset)
            {
                UnityObject newAsset = UnityObject.Instantiate(subAsset);
                AssetDatabase.RemoveObjectFromAsset(subAsset);
                AssetDatabase.CreateAsset(newAsset, newPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return newAsset;
            }
            return null;
        }

        /// <summary>
        /// Extract a sub asset from an asset file to <paramref name="newPath"/>.<br/>
        /// </summary>
        /// <param name="subAsset">Sub asset to extract. Can't be main asset, otherwise <paramref name="subAsset"/> becomes <see langword="null"/>.</param>
        /// <param name="newPath">Path to new asset file.</param>
        public static void ExtractSubAsset(ref UnityObject subAsset, string newPath)
        {
            if (subAsset == null) Helper.ThrowArgumentNullException_SubAsset();
            if (newPath is null) Helper.ThrowArgumentNullException_NewPath();
            if (newPath.Length == 0) Helper.ThrowArgumentException_NewPathCannotBeEmpty();

            subAsset = ExtractSubAsset(subAsset, newPath);
        }

        /// <summary>
        /// Extract a sub asset from an asset file.<br/>
        /// </summary>
        /// <param name="subAsset">Sub asset to extract. Can't be main asset, otherwise <paramref name="subAsset"/> becomes <see langword="null"/>.</param>
        /// <returns>New sub asset path, if fail this path is invalid.</returns>
        public static string ExtractSubAsset(ref UnityObject subAsset)
        {
            if (subAsset == null) Helper.ThrowArgumentNullException_SubAsset();

            string path = $"{string.Concat(AssetDatabase.GetAssetPath(subAsset).Split('.').Reverse().Skip(1).Reverse())} {subAsset.name}.asset";
            ExtractSubAsset(ref subAsset, path);
            return path;
        }

        /// <summary>
        /// Replaces the last section of the <see cref="string"/> <paramref name="source"/> delimited by '.' with <paramref name="extension"/>.
        /// </summary>
        /// <param name="source">Base <see cref="string"/></param>
        /// <param name="extension">New ending <see cref="string"/></param>
        /// <returns><paramref name="source"/> with a <paramref name="extension"/> as replacement of its last '.' segment.</returns>
        public static string WithDifferentExtension(this string source, string extension)
        {
            if (source is null) Helper.ThrowArgumentNullException_Source();
            if (extension is null) Helper.ThrowArgumentNullException_Extension();
            if (source.Length == 0) Helper.ThrowArgumentException_SourceCannotBeEmpty();
            if (extension.Length == 0) Helper.ThrowArgumentException_ExtensionCannotBeEmpty();

            string[] parts = source.Split(Helper.DOT_SEPARATOR);
            if (parts.Length == 1)
                Throw();
            parts[parts.Length - 1] = extension;
            return string.Join(".", parts);

            void Throw() => throw new ArgumentException("Must have at least one '.'.", nameof(source));
        }
    }
}