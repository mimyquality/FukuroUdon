using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays the parent material if the material is a variant.")]
    internal class MaterialVariant : IProjectExtensionComponent
    {
        public int Priority => 0;

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(ProjectExtension.isIconGUI || extension != ".mat" || ProjectExtension.GUIDToObject(guid) is not Material material) return;

            if(material.isVariant && material.parent) GUIHelper.DrawLabel(ref currentRect, $"Variant: {material.parent.name}");
        }
    }
}
