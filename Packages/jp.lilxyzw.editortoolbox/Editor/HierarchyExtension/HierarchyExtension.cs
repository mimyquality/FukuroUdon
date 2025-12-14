using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal class HierarchyExtension
    {
        private static readonly Color32 missingScriptIconColor = new Color32(255,0,255,255);
        private const int ICON_SIZE = 16;
        internal static Texture2D m_missingScriptIcon;
        internal static Texture2D MissingScriptIcon()
        {
            if(m_missingScriptIcon) return m_missingScriptIcon;
            return m_missingScriptIcon = GenerateTexture(missingScriptIconColor,ICON_SIZE,ICON_SIZE);
        }
        private static List<IHierarchyExtensionComponent> hierarchyExtensionComponents;

        [InitializeOnLoadMethod] private static void Initialize()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
            EditorToolboxSettingsEditor.update -= Resolve;
            EditorToolboxSettingsEditor.update += Resolve;
        }

        internal static readonly Type[] types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetCustomAttributes(typeof(ExportsHierarchyExtensionComponent), false))
            .SelectMany(export => ((ExportsHierarchyExtensionComponent)export).Types).ToArray();

        internal static readonly (string[] key, string fullname)[] names = types.Select(t => (new[]{Common.ToDisplayName(t.Name), t.GetCustomAttribute<TooltipAttribute>()?.tooltip}, t.FullName)).ToArray();
        internal static string[][] GetNameAndTooltips() => names.Select(n => n.key).ToArray();

        private static void Resolve()
        {
            hierarchyExtensionComponents = new List<IHierarchyExtensionComponent>();
            hierarchyExtensionComponents.AddRange(types.Where(t => EditorToolboxSettings.instance.hierarchyComponents.Contains(t.FullName)).Select(t => (IHierarchyExtensionComponent)Activator.CreateInstance(t)).OrderBy(c => c.Priority));
        }

        // GUI
        private static void OnGUI(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if(!go) return;

            var rectOrigin = selectionRect;
            selectionRect.x = selectionRect.xMax;

            if(hierarchyExtensionComponents == null) Resolve();

            var mix = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = false;
            foreach(var c in hierarchyExtensionComponents) c.OnGUI(ref selectionRect, go, instanceID, rectOrigin);
            EditorGUI.showMixedValue = mix;
        }

        private static Texture2D GenerateTexture(Color32 color, int width, int height)
        {
            var tex = new Texture2D(width,height,TextureFormat.RGBA32,false);
            tex.SetPixels32(Enumerable.Repeat(color,width*height).ToArray());
            tex.Apply();
            return tex;
        }
    }
}
