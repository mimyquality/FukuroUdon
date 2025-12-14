using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Highlight the assets you imported in each Unitypackage.")]
    internal class UnitypackageHilighter : IProjectExtensionComponent
    {
        public int Priority => -1400;

        private static HashSet<string> guids = new();
        private static Dictionary<string, HashSet<string>> folderItems = new();

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(guids.Contains(guid))
                EditorGUI.DrawRect(fullRect, EditorToolboxSettings.instance.backgroundHilightColor);
            
            else if(Directory.Exists(path) && GetItems(path).Any(p => guids.Contains(p)))
            {
                if(ProjectExtension.isIconGUI)
                {
                    var rect = fullRect;
                    rect.xMax = rect.xMin+6;
                    EditorGUI.DrawRect(rect, EditorToolboxSettings.instance.backgroundHilightColor);

                    rect = fullRect;
                    rect.xMin = rect.xMax-6;
                    EditorGUI.DrawRect(rect, EditorToolboxSettings.instance.backgroundHilightColor);
                }
                else
                {
                    var rect = fullRect;
                    rect.xMin = 0;
                    rect.xMax = 6;
                    EditorGUI.DrawRect(rect, EditorToolboxSettings.instance.backgroundHilightColor);
                }
            }
        }

        private static HashSet<string> GetItems(string path)
        {
            if(folderItems.TryGetValue(path, out var items)) return items;
            var current = Directory.GetCurrentDirectory();
            return folderItems[path] = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Where(p => !p.EndsWith(".meta"))
                .Union(Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                .Select(p => AssetDatabase.AssetPathToGUID(Path.GetRelativePath(current, p)))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet();
        }

        private class FolderResetter : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] a, string[] b, string[] c, string[] d)
                => folderItems.Clear();
        }

        private class UnitypackageHilighterData : ScriptableSingleton<UnitypackageHilighterData>
        {
            public string target = "None";
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            AssetDatabase.importPackageCompleted -= UpdateGUIDs;
            AssetDatabase.importPackageCompleted += UpdateGUIDs;
            EditorApplication.update -= SelectorButton.AddButton;
            EditorApplication.update += SelectorButton.AddButton;
            EditorToolboxSettingsEditor.update -= SelectorButton.UpdateButton;
            EditorToolboxSettingsEditor.update += SelectorButton.UpdateButton;
            UpdateGUIDs(UnitypackageHilighterData.instance.target);
        }

        private static void UpdateGUIDs(string name)
        {
            if(name != "None" && !name.EndsWith(".unitypackage")) name = $"{name}.unitypackage";
            var match = UnitypackageImporter.instance.importedAssets.LastOrDefault(i => i.name == name);
            if(match != null) guids = match.guids.ToHashSet();
            else guids.Clear();
            UnitypackageHilighterData.instance.target = name;
            SelectorButton.UpdateButton();
        }

        private class SelectorButton
        {
            private static IconPopup popup;
            private static List<string> names;
            private static List<string> Names => names != null && names.Count == UnitypackageImporter.instance.importedAssets.Count ? names : names = UnitypackageImporter.instance.importedAssets.Select(i => i.name).ToList();
            private static Texture tex = EditorGUIUtility.IconContent("LightingSettings Icon").image;

            internal static void AddButton()
            {
                if(popup == null) InitButton();
                ProjectToolbarExtension.Add(popup);
                EditorApplication.update -= AddButton;
            }

            private static void InitButton()
            {
                popup = new(tex, Names.ToList(), UnitypackageHilighterData.instance.target);
                popup.style.width = 24;
                popup.valueChanged += UpdateGUIDs;
                popup.rightClicked += UnitypackageLogEditor.Init;
                UpdateButton();
            }

            internal static void UpdateButton()
            {
                if(popup == null)
                {
                    InitButton();
                    return;
                }
                popup.choices = Names;
                popup.value = UnitypackageHilighterData.instance.target;
                popup.style.display = EditorToolboxSettings.instance.projectComponents.Any(c => c == typeof(UnitypackageHilighter).FullName) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            private class UnitypackageLogEditor : EditorWindow
            {
                internal static void Init() => GetWindow(typeof(UnitypackageLogEditor)).Show();
                internal static List<string> namesLocal;
                public Vector2 scrollPos;

                void OnGUI()
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    if(namesLocal == null || namesLocal.Count != (Names.Count-1))
                        namesLocal = Names.Where(n => n != "None").ToList();

                    int i = 1;
                    foreach(var n in namesLocal)
                    {
                        var rect = EditorGUILayout.GetControlRect();
                        var xMax = rect.xMax;
                        var buttonWidth = L10n.GetTextWidth("Remove") + 10;
                        rect.width -= buttonWidth + 4;
                        EditorGUI.LabelField(rect, n);
                        rect.xMin = rect.xMax;
                        rect.xMax = xMax;
                        EditorGUI.BeginChangeCheck();
                        L10n.Button(rect, "Remove");
                        if(EditorGUI.EndChangeCheck())
                        {
                            if(n == UnitypackageHilighterData.instance.target) guids.Clear();
                            UnitypackageImporter.instance.importedAssets.RemoveAt(i);
                            UnitypackageImporter.instance.Save();
                            UpdateButton();
                        }
                        i++;
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }
    }
}
