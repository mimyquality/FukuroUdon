using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Overlay any image on the icon.")]
    internal class IconOverlay : IProjectExtensionComponent
    {
        public int Priority => -1600;

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(!IconOverlayData.Dic.TryGetValue(guid, out var icon) || !icon) return;
            var rect = fullRect;
            var size = Mathf.Min(rect.width, rect.height);
            rect.width = size;
            rect.height = size;
            if(!ProjectExtension.isIconGUI) rect.x += 4;

            GUI.DrawTexture(rect, icon);
        }
    }
}
