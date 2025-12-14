using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{
    internal static class Gatherer
    {
        [MenuItem(Common.MENU_HEAD + "[AssetGrimoire] Output GUIDs")]
        private static void GahterGUIDs()
        {
            var libs = AssetDatabase.FindAssets("t:GUIDLibrary", null);
            if(libs.Length == 0) return;

            var path = EditorUtility.SaveFolderPanel("Save", "", "");
            foreach(var guid in libs)
            {
                var outlib = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GUIDLibrary>(AssetDatabase.GUIDToAssetPath(guid)));

                if(outlib.folder)
                    outlib.guids = GatherGUIDsFromFolder(AssetDatabase.GetAssetPath(outlib.folder));
                #if LIL_SHADPZIPLIB
                else if(!string.IsNullOrEmpty(outlib.unitypackage))
                    outlib.guids = GatherGUIDsFromUnitypackage(outlib.unitypackage);
                #endif

                var name = !string.IsNullOrEmpty(outlib.name) ? outlib.name : !string.IsNullOrEmpty(outlib.displayName) ? outlib.displayName : outlib.folder.name;
                if(outlib.guids.Length==0)
                {
                    Debug.Log($"Skip: {name}");
                    continue;
                }
                outlib.SerializeToText($"{path}/{name}.txt");
            }
        }

        [MenuItem(Common.MENU_HEAD + "[AssetGrimoire] Make Target (Folder)")]
        private static void MakeTarget()
        {
            if(!Directory.Exists("Assets/GUIDList")) AssetDatabase.CreateFolder("Assets", "GUIDList");
            foreach(var folder in Selection.objects)
            {
                if(folder is not DefaultAsset) continue;
                var lib = ScriptableObject.CreateInstance<GUIDLibrary>();
                lib.folder = (DefaultAsset)folder;

                var packages = AssetDatabase.FindAssets("t:PackageManifest", new string[]{AssetDatabase.GetAssetPath(lib.folder)});
                if(packages.Count() > 0)
                {
                    var package = JsonUtility.FromJson<Package>(File.ReadAllText(AssetDatabase.GUIDToAssetPath(packages.First())));
                    lib.displayName = package.displayName;
                    lib.name = package.name;
                    lib.url = package.url;
                    lib.repo = package.repo;
                }

                var name = !string.IsNullOrEmpty(lib.name) ? lib.name : !string.IsNullOrEmpty(lib.displayName) ? lib.displayName : folder.name;
                if(string.IsNullOrEmpty(lib.displayName)) lib.displayName = name;

                AssetDatabase.CreateAsset(lib, $"Assets/GUIDList/{name}.asset");
            }
        }
        [MenuItem(Common.MENU_HEAD + "[AssetGrimoire] Update Database")]
        private static void UpdateDatabase() => GUIDLibrary.Update();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if(!File.Exists(Grimoire.PATH_DATABASE)) GUIDLibrary.Update();
        }

        private static string[] GatherGUIDsFromFolder(params string[] pathToSearch)
        {
            return AssetDatabase.FindAssets("", pathToSearch);
        }

        // Need to install com.unity.sharp-zip-lib
        // https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@1.3/manual/Installation.html
        #if LIL_SHADPZIPLIB
        [MenuItem(Common.MENU_HEAD + "[AssetGrimoire] Make Target (Unitypackage)")]
        private static void MakeTargetUnitypackage()
        {
            var path = EditorUtility.OpenFilePanel("Open", "", "unitypackage");
            if(!Directory.Exists("Assets/GUIDList")) AssetDatabase.CreateFolder("Assets", "GUIDList");

            var lib = ScriptableObject.CreateInstance<GUIDLibrary>();
            lib.unitypackage = path;
            lib.displayName = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(lib, $"Assets/GUIDList/{lib.displayName}.asset");
            Selection.activeObject = lib;
        }

        private static string[] GatherGUIDsFromUnitypackage(string path)
        {
            using var stream = File.OpenRead(path);
            using var gz = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            using var tar = new Unity.SharpZipLib.Tar.TarInputStream(gz, null);
            Unity.SharpZipLib.Tar.TarEntry te;
            var guids = new System.Collections.Generic.List<string>();
            while((te = tar.GetNextEntry())!=null)
            {
                if(te.IsDirectory && te.Name.Length >= 32)
                {
                    var name = te.Name[..32];
                    if(Guid.TryParse(name, out Guid res)) guids.Add(name);
                }
            }
            return guids.ToArray();
        }
        #endif
    }

    [Serializable]
    internal class Package
    {
        public string displayName = "";
        public string name = "";
        public string url = "";
        public string repo = "";
    }
}
