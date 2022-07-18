using Enderlook.Unity.Toolset.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;

using UnityEngine;

using SystemAssembly = System.Reflection.Assembly;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Enderlook.Unity.Toolset.Drawers
{
    [CustomPropertyDrawer(typeof(PropertyAttribute), true)]
    [CustomPropertyDrawer(typeof(object), true)]
    internal sealed class MasterStackablePropertyDrawer : PropertyDrawer
    {
        // Inspired in https://github.com/j1930021/Stackable-Decorator.

        private static readonly Comparison<StackablePropertyDrawer> orderSelector = (a, b) => (a.Attribute?.order ?? 0).CompareTo(b.Attribute?.order ?? 0);
        private static Dictionary<Type, (Type Drawer, bool UseForChildren)> drawersMap;

        private IReadOnlyList<StackablePropertyDrawer> Drawers;
        private bool hasOnGUI;

        [DidReloadScripts]
        private static void Reset()
        {
            Dictionary<Type, (Type Drawer, bool UseForChildren)> dictionary = Interlocked.Exchange(ref drawersMap, null) ?? new Dictionary<Type, (Type Drawer, bool UseForChildren)>();
            dictionary.Clear();

            UnityAssembly[] unityAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                .Concat(CompilationPipeline.GetAssemblies(AssembliesType.Player))
                .ToArray();

            HashSet<SystemAssembly> assemblies = new HashSet<SystemAssembly>();
            foreach (SystemAssembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                foreach (UnityAssembly unityAssembly in unityAssemblies)
                    if (name == unityAssembly.name)
                        assemblies.Add(assembly);
            }

            foreach (SystemAssembly assembly in assemblies)
                foreach (Type type in assembly.GetTypes())
                    foreach (CustomStackablePropertyDrawerAttribute attribute in type.GetCustomAttributes<CustomStackablePropertyDrawerAttribute>())
                        dictionary.Add(attribute.Type, (type, attribute.UseForChildren));

            drawersMap = dictionary;
        }

        private IReadOnlyList<StackablePropertyDrawer> GetDrawers()
        {
            if (drawersMap is null)
                Reset();

            List<StackablePropertyDrawer> list = new List<StackablePropertyDrawer>();

            FieldInfo fieldInfo = this.fieldInfo;
            Type fieldType = fieldInfo.FieldType;
            Type propertyType = fieldType;
            if (!propertyType.IsArrayOrList(out propertyType))
                propertyType = fieldType;

            if (drawersMap.TryGetValue(propertyType, out (Type Drawer, bool UseForChildren) tuple) && !(tuple.Drawer is null))
                WithoutAttribute(tuple);
            else
            {
                Type type = propertyType;
            start:
                type = type.BaseType;
                if (!(type is null))
                {
                    if (drawersMap.TryGetValue(type, out tuple))
                    {
                        if (!(tuple.Drawer is null) && tuple.UseForChildren)
                        {
                            drawersMap.Add(propertyType, (tuple.Drawer, true));
                            WithoutAttribute(tuple);
                        }
                        else
                            drawersMap.Add(propertyType, default);
                    }
                    else
                        goto start;
                }
            }

            foreach (PropertyAttribute attribute in fieldInfo.GetCustomAttributes<PropertyAttribute>(true))
            {
                Type attributeType = attribute.GetType();
                if (drawersMap.TryGetValue(attributeType, out tuple) && !(tuple.Drawer is null))
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
                if (drawersMap.TryGetValue(attributeType, out tuple) && !(tuple.Drawer is null))
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

            list.Sort(orderSelector);
            return list.ToArray();

            void WithAttribute(PropertyAttribute attribute, (Type Drawer, bool UseForChildren) tuple)
            {
                StackablePropertyDrawer drawer = (StackablePropertyDrawer)Activator.CreateInstance(tuple.Drawer);
                drawer.Attribute = attribute;
                drawer.FieldInfo = fieldInfo;
                hasOnGUI |= drawer.HasOnGUI;
                list.Add(drawer);
            }

            void WithoutAttribute((Type Drawer, bool UseForChildren) tuple)
            {
                StackablePropertyDrawer drawer = (StackablePropertyDrawer)Activator.CreateInstance(tuple.Drawer);
                drawer.FieldInfo = fieldInfo;
                hasOnGUI |= drawer.HasOnGUI;
                list.Add(drawer);
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IReadOnlyList<StackablePropertyDrawer> drawers = Drawers ??= GetDrawers();

            bool includeChildren = true;
            bool visible = true;
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
                drawers[i].BeforeOnGUI(ref position, ref property, ref label, ref includeChildren, ref visible);

            if (visible)
            {
                if (hasOnGUI)
                {
                    for (int i = 0; i < count; i++)
                    {
                        StackablePropertyDrawer stackableDrawer = drawers[i];
                        if (stackableDrawer.HasOnGUI)
                            stackableDrawer.OnGUI(position, property, label, includeChildren);
                    }
                }
                else
                    EditorGUI.PropertyField(position, property, label, includeChildren);
            }

            for (int i = count - 1; i >= 0; i--)
                drawers[i].AfterOnGUI(position, property, label, includeChildren, visible);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            IReadOnlyList<StackablePropertyDrawer> drawers = Drawers ??= GetDrawers();

            bool includeChildren = true;
            bool visible = true;
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
                drawers[i].BeforeGetPropertyHeight(ref property, ref label, ref includeChildren, ref visible);

            float height = 0;
            if (visible)
            {
                if (!hasOnGUI)
                    height = EditorGUI.GetPropertyHeight(property, label, includeChildren);

                for (int i = count - 1; i >= 0; i--)
                    height = drawers[i].GetPropertyHeight(property, label, includeChildren, height);
            }

            for (int i = count - 1; i >= 0; i--)
                drawers[i].AfterGetPropertyHeight(property, label, includeChildren, visible, height);

            if (height == 0)
                height = -EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }
}