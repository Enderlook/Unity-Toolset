using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(PropertyAttribute), true)]
    [CustomPropertyDrawer(typeof(object), true)]
    internal sealed class MasterStackablePropertyDrawer : PropertyDrawer
    {
        // Inspired in https://github.com/j1930021/Stackable-Decorator.

        private static readonly Comparison<StackablePropertyDrawer> ORDER_SELECTOR = (a, b) => (a.Attribute?.order ?? 0).CompareTo(b.Attribute?.order ?? 0);
        private static readonly Dictionary<Type, (Type Drawer, bool UseForChildren)> drawersMap = new Dictionary<Type, (Type Drawer, bool UseForChildren)>();
        private static BackgroundTask task;

        private List<StackablePropertyDrawer> Drawers;
        private StackablePropertyDrawer main;

        [DidReloadScripts]
        private static void Reset()
        {
            task = BackgroundTask.Enqueue(
#if UNITY_2020_1_OR_NEWER
                _ => Progress.Start($"Initialize {typeof(MasterStackablePropertyDrawer)}", "Enqueued process..."),
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
                    drawersMap.Clear();

                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
#if UNITY_2020_1_OR_NEWER
                    int total = 0;
                    foreach (Assembly assembly in assemblies)
                        total += assembly.GetTypes().Length;
                    Progress.Report(id, 0, total);

                    int current = 0;
#endif
                    foreach (Assembly assembly in assemblies)
                    {
                        if (token.IsCancellationRequested)
                            goto cancelled;

                        foreach (Type type in assembly.GetTypes())
                        {
                            if (token.IsCancellationRequested)
                                goto cancelled;

#if UNITY_2020_1_OR_NEWER
                            Progress.Report(id, current++, total);
#endif

                            foreach (CustomStackablePropertyDrawerAttribute attribute in type.GetCustomAttributes<CustomStackablePropertyDrawerAttribute>())
                                drawersMap.Add(attribute.Type, (type, attribute.UseForChildren));
                        }
                    }

#if UNITY_2020_1_OR_NEWER
                    Progress.Finish(id);
                    return;
                cancelled:
                    Progress.Finish(id, Progress.Status.Canceled);
#else
                cancelled:;
#endif
                }
            );
        }

        private List<StackablePropertyDrawer> GetDrawers()
        {
            List<StackablePropertyDrawer> drawers = Drawers;
            if (!(drawers is null))
                return drawers;

            task.EnsureExecute();

            if (drawersMap is null)
                Reset();

            List<StackablePropertyDrawer> list = new List<StackablePropertyDrawer>();

            FieldInfo fieldInfo = this.fieldInfo;
            Type fieldType = fieldInfo.FieldType;
            Type propertyType = fieldType;
            if (!propertyType.IsArrayOrList(out propertyType))
                propertyType = fieldType;

            Check(propertyType);

            if (propertyType.IsGenericType)
                Check(propertyType.GetGenericTypeDefinition());

            foreach (PropertyAttribute attribute in fieldInfo.GetCustomAttributes<PropertyAttribute>(true))
            {
                Type attributeType = attribute.GetType();
                if (drawersMap.TryGetValue(attributeType, out (Type Drawer, bool UseForChildren) tuple) && !(tuple.Drawer is null))
                    WithAttribute(attribute, tuple);
                else
                {
                    Type type = attributeType;
                start:
                    type = type.BaseType;
                    if (type is null)
                        continue;
                    if (drawersMap.TryGetValue(type, out tuple))
                    {
                        if (!(tuple.Drawer is null) && tuple.UseForChildren)
                        {
                            drawersMap.Add(attributeType, (tuple.Drawer, true));
                            WithAttribute(attribute, tuple);
                        }
                        else
                            drawersMap.Add(attributeType, default);
                    }
                    else
                        goto start;
                }
            }

            foreach (PropertyAttribute attribute in propertyType.GetCustomAttributes<PropertyAttribute>(true))
            {
                Type attributeType = attribute.GetType();
                if (drawersMap.TryGetValue(attributeType, out (Type Drawer, bool UseForChildren) tuple) && !(tuple.Drawer is null))
                    WithAttribute(attribute, tuple);
                else
                {
                    Type type = attributeType;
                start:
                    type = type.BaseType;
                    if (type is null)
                        continue;
                    if (drawersMap.TryGetValue(type, out tuple))
                    {
                        if (!(tuple.Drawer is null) && tuple.UseForChildren)
                        {
                            drawersMap.Add(attributeType, (tuple.Drawer, true));
                            WithAttribute(attribute, tuple);
                        }
                        else
                            drawersMap.Add(attributeType, default);
                    }
                    else
                        goto start;
                }
            }

            list.Sort(ORDER_SELECTOR);

            int i = 0;
            int count = list.Count;
            for (; i < count; i++)
            {
                StackablePropertyDrawer drawer = list[i];
                if (drawer.RequestMain)
                {
                    drawer.IsMain(true);
                    main = drawer;
                    break;
                }
            }
            for (; i < count; i++)
            {
                StackablePropertyDrawer drawer = list[i];
                if (drawer.RequestMain)
                    drawer.IsMain(false);
            }

            Drawers = list;
            return list;

            void Check(Type propertyType_)
            {
                if (drawersMap.TryGetValue(propertyType_, out (Type Drawer, bool UseForChildren) tuple) && !(tuple.Drawer is null))
                    WithoutAttribute(tuple);
                else
                {
                    Type type = propertyType_;
                start:
                    type = type.BaseType;
                    if (!(type is null))
                    {
                        if (drawersMap.TryGetValue(type, out tuple))
                        {
                            if (!(tuple.Drawer is null) && tuple.UseForChildren)
                            {
                                drawersMap.Add(propertyType_, (tuple.Drawer, true));
                                WithoutAttribute(tuple);
                            }
                            else
                                drawersMap.Add(propertyType_, default);
                        }
                        else
                            goto start;
                    }
                }
            }

            void WithAttribute(PropertyAttribute attribute, (Type Drawer, bool UseForChildren) tuple_)
            {
                StackablePropertyDrawer drawer = (StackablePropertyDrawer)Activator.CreateInstance(tuple_.Drawer);
                drawer.Attribute = attribute;
                drawer.FieldInfo = fieldInfo;
                list.Add(drawer);
            }

            void WithoutAttribute((Type Drawer, bool UseForChildren) tuple_)
            {
                StackablePropertyDrawer drawer = (StackablePropertyDrawer)Activator.CreateInstance(tuple_.Drawer);
                drawer.FieldInfo = fieldInfo;
                list.Add(drawer);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            List<StackablePropertyDrawer> drawers = GetDrawers();

            bool includeChildren = true;
            bool visible = true;
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
                drawers[i].BeforeOnGUI(ref position, ref property, ref label, ref includeChildren, ref visible);

            if (visible)
            {
                if (main is null)
                    EditorGUI.PropertyField(position, property, label, includeChildren);
                else
                    main.OnGUI(position, property, label, includeChildren);
            }

            for (int i = count - 1; i >= 0; i--)
                drawers[i].AfterOnGUI(position, property, label, includeChildren, visible);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            List<StackablePropertyDrawer> drawers = GetDrawers();

            bool includeChildren = true;
            bool visible = true;
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
                drawers[i].BeforeGetPropertyHeight(ref property, ref label, ref includeChildren, ref visible);

            float height = 0;
            if (visible)
            {
                if (main is null)
                    height = EditorGUI.GetPropertyHeight(property, label, includeChildren);
                else
                    height = main.GetPropertyHeight(property, label, includeChildren);
            }

            for (int i = count - 1; i >= 0; i--)
                height = drawers[i].AfterGetPropertyHeight(property, label, includeChildren, height);

            if (height == 0)
                height = -EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            foreach (StackablePropertyDrawer drawer in GetDrawers())
                if (!drawer.CanCacheInspectorGUI(property))
                    return false;
            return true;
        }
    }
}