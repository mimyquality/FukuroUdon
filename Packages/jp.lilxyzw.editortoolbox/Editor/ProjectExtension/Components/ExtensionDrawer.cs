using System.IO;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays the file extension.")]
    internal class ExtensionDrawer : IProjectExtensionComponent
    {
        public int Priority => 0;

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(ProjectExtension.isIconGUI || !File.Exists(path) || ProjectExtension.isSubAsset) return;
            currentRect.width = Common.GetTextWidth(extension);
            GUI.Label(currentRect, extension, EditorStyles.label);
            currentRect.x += currentRect.width;
        }
    }
}
