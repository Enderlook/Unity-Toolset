using Enderlook.Unity.Toolset.Attributes;
using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using UnityObject = UnityEngine.Object;

namespace Enderlook.Unity.Toolset.Windows
{
    internal sealed partial class ObjectWindow : EditorWindow
    {
        private static readonly GUIContent CONTEXT_PROPERTY_MENU = new GUIContent("Object Menu", "Open the Object Menu");
        private static readonly GUIContent TITLE_CONTENT = new GUIContent("Object Menu");
        private const string EDIT_POPUP = "Edit";
        private const string SELECT_POPUP = "Select";
        private const string CREATE_POPUP = "Create";
        private static readonly List<string> POPUP_OPTIONS = new List<string>() { EDIT_POPUP, SELECT_POPUP, CREATE_POPUP };
        private static readonly List<HideFlags> HIDE_FLAGS = new List<HideFlags>((HideFlags[])Enum.GetValues(typeof(HideFlags)));
        private static readonly char[] SPLIT = new char[] { '/' };
        private static readonly Comparison<Type> COMPARE_TYPES = (a, b) => a.FullName.CompareTo(b.FullName);
        private static readonly Func<KeyValuePair<Type, Type>, Type> KEY_SELECTOR = e => e.Key;
        private static readonly Func<KeyValuePair<Type, Type>, Type> ELEMENT_SELECTOR = e => e.Value;
        private static readonly Func<VisualElement> CREATE_LABEL = () => new Label();

        // Pool values
        private static Stack<Type> tmpStack;
        private static List<Type> tmpList;

        private static ILookup<Type, Type> derivedTypes;
        private static BackgroundTask derivedTypesTask;

        private SerializedProperty property;
        private RestrictTypeAttribute restrictTypeAttribute;

        private string propertyPath;
        private readonly List<Type> allowedTypesToInstantiate = new List<Type>();

        private readonly List<UnityObject> elements = new List<UnityObject>();
        private UnityObject original;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        [InitializeOnLoadMethod]
        private static void AddContextualPropertyMenu()
        {
            EditorApplication.contextualPropertyMenu += (GenericMenu menu, SerializedProperty property) =>
            {
                if (property.IsArrayOrListSize() || !property.TryGetMemberInfo(out MemberInfo memberInfo))
                    return;

                if (typeof(UnityObject).IsAssignableFrom(property.GetPropertyType()))
                {
                    menu.AddItem(
                        CONTEXT_PROPERTY_MENU,
                        false,
                        () => CreateWindow(property, memberInfo)
                    );
                }
            };
        }

        [DidReloadScripts]
        private static void InitializeDerivedTypes()
        {
            derivedTypesTask = BackgroundTask.Enqueue(
#if UNITY_2020_1_OR_NEWER
                token => Progress.Start("Object Window Initialization", "Enqueued process..."),
                (id, token) =>
#else
                token =>
#endif
                {
                    if (token.IsCancellationRequested)
                        goto cancelled;

#if UNITY_2020_1_OR_NEWER
                    Progress.SetDescription(id, null);
#endif

                    derivedTypes = null;

                    Type root = typeof(UnityObject);

                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
#if UNITY_2020_1_OR_NEWER
                    int total = 0;
                    foreach (Assembly assembly in assemblies)
                        total += assembly.GetTypes().Length;
                    Progress.Report(id, 0, total);

#endif
                    StrongBox<(int Current, bool HasErrors)> box = new StrongBox<(int Current, bool HasErrors)>();
                    // TODO: On .Net Standard 2.1 use initialCapacity.
                    HashSet<KeyValuePair<Type, Type>> result = new HashSet<KeyValuePair<Type, Type>>();

                    // By using multithreading we can speed up large workflows around a 60% from 5.5s to 3.5s.
                    // However, we can't use a ConcurrentBag to stores keys because that slowdown the code in a factor of x3 (from 5.5s to 17.2s).

                    Parallel.For(0, assemblies.Length, (int i) =>
                    {
                        Assembly assembly = assemblies[i];
                        IEnumerable<Type> loadedTypes;
                        try
                        {
                            loadedTypes = assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException exception)
                        {
                            loadedTypes = exception.Types.Where(e => e != null);

                            lock (box)
                            {
                                if (!box.Value.HasErrors)
                                {
                                    box.Value.HasErrors = true;
                                    Debug.LogError($"While getting Types from loaded assemblies in {nameof(ObjectWindow)} the following exceptions occurred:");
                                }
                            }

                            foreach (Exception e in exception.LoaderExceptions)
                                Debug.LogError($"{assembly.FullName}: {e.Message}.");
                        }

                        Stack<Type> stack = new Stack<Type>();
                        // TODO: On .Net Standard 2.1 use initialCapacity.
                        HashSet<KeyValuePair<Type, Type>> set = new HashSet<KeyValuePair<Type, Type>>();
                        foreach (Type type in loadedTypes)
                        {
#if UNITY_2020_1_OR_NEWER
                            Progress.Report(id, Interlocked.Increment(ref box.Value.Current), total);
#endif
                            if (root.IsAssignableFrom(type))
                            {
                                Type result_ = type;

                                while (true)
                                {
                                    Type baseType = result_.BaseType;
                                    if (root.IsAssignableFrom(baseType))
                                    {
                                        KeyValuePair<Type, Type> item = new KeyValuePair<Type, Type>(baseType, result_);
                                        if (!set.Contains(item))
                                        {
                                            set.Add(item);
                                            stack.Push(baseType);
                                        }
                                    }

                                    // TODO: In .Net Standard 2.1 use .TryPop.
                                    if (stack.Count == 0)
                                        break;
                                    result_ = stack.Pop();
                                }
                            }
                        }

                        lock (result)
                            result.UnionWith(set);
                    });

                    derivedTypes = result.ToLookup(KEY_SELECTOR, ELEMENT_SELECTOR);

#if UNITY_2020_1_OR_NEWER
                    Progress.Finish(id);
                    return;
                cancelled:
                    Progress.Finish(id, Progress.Status.Canceled);
#else
                cancelled:;
#endif
                });
        }

        private static void CreateWindow(SerializedProperty property, MemberInfo memberInfo)
        {
            derivedTypesTask.EnsureExecute();

            ObjectWindow window = GetWindow<ObjectWindow>();
            window.titleContent = TITLE_CONTENT;

            window.property = property;
            window.original = property.GetValue<UnityObject>();
            window.restrictTypeAttribute = memberInfo.GetCustomAttribute<RestrictTypeAttribute>();
            window.propertyPath = AssetDatabaseHelper.GetAssetPath(property);
            window.SetAllowedTypesToInstantiate();
        }

        private void RefeshObjects(bool gatherFromAssets, ListView list)
        {
            object selected = list.selectedItem;

            IEnumerable<UnityObject> found;
            if (gatherFromAssets)
                found = Resources.FindObjectsOfTypeAll(property.GetPropertyType());
            else
                found = FindObjectsOfType(property.GetPropertyType());

            if (!(restrictTypeAttribute is null))
                found = found.Where(e => restrictTypeAttribute.CheckIfTypeIsAllowed(e.GetType()));

            elements.Clear();
            elements.Add(null);

            if (original != null)
            {
                if (found is UnityObject[] found_)
                {
                    if (Array.IndexOf(found_, original) == -1)
                        elements.Add(original);
                }
                else
                {
                    foreach (UnityObject e in found)
                        if (e != original)
                            goto outside;
                    elements.Add(original);
                    outside:;
                }
            }

            UnityObject current = property.GetValue<UnityObject>();
            if (current != null && elements.IndexOf(current) == -1)
            {
                if (found is UnityObject[] found_)
                {
                    if (Array.IndexOf(found_, current) == -1)
                        elements.Add(current);
                }
                else
                {
                    foreach (UnityObject e in found)
                        if (e != current)
                            goto outside;
                    elements.Add(current);
                    outside:;
                }
            }

            elements.AddRange(found);

            int index = elements.IndexOf((UnityObject)selected);
            if (index == -1)
                index = elements.IndexOf(original);
            if (index == -1)
                index = 0;
            list.selectedIndex = index;
            list.Refresh();
        }

        private void SetAllowedTypesToInstantiate()
        {
            Stack<Type> stack = Interlocked.Exchange(ref tmpStack, null) ?? new Stack<Type>();
            List<Type> list = Interlocked.Exchange(ref tmpList, null) ?? new List<Type>();
            Debug.Assert(stack.Count == 0);
            Debug.Assert(list.Count == 0);
            Debug.Assert(allowedTypesToInstantiate.Count == 0);

            Type propertyType = property.GetPropertyType();

            foreach (Type type in derivedTypes[propertyType])
                stack.Push(type);

            list.Add(propertyType);
            list.AddRange(stack);

            // TODO: In .Net Standard 2.1 use .TryPop.
            while (stack.Count > 0)
            {
                Type result = stack.Pop();
                foreach (Type type in derivedTypes[result])
                {
                    stack.Push(type);
                    list.Add(type);
                }
            }

            if (restrictTypeAttribute is null)
            {
                foreach (Type type in list)
                    if (!type.IsAbstract)
                        allowedTypesToInstantiate.Add(type);
            }
            else
            {
                foreach (Type type in list)
                    if (!type.IsAbstract && restrictTypeAttribute.CheckIfTypeIsAllowed(type))
                        allowedTypesToInstantiate.Add(type);
            }

            Debug.Assert(stack.Count == 0);
            list.Clear();

            tmpStack = stack;
            tmpList = list;

            allowedTypesToInstantiate.Sort(COMPARE_TYPES);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnEnable()
        {
            rootVisualElement.schedule.Execute(() =>
            {
                // Prevent error when inspector is closed but window is left open.
                if (property is null)
                {
                    Close();
                    return;
                }

                elements.Add(null);
                UnityObject current = property.GetValue<UnityObject>();
                if (current != null)
                    elements.Add(current);

                ScrollView scroll = new ScrollView();
                {
                    scroll.style.flexGrow = 1;

                    VisualElement middleContent = DrawMiddle();
                    scroll.Add(middleContent);
                }
                rootVisualElement.Add(scroll);
            });
        }

        private VisualElement DrawMiddle()
        {
            VisualElement middleContent = new VisualElement();
            {
                middleContent.style.flexGrow = 1;

                PopupField<string> middleTab = new PopupField<string>("Panel", POPUP_OPTIONS, EDIT_POPUP);
                {
                    middleTab.SetEnabled(allowedTypesToInstantiate.Count > 0);
                }
                middleContent.Add(middleTab);

                Field propertyField = new Field("", property);
                {
                    propertyField.style.display = DisplayStyle.Flex;
                }

                middleContent.Add(propertyField);

                VisualElement pickerContent = DrawPickerContent(propertyField);
                {
                    pickerContent.style.display = DisplayStyle.None;
                }
                middleContent.Add(pickerContent);

                VisualElement createContent = DrawCreateContent();
                {
                    createContent.style.display = DisplayStyle.None;
                }
                middleContent.Add(createContent);

                middleTab.RegisterValueChangedCallback(e =>
                {
                    switch (e.newValue)
                    {
                        case EDIT_POPUP:
                            propertyField.style.display = DisplayStyle.Flex;
                            pickerContent.style.display = DisplayStyle.None;
                            createContent.style.display = DisplayStyle.None;
                            break;
                        case SELECT_POPUP:
                            propertyField.style.display = DisplayStyle.None;
                            pickerContent.style.display = DisplayStyle.Flex;
                            createContent.style.display = DisplayStyle.None;
                            break;
                        case CREATE_POPUP:
                            propertyField.style.display = DisplayStyle.None;
                            pickerContent.style.display = DisplayStyle.None;
                            createContent.style.display = DisplayStyle.Flex;
                            break;
                    }
                });
            }

            return middleContent;
        }

        private VisualElement DrawCreateContent()
        {
            VisualElement createContent = new VisualElement();
            {
                createContent.style.flexGrow = 1;

                TextField nameField = new TextField("Name");
                {
                    nameField.tooltip = "Name of the object to create.";
                }
                createContent.Add(nameField);

                TextField textField = new TextField("Path to file");
                {
                    textField.tooltip = "Determines in which location will the created asset be stored.";
                    textField.value = "Resources/";

                    nameField.RegisterValueChangedCallback(e => {
                        string[] parts = textField.value.Split(SPLIT);
                        if (parts[parts.Length - 1] == e.previousValue)
                            parts[parts.Length - 1] = e.newValue;
                        textField.value = string.Join("/", parts);
                    });

                    textField.RegisterValueChangedCallback(e =>
                    {
                        string[] parts = e.previousValue.Split(SPLIT);
                        if (parts[parts.Length - 1] == nameField.value)
                        {
                            parts = e.newValue.Split(SPLIT);
                            nameField.value = parts[parts.Length - 1];
                        }
                    });
                }
                createContent.Add(textField);

                TextField textFieldOutput = new TextField("Path to save");
                {
                    textFieldOutput.tooltip = "In which location will the created asset be stored.";
                    textFieldOutput.SetEnabled(false);
                    textFieldOutput.value = "Assets/Resources/";

                    textField.RegisterValueChangedCallback(e => textFieldOutput.value = $"Assets/{e.newValue}.asset");
                }
                createContent.Add(textFieldOutput);

                PopupField<HideFlags> hideFlags = new PopupField<HideFlags>("Hide Flags", HIDE_FLAGS, default(HideFlags));
                {
                    hideFlags.tooltip = "Determines the visiblity of this object in the editor.";
                }
                createContent.Add(hideFlags);

                Button saveAssetButton = new Button();
                {
                    saveAssetButton.text = "Instantiate, assign to property and save as asset file";
                    saveAssetButton.tooltip = "Instance, assign to property and save it as an asset file.";
                    saveAssetButton.SetEnabled(false);
                }
                createContent.Add(saveAssetButton);

                Button addToAssetButton = new Button();
                {
                    addToAssetButton.text = "Instantiate, assign to property and add to asset";
                    addToAssetButton.tooltip = "Instance, assign to property and save it in the current asset or prefab fille.";
                    addToAssetButton.SetEnabled(false);
                }
                createContent.Add(addToAssetButton);

                Button addToSceneButton = new Button();
                {
                    addToSceneButton.text = "Instantiate, assign to property and add to scene";
                    addToSceneButton.tooltip = "Instance, assign to property and save it in the current scene fille.";
                    addToSceneButton.SetEnabled(false);
                }
                createContent.Add(addToSceneButton);

                Label label = new Label("Types");
                {
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    label.style.unityTextAlign = TextAnchor.MiddleCenter;
                }
                createContent.Add(label);

                ListView list;
                Box box = new Box();
                {
                    box.style.minHeight = 100;
                    box.style.flexGrow = 1;

                    list = new ListView(allowedTypesToInstantiate, 20, CREATE_LABEL, (e, i) => ((Label)e).text = allowedTypesToInstantiate[i].ToString());
                    {
                        list.selectionType = SelectionType.Single;
                        list.style.flexGrow = 1;
                    }
                    box.Add(list);
                }
                createContent.Add(box);

                textField.RegisterValueChangedCallback(_ => Check(nameField.value));
                nameField.RegisterValueChangedCallback(e => Check(e.newValue));

#if UNITY_2020_1_OR_NEWER
                Action<IEnumerable<object>> callback = _ => Check(nameField.value);
                list.onItemsChosen += callback;
                list.onSelectionChange += callback;
#else
                list.onItemChosen += _ => Check(nameField.value);
                list.onSelectionChanged += _ => Check(nameField.value);
#endif

                void Check(string text)
                {
                    bool value = !string.IsNullOrEmpty(text) && list.selectedItem != null;
                    saveAssetButton.SetEnabled(value);
                    addToAssetButton.SetEnabled(value);
                    addToSceneButton.SetEnabled(value);
                }

                saveAssetButton.clickable.clicked += () =>
                {
                    UnityObject createdObject = InstantiateAndApply((Type)list.selectedItem, property.serializedObject.targetObject, nameField.value, hideFlags.value, saveAssetButton.text);
                    AssetDatabaseHelper.CreateAsset(createdObject, $"Assets/{textField.value}.asset");
                    AssetDatabase.Refresh();
                };

                addToAssetButton.clickable.clicked += () =>
                {
                    UnityObject createdObject = InstantiateAndApply((Type)list.selectedItem, property.serializedObject.targetObject, nameField.value, hideFlags.value, saveAssetButton.text);
                    AssetDatabase.AddObjectToAsset(createdObject, propertyPath);
                    AssetDatabase.Refresh();
                };

                addToSceneButton.clickable.clicked += () => InstantiateAndApply((Type)list.selectedItem, property.serializedObject.targetObject, nameField.value, hideFlags.value, saveAssetButton.text);
            }

            return createContent;
        }

        private VisualElement DrawPickerContent(Field propertyField)
        {
            VisualElement pickerContent = new VisualElement();
            {
                pickerContent.style.flexGrow = 1;

                Button searchInContext, searchInAssets;
                VisualElement buttonsBar = new VisualElement();
                {
                    buttonsBar.style.flexDirection = FlexDirection.Row;

                    searchInContext = new Button();
                    {
                        searchInContext.text = "Search in Context";
                        searchInContext.tooltip = "Search for all references in current context (scene or prefab file) that can be assigned to this field.";
                        searchInContext.style.flexGrow = 1;
                    }
                    buttonsBar.Add(searchInContext);

                    searchInAssets = new Button();
                    {
                        searchInAssets.text = "Search in Assets";
                        searchInAssets.tooltip = "Search for all references in the assets database that can be assigned to this field.";
                        searchInAssets.style.flexGrow = 1;
                    }
                    buttonsBar.Add(searchInAssets);
                }
                pickerContent.Add(buttonsBar);

                Label label = new Label("Values");
                {
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    label.style.unityTextAlign = TextAnchor.MiddleCenter;
                }
                pickerContent.Add(label);

                ListView list;
                Box box = new Box();
                {
                    box.style.minHeight = 100;
                    box.style.flexGrow = 1;

                    list = new ListView(elements, 20, CREATE_LABEL, (e, i) =>
                    {
                        UnityObject element = elements[i];
                        ((Label)e).text = element is null ? "<Null>" : element.ToString();
                    });
                    {
                        list.selectionType = SelectionType.Single;
                        list.style.flexGrow = 1;
                        list.selectedIndex = elements.Count - 1;
#if UNITY_2020_1_OR_NEWER
                        list.onItemsChosen += e =>
                        {
                            property.SetValue((UnityObject)e.First());
                            propertyField.Set(property.GetValue<UnityObject>());
                            property.serializedObject.ApplyModifiedProperties();
                        };
#else
                        list.onItemChosen += e =>
                        {
                            property.SetValue((UnityObject)e);
                            propertyField.Set(property.GetValue<UnityObject>());
                            property.serializedObject.ApplyModifiedProperties();
                        };
#endif
                    }
                    box.Add(list);

                    searchInContext.clickable.clicked += () => RefeshObjects(false, list);
                    searchInAssets.clickable.clicked += () => RefeshObjects(true, list);
                }
                pickerContent.Add(box);

                Field selectedField = new Field("Selected", "Selected");
                {
                    selectedField.style.display = DisplayStyle.None;

#if UNITY_2020_1_OR_NEWER
                    list.onSelectionChange += e =>
                    {
                        UnityObject objectReferenceValue = (UnityObject)e.FirstOrDefault();
                        if (objectReferenceValue is null)
                        {
                            selectedField.Set(objectReferenceValue);
                            selectedField.style.display = DisplayStyle.Flex;
                        }
                        else
                            selectedField.style.display = DisplayStyle.None;
                    };
#else
                    list.onSelectionChanged += e =>
                    {
                        if (e.Count == 0)
                            selectedField.style.display = DisplayStyle.None;
                        else
                        {
                            UnityObject objectReferenceValue = (UnityObject)e[0];
                            selectedField.Set(objectReferenceValue);
                            selectedField.style.display = objectReferenceValue is null ? DisplayStyle.None : DisplayStyle.Flex;
                        }
                    };
#endif
                }
                pickerContent.Add(selectedField);
            }
            return pickerContent;
        }

        private UnityObject InstantiateAndApply(Type type, UnityObject targetObject, string name, HideFlags hideFlag, string undoComment)
        {
            Undo.RecordObject(targetObject, undoComment);
            UnityObject createdObject = CreateInstance(type);
            createdObject.name = name;
            createdObject.hideFlags = hideFlag;
            property.SetValue(createdObject);
            property.serializedObject.ApplyModifiedProperties();
            return createdObject;
        }
    }
}