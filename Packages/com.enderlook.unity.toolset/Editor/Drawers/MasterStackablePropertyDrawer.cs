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

        private IReadOnlyList<StackablePropertyDrawer> GetDrawers(SerializedProperty property)
        {
            if (drawersMap is null)
                Reset();

            List<StackablePropertyDrawer> list = new List<StackablePropertyDrawer>();

            MemberInfo memberInfo = property.GetMemberInfo();
            Type memberType;
            if (memberInfo is FieldInfo fieldInfo)
                memberType = fieldInfo.FieldType;
            else if (memberInfo is PropertyInfo propertyInfo)
                memberType = propertyInfo.PropertyType;
            else
            {
                Debug.Assert(false, "Invalid member type.");
                memberType = null;
            }

            if (drawersMap.TryGetValue(memberType, out (Type Drawer, bool UseForChildren) tuple) && !(tuple.Drawer is null))
                WithoutAttribute(tuple);
            else
            {
                Type type = memberType;
            start:
                type = type.BaseType;
                if (!(type is null))
                {
                    if (drawersMap.TryGetValue(type, out tuple))
                    {
                        if (!(tuple.Drawer is null) && tuple.UseForChildren)
                        {
                            drawersMap.Add(memberType, (tuple.Drawer, true));
                            WithoutAttribute(tuple);
                        }
                        else
                            drawersMap.Add(memberType, default);
                    }
                    else
                        goto start;
                }
            }

            IEnumerable<PropertyAttribute> attributes = memberInfo.GetCustomAttributes<PropertyAttribute>(true);
            foreach (PropertyAttribute attribute in attributes)
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
                drawer.SetAttribute(attribute);
                hasOnGUI |= drawer.HasOnGUI;
                list.Add(drawer);
            }

            void WithoutAttribute((Type Drawer, bool UseForChildren) tuple)
            {
                StackablePropertyDrawer drawer = (StackablePropertyDrawer)Activator.CreateInstance(tuple.Drawer);
                hasOnGUI |= drawer.HasOnGUI;
                list.Add(drawer);
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IReadOnlyList<StackablePropertyDrawer> drawers = Drawers ??= GetDrawers(property);

            SerializedPropertyInfo propertyInfo = new SerializedPropertyInfo(property, fieldInfo);

            bool includeChildren = true;
            bool visible = true;
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
                drawers[i].BeforeOnGUI(ref position, ref propertyInfo, ref label, ref includeChildren, ref visible);

            if (visible)
            {
                if (hasOnGUI)
                {
                    for (int i = 0; i < count; i++)
                    {
                        StackablePropertyDrawer stackableDrawer = drawers[i];
                        if (stackableDrawer.HasOnGUI)
                            stackableDrawer.OnGUI(position, propertyInfo, label, includeChildren);
                    }
                }
                else
                    EditorGUI.PropertyField(position, property, label, includeChildren);
            }

            for (int i = count - 1; i >= 0; i--)
                drawers[i].AfterOnGUI(position, propertyInfo, label, includeChildren, visible);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            IReadOnlyList<StackablePropertyDrawer> drawers = Drawers ??= GetDrawers(property);

            SerializedPropertyInfo propertyInfo = new SerializedPropertyInfo(property, fieldInfo);

            bool includeChildren = true;
            bool visible = true;
            int count = drawers.Count;
            for (int i = 0; i < count; i++)
                drawers[i].BeforeGetPropertyHeight(ref propertyInfo, ref label, ref includeChildren, ref visible);

            float height = 0;
            if (visible)
            {
                if (!hasOnGUI)
                    height = EditorGUI.GetPropertyHeight(property, label, includeChildren);

                for (int i = count - 1; i >= 0; i--)
                    height = drawers[i].GetPropertyHeight(propertyInfo, label, includeChildren, height);
            }

            for (int i = count - 1; i >= 0; i--)
                drawers[i].AfterGetPropertyHeight(propertyInfo, label, includeChildren, visible, height);

            if (height == 0)
                height = -EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }
}