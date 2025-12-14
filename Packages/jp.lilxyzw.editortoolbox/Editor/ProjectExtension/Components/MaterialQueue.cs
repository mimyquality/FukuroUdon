using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays the Render Queue for materials.")]
    internal class MaterialQueue : IProjectExtensionComponent
    {
        public int Priority => 1;

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(ProjectExtension.isIconGUI || extension != ".mat" || ProjectExtension.GUIDToObject(guid) is not Material material) return;

            GUIHelper.DrawLabel(ref currentRect, $"Q: {material.renderQueue}");
        }
    }
}
