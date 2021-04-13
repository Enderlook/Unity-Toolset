using Enderlook.Enumerables;
using Enderlook.Reflection;
using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UnityEditor;

using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Windows
{
    internal class ScriptableObjectWindow : EditorWindow
    {
        private static readonly GUIContent CONTEXT_PROPERTY_MENU = new GUIContent("Scriptable Object Menu", "Open the Scriptable Object Menu.");
        private static readonly GUIContent TITLE_CONTENT = new GUIContent("Scriptable Object Manager");
        private static readonly GUIContent INSTANTIATE_TYPE_CONTENT = new GUIContent("Instance type", "Scriptable object instance type to create.");
        private static readonly GUIContent ADD_TO_ASSET_CONTENT = new GUIContent("Instantiate in field and add to asset", "Create and instance and assign to field. The scriptable object will be added to the asset or prefab file.");
        private static readonly GUIContent ADD_TO_SCENE_CONTENT = new GUIContent("Instantiate in field and add to scene", "Create and instance and assign to field. The scriptable object will be added to the scene file.");
        private static readonly GUIContent PATH_TO_FILE_CONTENT = new GUIContent("Path to file", "Path where the asset file is stored or will be saved.");
        private static readonly GUIContent SAVE_ASSET_CONTENT = new GUIContent("Instantiate in field and save asset", "Create and instance, assign to field and save it as an asset file.");
        private static readonly GUIContent CLEAN_FIELD = new GUIContent("Clean field", "Remove current instance of field.");
        private static readonly string[] EXTENSIONS = new string[] { ".asset", ".prefab", ".scene" };

        private const string DEFAULT_PATH = "Resources/";

        private static readonly Type root = typeof(UnityObject); // We don't use ScriptableObjet so this can work with RestrictTypeCheckingAttribute

        // Pool values
        private static readonly List<Type> tmpList = new List<Type>();
        private static readonly List<Type> tmpList2 = new List<Type>();
        private static readonly Stack<Type> tmpStack = new Stack<Type>();
        private static readonly char[] splitBy = new[] { '/' };
        private static readonly Func<Type, string> GetName = (Type type) => type.Name;

        private static ILookup<Type, Type> derivedTypes;

        private List<Type> allowedTypes;
        private string[] allowedTypesNames;
        private int index;

        private string path = DEFAULT_PATH;
        private string scriptableObjectName;
        private string propertyPath;
        private bool scriptableObjectNameAuto = true;

        private HideFlags scriptableObjectHideFlags;

        private ScriptableObject oldScriptableObject;

        private SerializedProperty property;
        private Accessors accessors;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        [InitializeOnLoadMethod]
        private static void AddContextualPropertyMenu()
        {
            ContextualPropertyMenu.contextualPropertyMenu += (GenericMenu menu, SerializedProperty property) =>
            {
                if (property.propertyPath.EndsWith(".Array.Size"))
                    return;

                FieldInfo fieldInfo = property.GetFieldInfo();
                if (fieldInfo is null)
                    return;

                Type fieldType = fieldInfo.FieldType;
                if (fieldType.IsArrayOrList())
                    fieldType = fieldType.GetElementTypeOfArrayOrList();

                if (typeof(UnityObject).IsAssignableFrom(fieldType))
                    menu.AddItem(
                        CONTEXT_PROPERTY_MENU,
                        false,
                        () => CreateWindow(property, fieldInfo)
                    );
            };
        }

        private static void InitializeDerivedTypes()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Exception[][] exceptions = new Exception[assemblies.Length][];
            HashSet<KeyValuePair<Type, Type>>[] sets = new HashSet<KeyValuePair<Type, Type>>[assemblies.Length];

            // By using multithreading we can speed up large workflows around a 60% from 5.5s to 3.5s.
            // However, we can't use a ConcurrentBag to stores keys because that slowdown the code in a factor of x3 (from 5.5s to 17.2s).

            Parallel.For(0, assemblies.Length, (int i) =>
            {
                Assembly assembly = assemblies[i];
                if (!assembly.TryGetTypes(out IEnumerable<Type> loadedTypes, out Exception[] exceptions_))
                    exceptions[i] = exceptions_;
                else
                    exceptions[i] = Array.Empty<Exception>();

                Stack<Type> stack = new Stack<Type>();
                foreach (Type type in loadedTypes)
                    if (root.IsAssignableFrom(type))
                        stack.Push(type);

                HashSet<KeyValuePair<Type, Type>> set = new HashSet<KeyValuePair<Type, Type>>();
                while (stack.TryPop(out Type result))
                {
                    set.Add(new KeyValuePair<Type, Type>(result, result));
                    Type baseType = result.BaseType;
                    if (root.IsAssignableFrom(baseType))
                    {
                        KeyValuePair<Type, Type> item = new KeyValuePair<Type, Type>(baseType, result);
                        if (!set.Contains(item))
                        {
                            set.Add(item);
                            stack.Push(baseType);
                        }
                    }
                }
                sets[i] = set;
            });

            bool hasErrors = false;
            for (int i = 0; i < exceptions.Length; i++)
            {
                Exception[] exceptions_ = exceptions[i];
                Assembly assembly = assemblies[i];
                if (exceptions_.Length > 0)
                {
                    if (!hasErrors)
                    {
                        hasErrors = true;
                        Debug.LogError("While getting Types from loaded assemblies in Scriptable Object Window the following exceptions occurred:");
                    }

                    foreach (Exception exception in exceptions_)
                        Debug.LogWarning($"{assembly.FullName}: {exception.Message}.");
                }
            }

            if (sets.Length == 0)
                derivedTypes = Array.Empty<KeyValuePair<Type, Type>>().ToLookup();
            else
            {
                HashSet<KeyValuePair<Type, Type>> keys = sets[0];

                for (int i = 1; i < sets.Length; i++)
                    keys.UnionWith(sets[i]);

                derivedTypes = keys.ToLookup();
            }
        }

        private static IEnumerable<Type> GetDerivedTypes(Type type)
        {
            Debug.Assert(tmpStack.Count == 0);
            foreach (Type t in derivedTypes[type])
                if (t != type)
                    tmpStack.Push(t);

            tmpList.Clear();
            tmpList.Add(type);
            tmpList.AddRange(tmpStack);

            while (tmpStack.TryPop(out Type result))
            {
                foreach (Type t in derivedTypes[result])
                {
                    if (t != result)
                    {
                        tmpStack.Push(t);
                        tmpList.Add(t);
                    }
                }
            }

            return tmpList.OrderBy(GetName);
        }

        private static void CreateWindow(SerializedProperty property, FieldInfo fieldInfo)
        {
            if (derivedTypes == null)
                InitializeDerivedTypes();

            ScriptableObjectWindow window = GetWindow<ScriptableObjectWindow>();

            window.propertyPath = AssetDatabaseHelper.GetAssetPath(property);

            window.property = property;
            window.accessors = property.GetTargetObjectAccessors();

            Debug.Assert(tmpList2.Count == 0);
            foreach (Type type in GetDerivedTypes(window.property.GetValueType()))
                if (!type.IsAbstract)
                    tmpList2.Add(type);

            // RestrictTypeAttribute compatibility
            RestrictTypeAttribute restrictTypeAttribute = fieldInfo.GetCustomAttribute<RestrictTypeAttribute>();
            List<Type> allowedTypes;
            if (restrictTypeAttribute != null)
            {
                allowedTypes = new List<Type>();
                foreach (Type type in tmpList2)
                    if (restrictTypeAttribute.CheckIfTypeIsAllowed(type))
                        allowedTypes.Add(type);
            }
            else
                allowedTypes = new List<Type>(tmpList2);

            tmpList2.Clear();

            window.allowedTypes = allowedTypes;

            window.allowedTypesNames = new string[allowedTypes.Count];

            for (int i = 0; i < allowedTypes.Count; i++)
                window.allowedTypesNames[i] = allowedTypes[i].Name;

            window.index = window.GetIndex(window.property.GetValueType());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnGUI()
        {
            titleContent = TITLE_CONTENT;

            ScriptableObject scriptableObject = (ScriptableObject)accessors.Get();
            bool hasScriptableObject = scriptableObject != null;

            if (oldScriptableObject != scriptableObject)
            {
                oldScriptableObject = scriptableObject;

                if (hasScriptableObject)
                {
                    index = GetIndex(scriptableObject.GetType());
                    scriptableObjectHideFlags = scriptableObject.hideFlags;
                    scriptableObjectName = scriptableObject.name;
                }
            }

            // Instance Type
            EditorGUI.BeginDisabledGroup(hasScriptableObject);
            index = EditorGUILayout.Popup(INSTANTIATE_TYPE_CONTENT, index, allowedTypesNames);
            EditorGUI.EndDisabledGroup();

            // Get Name
            if (scriptableObjectNameAuto && !hasScriptableObject)
                scriptableObjectName = path.Split(splitBy).Last().Split(EXTENSIONS, StringSplitOptions.None).First();

            UnityObject targetObject = property.serializedObject.targetObject;

            if (hasScriptableObject)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Name of Scriptable Object", scriptableObject.name);
                EditorGUI.EndDisabledGroup();
                scriptableObjectName = EditorGUILayout.TextField("New name", scriptableObjectName);
                scriptableObjectHideFlags = (HideFlags)EditorGUILayout.EnumFlagsField("New Hide Flags", scriptableObjectHideFlags);

                if (GUILayout.Button("Rename Scriptable Object"))
                {
                    scriptableObject.name = scriptableObjectName;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                string old = scriptableObjectName;
                scriptableObjectName = EditorGUILayout.TextField("Name of Scriptable Object", scriptableObjectName);
                scriptableObjectNameAuto = scriptableObjectNameAuto && scriptableObjectName == old;
            }

            if (!hasScriptableObject)
            {
                // Create
                EditorGUI.BeginDisabledGroup(index == -1 || string.IsNullOrEmpty(scriptableObjectName));
                if (GUILayout.Button(ADD_TO_ASSET_CONTENT))
                {
                    scriptableObject = InstantiateAndApply(targetObject, scriptableObjectName);
                    AssetDatabase.AddObjectToAsset(scriptableObject, propertyPath);
                    AssetDatabase.Refresh();
                }
                if (GUILayout.Button(ADD_TO_SCENE_CONTENT))
                {
                    scriptableObject = InstantiateAndApply(targetObject, scriptableObjectName);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();

            // Get path to file
            EditorGUI.BeginDisabledGroup(hasScriptableObject);
            string pathToAsset = AssetDatabase.GetAssetPath(scriptableObject);
            bool hasAsset = !string.IsNullOrEmpty(pathToAsset);
            path = hasAsset ? pathToAsset : path;
            path = EditorGUILayout.TextField(PATH_TO_FILE_CONTENT, path);
            string _path = path.StartsWith("Assets/") ? path : "Assets/" + path;
            _path = _path.EndsWith(".asset") ? _path : _path + ".asset";
            if (!hasAsset)
                EditorGUILayout.LabelField("Path to save:", _path);
            EditorGUI.EndDisabledGroup();

            // Create file
            if (!hasScriptableObject)
            {
                EditorGUI.BeginDisabledGroup(index == -1);
                if (GUILayout.Button(SAVE_ASSET_CONTENT))
                {
                    scriptableObject = InstantiateAndApply(targetObject, scriptableObjectName);
                    AssetDatabaseHelper.CreateAsset(scriptableObject, _path);
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                // Clean
                if (GUILayout.Button(CLEAN_FIELD))
                {
                    Undo.RecordObject(targetObject, "Clean field");
                    accessors.Set(null);
                    path = DEFAULT_PATH;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private ScriptableObject InstantiateAndApply(UnityObject targetObject, string name)
        {
            ScriptableObject scriptableObject;
            Undo.RecordObject(targetObject, "Instantiate field");
            scriptableObject = CreateInstance(allowedTypes[index]);
            scriptableObject.name = name;
            accessors.Set(scriptableObject);
            property.serializedObject.ApplyModifiedProperties();
            return scriptableObject;
        }

        private int GetIndex(Type type) => allowedTypes.IndexOf(type);
    }
}
