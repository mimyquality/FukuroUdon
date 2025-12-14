using System.IO;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays lines that represent the hierarchy of files.")]
    internal class ProjectLine : IProjectExtensionComponent
    {
        public int Priority => -1700;
        private static Rect lastRect = new();

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(ProjectExtension.isIconGUI) return;
            if(lastRect.y > fullRect.y) lastRect = new();
            if(lastRect.x > 16)
            {
                //var transform = gameObject.transform;
                var isFolder = Directory.Exists(path) && Directory.GetFiles(path).Length > 0;

                // 横線
                var rectHorizLine = lastRect;
                rectHorizLine.height = 1;
                rectHorizLine.x -= lastRect.height * 0.5f;
                rectHorizLine.x -= lastRect.height - 2;
                rectHorizLine.width = isFolder ? lastRect.height * 1.0f : lastRect.height * 1.2f;
                rectHorizLine.y += lastRect.height * 0.5f;
                EditorGUI.DrawRect(rectHorizLine, EditorToolboxSettings.instance.lineColor);

                // 縦線
                var rectLine = lastRect;
                rectLine.width = 1;
                rectLine.x -= lastRect.height * 0.5f;

                rectLine.x -= lastRect.height - 2;
                rectLine.height = IsLastChild(fullRect) ? lastRect.height * 0.5f : lastRect.height;
                EditorGUI.DrawRect(rectLine, EditorToolboxSettings.instance.lineColor);
                rectLine.height = lastRect.height;

                while(rectLine.x > 16)
                {
                    rectLine.x -= lastRect.height - 2;
                    EditorGUI.DrawRect(rectLine, EditorToolboxSettings.instance.lineColor);
                }
            }
            lastRect = fullRect;
        }

        private static bool IsLastChild(Rect fullRect) => lastRect.x > fullRect.x;
    }
}
