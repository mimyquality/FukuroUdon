using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Add margins so as not to interfere with other Hierarchy extensions.")]
    internal class HierarchySpacer : IHierarchyExtensionComponent
    {
        public int Priority => EditorToolboxSettings.instance.hierarchySpacerPriority;

        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect)
        {
            currentRect.x = fullRect.xMax - EditorToolboxSettings.instance.hierarchySpacerWidth;
        }
    }
}
