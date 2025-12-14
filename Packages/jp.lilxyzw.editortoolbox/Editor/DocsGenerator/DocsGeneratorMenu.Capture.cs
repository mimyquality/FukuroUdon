using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{
    internal static partial class DocsGeneratorMenu
    {
        private static HashSet<(Type type, EditorWindow window, string lang)> windowQueue = new();
        private static Object SampleObject => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath("570ab5666cbbf3646a455dd3e56dcbae"));
        private static Object SampleMaterialA => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath("21e309c40a0620543b17dabc7ee33a66"));
        private static Object SampleMaterialB => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath("6f396356d8a3428418d243364f348690"));

        private static void Capture()
        {
            if(frameCount == 5)
            {
                Selection.activeObject = Shader.Find("Hidden/_lil/KeywordInitializer");
                windowQueue.First(q => q.type == typeof(ShaderKeywordViewer)).window.Repaint();
            }
            if(frameCount++ < 10) return;
            foreach(var (type, window, lang) in windowQueue)
            {
                window.Focus();
                var pos = new Vector2(window.position.x, window.position.y);
                var w = (int)window.position.width;
                var h = (int)window.position.yMax + 21 - (int)pos.y;
                var pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(pos, w, h);
                var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
                texture.SetPixels(pixels);

                var path = $"docs/public/images/{lang}/EditorWindow/{type.Name}.png";
                var directory = Path.GetDirectoryName(path);
                if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                WriteBytes(path, texture.EncodeToPNG());

                window.Close();
            }
            EditorApplication.update -= Capture;
            EditorApplication.update += Next;
        }

        private static void ActionPerType(Type type, StringBuilder sb, string lang)
        {
            // 長いページは目次を表示
            if(type == typeof(EditorToolboxSettings)) sb.AppendLine("[[toc]]").AppendLine();

            if(!type.IsSubclassOf(typeof(EditorWindow))) return;
            frameCount = 0;
            if(windowQueue.Count == 0) EditorApplication.update += Capture;

            sb.AppendLine($"![{type.Name}](/images/{lang}/EditorWindow/{type.Name}.png \"{type.Name}\")");

            if(type == typeof(TabInspector))
            {
                Selection.activeObject = SampleObject;
            }

            var window = EditorWindow.CreateInstance(type) as EditorWindow;
            window.titleContent = new GUIContent(type.Name);
            window.Show();

            int width = 450;
            int height = 300;

            if(window is FolderOpener fo)
            {
                height = 500;
            }
            else if(window is JsonObjectViewer jv)
            {
                jv.target = SampleObject;
                jv.json = jv.target ? EditorJsonUtility.ToJson(jv.target, true) : "";
            }
            else if(window is MissingFinder mf)
            {
                mf.target = SampleObject;
                typeof(MissingFinder).GetMethod("ScanRecursive", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(mf, new object[]{mf.target});
            }
            else if(window is ReferenceReplacer rr)
            {
                rr.target = SampleObject;
                rr.from = SampleMaterialA;
                rr.to = SampleMaterialB;
            }
            else if(window is SceneCapture sc)
            {
            }
            else if(window is ShaderKeywordViewer skv)
            {
                skv.showBuiltin = true;
                height = 700;
            }
            else if(window is TabInspector ti)
            {
                ti.titleContent = new GUIContent(Selection.activeObject.name, AssetPreview.GetMiniThumbnail(Selection.activeObject));
                height = 700;
            }
            else if(window is TexturePacker tp)
            {
                tp.channelParams[0].tex = Texture2D.whiteTexture;
                typeof(TexturePacker).GetMethod("Pack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tp, null);
                width = 800;
                height = 500;
            }
            else if(window is TransformResetter tr)
            {
                tr.target = SampleObject as GameObject;
                tr.isHuman = false;
                tr.isPrefab = true;
                height = 400;
            }

            window.position = new Rect(10, 10, width, height-1);
            windowQueue.Add((type, window, lang));
        }
    }
}
