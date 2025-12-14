using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    internal static class ToolbarExtension
    {
        private static VisualElement rootVisualElement;
        private static VisualElement leftElement;
        private static VisualElement rightElement;
        private static readonly HashSet<VisualElement> addedElements = new();

        private static List<IToolbarExtensionComponent> toolbarExtensionComponents;

        internal static readonly Type[] types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetCustomAttributes(typeof(ExportsToolbarExtensionComponent), false))
            .SelectMany(export => ((ExportsToolbarExtensionComponent)export).Types).ToArray();

        internal static readonly (string[] key, string fullname)[] names = types.Select(t => (new[]{Common.ToDisplayName(t.Name), t.GetCustomAttribute<TooltipAttribute>()?.tooltip}, t.FullName)).ToArray();
        internal static string[][] GetNameAndTooltips() => names.Select(n => n.key).ToArray();

        private static void Resolve()
        {
            toolbarExtensionComponents = new List<IToolbarExtensionComponent>();
            toolbarExtensionComponents.AddRange(types.Where(t => EditorToolboxSettings.instance.toolbarComponents.Contains(t.FullName)).Select(t => (IToolbarExtensionComponent)Activator.CreateInstance(t)).OrderBy(c => c.Priority));

            foreach(var element in addedElements)
            {
                if(leftElement.Contains(element)) leftElement.Remove(element);
                if(rightElement.Contains(element)) rightElement.Remove(element);
            }
            addedElements.Clear();

            foreach(var c in toolbarExtensionComponents)
            {
                if(c.InLeftSide) AddToLeft(c.GetRootElement());
                else AddToRight(c.GetRootElement());
            }
        }

        private static void AddToLeft(VisualElement element)
        {
            leftElement.Add(element);
            addedElements.Add(element);
        }

        private static void AddToRight(VisualElement element)
        {
            rightElement.Add(element);
            addedElements.Add(element);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update -= GetVisualElements;
            EditorApplication.update += GetVisualElements;
            EditorToolboxSettingsEditor.update += Resolve;
            EditorToolboxSettingsEditor.update += Resolve;
        }

        private static void GetVisualElements()
        {
            rootVisualElement = ToolbarWrap.get.m_Root;
            if(rootVisualElement == null) return;
            leftElement = rootVisualElement.Q("ToolbarZoneLeftAlign");
            rightElement = rootVisualElement.Q("ToolbarZoneRightAlign");
            if(toolbarExtensionComponents == null) Resolve();
            EditorApplication.update -= GetVisualElements;
        }
    }
}
