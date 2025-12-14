using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal class AssetOverwriter : AssetPostprocessor
    {
        private static HashSet<string> filesToRemove = new HashSet<string>();

        void OnPreprocessAsset()
        {
            // metaファイルがあればreturn
            if(!assetImporter.importSettingsMissing) return;

            // D&D前後でファイル名が一致している場合はreturn
            var name = Path.GetFileNameWithoutExtension(assetPath);
            if(DragAndDrop.paths.Length == 0 || DragAndDrop.paths.Any(p => name == Path.GetFileName(p))) return;

            // ホントは先頭で判断したいけどエラーになる
            if(!EditorToolboxSettings.instance.dragAndDropOverwrite) return;

            // ファイル名が" 数字"で終わっていなければreturn
            var count = name.Split(" ").Last();
            if(!uint.TryParse(count, out _)) return;

            // 上書き先のファイルが存在しなければreturn
            var original = Path.GetDirectoryName(assetPath) + Path.DirectorySeparatorChar + name.Substring(0, name.Length - count.Length - 1) + Path.GetExtension(assetPath);
            if(!File.Exists(original)) return;

            File.Copy(assetPath, original, true);
            filesToRemove.Add(assetPath);
            Debug.Log($"Overwrite {original}");

            // ダミーのファイルを作って高速化
            // ファイル削除だとエラーになる
            if(assetImporter is TextureImporter) WriteAndCopyTime(assetPath, new Texture2D(1,1).EncodeToPNG()); // 拡張子はおそらく関係ないのでpng固定
            if(assetPath.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase)) WriteAndCopyTime(assetPath, new byte[]{});
            if(assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) WriteAndCopyTime(assetPath, "; FBX 7.7.0 project file");
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if(filesToRemove.Count == 0) return;
            foreach(var file in filesToRemove) AssetDatabase.DeleteAsset(file);
            filesToRemove.Clear();
        }

        private static void WriteAndCopyTime(string path, string value)
        {
            var fi = new FileInfo(path);
            var timeC = fi.CreationTime;
            var timeW = fi.LastWriteTime;
            File.WriteAllText(path, value);
            new FileInfo(path){CreationTime = timeC, LastWriteTime = timeW};
        }

        private static void WriteAndCopyTime(string path, byte[] value)
        {
            var fi = new FileInfo(path);
            var timeC = fi.CreationTime;
            var timeW = fi.LastWriteTime;
            File.WriteAllBytes(path, value);
            new FileInfo(path){CreationTime = timeC, LastWriteTime = timeW};
        }
    }
}
