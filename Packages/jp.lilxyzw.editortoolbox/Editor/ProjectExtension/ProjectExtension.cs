using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{
    public class ProjectExtension
    {
        // アイコン表示かどうか
        public static bool isIconGUI = false;

        // サブアセットかどうか
        public static bool isSubAsset = false;

        // 各IProjectExtensionComponentでオブジェクトのロードをすると無駄なのでここに集約
        private static readonly Dictionary<string, Object> objMap = new();
        public static Object GUIDToObject(string guid)
        {
            if(objMap.TryGetValue(guid, out var obj)) return obj;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if(string.IsNullOrEmpty(path)) return objMap[guid] = null;
            return objMap[guid] = AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        internal static readonly Type[] types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetCustomAttributes(typeof(ExportsProjectExtensionComponent), false))
            .SelectMany(export => ((ExportsProjectExtensionComponent)export).Types).ToArray();

        internal static readonly (string[] key, string fullname)[] names = types.Select(t => (new[]{Common.ToDisplayName(t.Name), t.GetCustomAttribute<TooltipAttribute>()?.tooltip}, t.FullName)).ToArray();
        internal static string[][] GetNameAndTooltips() => names.Select(n => n.key).ToArray();

        private static List<IProjectExtensionComponent> projectExtensionComponents;
        private static string guidEditing = "";
        private static Dictionary<EditorWindow, (SerializedObject so, SerializedProperty m_IsRenaming, SerializedProperty m_IsWaitingForDelay)> windows = new();

        private static void Resolve()
        {
            projectExtensionComponents = new List<IProjectExtensionComponent>();
            projectExtensionComponents.AddRange(types.Where(t => EditorToolboxSettings.instance.projectComponents.Contains(t.FullName)).Select(t => (IProjectExtensionComponent)Activator.CreateInstance(t)).OrderBy(c => c.Priority));
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
            EditorApplication.projectWindowItemOnGUI -= Draw;
            EditorApplication.projectWindowItemOnGUI += Draw;
            EditorToolboxSettingsEditor.update -= Resolve;
            EditorToolboxSettingsEditor.update += Resolve;
        }

        private static void EditorUpdate()
        {
            guidEditing = "";
            try
            {
                if(EditorWindow.focusedWindow && EditorWindow.focusedWindow.GetType().FullName == "UnityEditor.ProjectBrowser")
                {
                    if(!windows.TryGetValue(EditorWindow.focusedWindow, out var values))
                    {
                        values.so = new SerializedObject(EditorWindow.focusedWindow);
                        values.m_IsRenaming = values.so.FindProperty("m_ListAreaState.m_RenameOverlay.m_IsRenaming");
                        values.m_IsWaitingForDelay = values.so.FindProperty("m_ListAreaState.m_RenameOverlay.m_IsWaitingForDelay");
                        windows.Add(EditorWindow.focusedWindow, values);
                    }
                    values.so.Update();
                    if(values.m_IsRenaming.boolValue && !values.m_IsWaitingForDelay.boolValue) guidEditing = Selection.assetGUIDs[0];
                }
            }
            catch{}
        }

        private static string guidPrev = "";

        private static void Draw(string guid, Rect position)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if(string.IsNullOrEmpty(path) || guidEditing == guid) return;

            var name = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);
            var currentRect = position;
            currentRect.xMin += Common.GetTextWidth(name) + 17;
            if(position.x > 16) // One Column
            {
                currentRect.xMin -= 2;
                currentRect.y -= 1;
            }
            isIconGUI = position.height > 16;

            if(projectExtensionComponents == null) Resolve();

            isSubAsset = guid == guidPrev;
            foreach(var c in projectExtensionComponents) c.OnGUI(ref currentRect, guid, path, name, extension, position);

            guidPrev = guid;
        }
    }
}
