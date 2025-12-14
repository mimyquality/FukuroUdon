using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Mark any asset to make it easier to find.")]
    internal class AssetMarker : IProjectExtensionComponent
    {
        public int Priority => -1700;
        private Dictionary<Texture, Texture> iconDic = new();

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(AssetMarkerData.instance.TryGetColor(guid, out var color))
            {
                var defaultIcon = AssetDatabase.GetCachedIcon(path);
                if(!iconDic.TryGetValue(defaultIcon, out var icon))
                    iconDic[defaultIcon] = icon = GraphicUtils.ProcessTexture("Hidden/_lil/IconMaker", defaultIcon);

                if(ProjectExtension.isIconGUI)
                {
                    var rect = fullRect;
                    var size = Mathf.Min(rect.width, rect.height);
                    rect.width = size;
                    rect.height = size;
                    GUI.DrawTexture(rect, icon, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
                }
                else
                {
                    EditorGUI.DrawRect(fullRect, new Color(color.r, color.g, color.b, 0.25f));
                }
            }
        }
    }
}
