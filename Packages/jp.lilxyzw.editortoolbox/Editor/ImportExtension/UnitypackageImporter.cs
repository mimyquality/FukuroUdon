using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    // ディレクトリが空だとUnityのバグでエラーになるので ./ は必須
    [FilePath("./jp.lilxyzw.editortoolbox.UnitypackageImporter.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class UnitypackageImporter : ScriptableSingleton<UnitypackageImporter>
    {
        public List<UnitypackageAssets> importedAssets = new();
        private static string lastProjectPath = "";

        [InitializeOnLoadMethod]
        internal static void Init()
        {
            AssetDatabase.importPackageStarted -= OnImportPackageStarted;
            AssetDatabase.importPackageStarted += OnImportPackageStarted;
            DragAndDrop.AddDropHandler((_,path,_) => {
                lastProjectPath = path;
                return DragAndDropVisualMode.None;
            });
            if(instance.importedAssets.Count == 0) AddToList("None", new());
        }

        internal void Save() => instance.Save(true);

        private static void AddToList(string name, List<string> guids)
        {
            var first = instance.importedAssets.FirstOrDefault(i => i.name == name);
            if(first == null)
                instance.importedAssets.Add(new UnitypackageAssets{name = name, guids = guids});
            else
                first.guids = first.guids.Union(guids).Distinct().ToList();
            instance.Save(true);
        }

        private static void OnImportPackageStarted(string name)
        {
            if(DragAndDrop.paths.Length == 0) return;
            runtime.CoroutineHandler.StartStaticCoroutine(ProcessPackageImportWindow(name));
        }

        private static IEnumerator ProcessPackageImportWindow(string name)
        {
            while(!PackageImportWrap.HasOpenInstances()) yield return null;
            var window = new PackageImportWrap(EditorWindow.GetWindow(PackageImportWrap.type));
            var m_ImportPackageItems = window.m_ImportPackageItems;
            var items = m_ImportPackageItems.Select(o => new ImportPackageItemWrap(o)).ToArray();

            var packageName = $"{name}.unitypackage";
            AddToList(packageName, items.Select(i => i.guid).ToList());

            // Packages配下の上書き防止
            if(EditorToolboxSettings.instance.cancelUnitypackageOverwriteInPackages)
            {
                foreach(var item in items)
                {
                    var dest = item.destinationAssetPath;
                    if(!dest.StartsWith("Packages/") || !Directory.Exists(dest[..(dest.IndexOf('/', "Packages/".Length) + 1)])) continue;
                    item.assetChanged = false;
                }
            }

            if(window.ShowTreeGUI(m_ImportPackageItems))
            {
                var root = new VisualElement();
                root.style.marginLeft = 6;
                root.style.alignItems = Align.Center;
                root.style.flexDirection = FlexDirection.Row;
                window.w.rootVisualElement.Add(root);

                // スクリプトの検知
                if (EditorToolboxSettings.instance.addUnitypackageContainsScriptWarning && items.Any(i => i.destinationAssetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || i.destinationAssetPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    var help = new HelpBox(L10n.L("This unitypackage contains the script."), HelpBoxMessageType.Warning);
                    root.Add(help);
                    var excludebutton = new Button { text = L10n.L("Exclude scripts from import") };
                    help.Add(excludebutton);
                    excludebutton.clicked += () =>
                    {
                        foreach (var item in items)
                        {
                            var dest = item.destinationAssetPath;
                            if (!dest.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && !dest.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && !dest.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase) && !dest.EndsWith(".asmref", StringComparison.OrdinalIgnoreCase)) continue;
                            item.assetChanged = false;
                        }
                        root.Remove(help);
                    };
                }

                // インポート先の変更
                if (!EditorToolboxSettings.instance.addUnitypackageDirectorySelectionMenu || string.IsNullOrEmpty(lastProjectPath) || !lastProjectPath.StartsWith("Assets/")) yield break;
                var path = lastProjectPath;
                var origins = new Dictionary<object, string>();
                root.Add(new Label(L10n.L("Import directory")));

                var button = new Button(){text = "Assets/"};
                button.style.width = Common.GetTextWidth(button.text) + 16;
                button.clicked += () => {
                    if(origins.Count == 0)
                    {
                        foreach(var item in items)
                        {
                            if(!string.IsNullOrEmpty(item.existingAssetPath) || !item.destinationAssetPath.StartsWith("Assets/")) continue;
                            origins[item] = item.destinationAssetPath;
                            item.destinationAssetPath = path + item.destinationAssetPath.Substring("Assets".Length);
                        }
                        button.text = path+"/";
                        button.style.width = Common.GetTextWidth(button.text) + 16;
                    }
                    else
                    {
                        foreach(var item in items)
                            if(origins.TryGetValue(item, out var path)) item.destinationAssetPath = path;
                        origins.Clear();
                        button.text = "Assets/";
                        button.style.width = Common.GetTextWidth(button.text) + 16;
                    }
                };

                root.Add(button);
            }
            else
            {
                // 既存アセットの場所を表示
                var button = new Button(){text = L10n.L("Show Import Window")};
                button.style.marginTop = 36;
                button.style.width = Common.GetTextWidth(button.text) + 32;
                button.clicked += () => {
                    foreach(var i in items)
                        if(!i.isFolder) i.assetChanged = true;
                    window.w.rootVisualElement.Remove(button);
                };
                window.w.rootVisualElement.Add(button);
            }
        }

        [Serializable]
        internal class UnitypackageAssets
        {
            public string name;
            public List<string> guids;
        }
    }
}
