using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using jp.lilxyzw.editortoolbox.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal static partial class DocsGeneratorMenu
    {
        private static int frameCount = 0;
        private static string currentLang;
        private static HashSet<string> queue = new();
        private static Type[] types;

        [MenuItem("Help/DocsGenerator/lilEditorToolbox")]
        private static void Generate()
        {
            currentLang = EditorToolboxSettings.instance.language;
            var asms = new HashSet<Assembly>(){Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(EditorOnlyBehaviour))};
            types = asms.SelectMany(a => a.GetTypes()).Where(t => t.GetCustomAttribute<DocsAttribute>() != null).ToArray();
            var langs = L10n.GetLanguages();
            queue.UnionWith(langs);
            EditorApplication.update += Next;
            BuildIndexMts(langs);
        }

        private static void Next()
        {
            EditorApplication.update -= Next;
            frameCount = 0;
            windowQueue.Clear();
            if(queue.Count > 0)
            {
                var lang = queue.First();
                queue.Remove(lang);
                EditorToolboxSettings.instance.language = lang;
                L10n.Load();
                var path = AssetDatabase.GUIDToAssetPath("3e480d0d691fbaf4c82d5ea65c66a497") + "/" + lang + ".po";
                var localizationAsset = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(path);
                Func<string,string> loc = localizationAsset.GetLocalizedString;
                var code = lang.Replace('-', '_');
                var root = $"docs/{code}";
                DocsGenerator.Generate(
                    (t) => TypeToPath(root, t),
                    loc,
                    GetHeader,
                    GetTooltip,
                    NeedToDraw,
                    (t,sb) => ActionPerType(t,sb,code),
                    types);

                BuildHome(root, code, loc);
                BuildDocsIndex(root, code, loc);
                BuildIndex(root, code, loc);
            }
            else
            {
                EditorToolboxSettings.instance.language = currentLang;
            }
        }

        private static (string,string) GetHeader(FieldInfo field)
        {
            var l10nHeader = field.GetCustomAttribute<L10nHeaderAttribute>();
            var header = l10nHeader?.key[0] ?? field.GetCustomAttribute<HeaderAttribute>()?.header;
            var headertooltip = l10nHeader?.key.Length == 2 ? l10nHeader?.key[1] : null;
            return (header, headertooltip);
        }

        private static string GetTooltip(FieldInfo field) => field.GetCustomAttribute<TooltipAttribute>()?.tooltip;
        private static bool NeedToDraw(FieldInfo field) => field.GetCustomAttribute<SerializeField>() != null || !field.IsNotSerialized && !field.IsStatic && field.IsPublic;

        private static string TypeToPath(string root, Type type)
        {
            if(type == typeof(EditorToolboxSettings))
            {
                return $"{root}/docs/Settings.md";
            }
            else if(type.IsSubclassOf(typeof(EditorWindow)))
            {
                return $"{root}/docs/EditorWindow/{type.Name}.md";
            }
            else if(type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                return $"{root}/docs/Components/{type.Name}.md";
            }
            else
            {
                return $"{root}/docs/{type.Name}.md";
            }
        }

        private static void WriteText(string path, string text)
        {
            var directory = Path.GetDirectoryName(path);
            if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        private static void WriteBytes(string path, byte[] bytes)
        {
            var directory = Path.GetDirectoryName(path);
            if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllBytes(path, bytes);
        }
    }
}
