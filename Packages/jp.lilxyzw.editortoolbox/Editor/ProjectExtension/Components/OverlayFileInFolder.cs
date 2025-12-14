using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Overlays the folder icon with the files inside.")]
    internal class OverlayFileInFolder : IProjectExtensionComponent
    {
        public int Priority => -1550;
        private static Dictionary<string, Texture> icons = new();
        private static Material material;

        private static Texture MakeIcon(Texture tex)
        {
            if(!material)
            {
                material = new Material(Shader.Find("Hidden/_lil/OutlineMaker"));
                material.SetColor("_OutlineColor", EditorGUIUtility.isProSkin ? new Color(0.2f,0.2f,0.2f) : new Color(0.75f,0.75f,0.75f));
                material.SetFloat("_OutlineWidth", 4);
            }
            return GraphicUtils.ProcessTexture(material, tex);
        }

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(!Directory.Exists(path)) return;
            if(!icons.TryGetValue(path, out var icon))
            {
                icons[path] = null;
                var files = Directory.GetFiles(path);
                if(files.Length == 0) return;

                var groups = files.GroupBy(f => Path.GetExtension(f)).Where(g => !string.IsNullOrEmpty(g.Key) && g.Key != ".meta").OrderByDescending(g => g.Count());
                if(groups.Count() == 0) return;
                var assetPath = groups.First().First();
                icon = AssetDatabase.GetCachedIcon(assetPath);
                if(icon) icon = MakeIcon(MakeIcon(icon));
                icons[path] = icon;
            }

            if(!icon) return;
            var rect = fullRect;
            var size = Mathf.Min(rect.width, rect.height);
            rect.width = size;
            rect.height = size;
            if(ProjectExtension.isIconGUI)
            {
                rect.xMin += size * 0.5f;
                rect.yMin += size * 0.5f;
                rect.x -= size * 0.05f;
                rect.y -= size * 0.1f;
            }
            else
            {
                rect.xMin += size * 0.25f;
                rect.yMin += size * 0.25f;
                rect.x += 2;
            }

            GUI.DrawTexture(rect, icon);
        }
    }
}
