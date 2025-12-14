using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays Prefab information.")]
    internal class PrefabInfo : IProjectExtensionComponent
    {
        public int Priority => 0;

        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect)
        {
            if(ProjectExtension.isIconGUI || extension != ".prefab" || ProjectExtension.GUIDToObject(guid) is not Object prefab || !prefab) return;

            var type = PrefabUtility.GetPrefabAssetType(prefab);
            var label = type.ToString();
            if(type == PrefabAssetType.Variant)
            {
                var parent = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
                if(parent) label = $"Variant: {parent.name}";
            }
            GUIHelper.DrawLabel(ref currentRect, label);
        }
    }
}
