using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays the child components of an object.")]
    internal class ChildrenDrawer : IHierarchyExtensionComponent
    {
        private const int ICON_SIZE_CHILD = 8;
        private static readonly Dictionary<GameObject, (Texture2D[],bool)> iconsMap = new();

        public int Priority => 1200;

        [InitializeOnLoadMethod] private static void Initialize() =>
            ObjectChangeEvents.changesPublished += (ref ObjectChangeEventStream stream) => iconsMap.Clear();

        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect)
        {
            if(!iconsMap.TryGetValue(gameObject, out var icons))
            {
                var components = gameObject.GetComponentsInChildren<Component>(true);
                icons.Item1 = components
                    .Where(c => c && c is not Transform && c.gameObject != gameObject)
                    .GroupBy(c => c.GetType())
                    .OrderBy(c => c.Key.FullName)
                    .Select(c => AssetPreview.GetMiniThumbnail(c.First()))
                    .Distinct().ToArray();
                icons.Item2 = components.Any(c => !c);
                iconsMap[gameObject] = icons;
            }

            if(icons.Item1.Length > 0)
            {
                currentRect.x -= ICON_SIZE_CHILD * icons.Item1.Length;
                currentRect.width = ICON_SIZE_CHILD;
                var xmin = currentRect.x;

                foreach(var icon in icons.Item1)
                {
                    GUI.Box(currentRect, icon, GUIStyle.none);
                    currentRect.x += ICON_SIZE_CHILD;
                }

                currentRect.x = xmin;
            }

            if(icons.Item2)
            {
                currentRect.x -= ICON_SIZE_CHILD;
                currentRect.width = ICON_SIZE_CHILD;
                GUI.Box(currentRect, HierarchyExtension.MissingScriptIcon(), GUIStyle.none);
            }
        }
    }
}
