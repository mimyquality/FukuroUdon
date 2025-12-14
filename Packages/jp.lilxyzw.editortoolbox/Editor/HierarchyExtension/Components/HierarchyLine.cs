using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays lines representing the parent-child relationships of objects in the Hierarchy.")]
    internal class HierarchyLine : IHierarchyExtensionComponent
    {
        public int Priority => -900;

        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect)
        {
            if(gameObject.transform.parent)
            {
                var transform = gameObject.transform;

                // 横線
                var rectHorizLine = fullRect;
                rectHorizLine.height = 1;
                rectHorizLine.x -= fullRect.height * 0.5f;
                rectHorizLine.x -= fullRect.height - 2;
                rectHorizLine.width = transform.childCount > 0 ? fullRect.height * 0.5f : fullRect.height * 1.2f;
                rectHorizLine.y += fullRect.height * 0.5f;
                EditorGUI.DrawRect(rectHorizLine, EditorToolboxSettings.instance.lineColor);

                // 縦線
                var rectLine = fullRect;
                rectLine.width = 1;
                rectLine.x -= fullRect.height * 0.5f;

                rectLine.x -= fullRect.height - 2;
                rectLine.height = IsLastChild(transform) ? fullRect.height * 0.5f : fullRect.height;
                EditorGUI.DrawRect(rectLine, EditorToolboxSettings.instance.lineColor);
                transform = transform.parent;
                rectLine.height = fullRect.height;

                while(transform)
                {
                    rectLine.x -= fullRect.height - 2;
                    if(transform.parent && !IsLastChild(transform)) EditorGUI.DrawRect(rectLine, EditorToolboxSettings.instance.lineColor);
                    transform = transform.parent;
                }
            }
        }

        private static bool IsLastChild(Transform t) => t.parent && t.GetSiblingIndex() == t.parent.childCount-1;
    }
}
