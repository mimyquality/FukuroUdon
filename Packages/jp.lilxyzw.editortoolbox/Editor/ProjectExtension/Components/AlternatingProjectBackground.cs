using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Alternates the background color of the Project window.")]
    internal class AlternatingProjectBackground : IProjectExtensionComponent
    {
        public int Priority => -1500;

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(!ProjectExtension.isIconGUI && (int)fullRect.y % (int)(fullRect.height*2) >= (int)fullRect.height)
                EditorGUI.DrawRect(fullRect, EditorToolboxSettings.instance.backgroundColor);
        }
    }
}
