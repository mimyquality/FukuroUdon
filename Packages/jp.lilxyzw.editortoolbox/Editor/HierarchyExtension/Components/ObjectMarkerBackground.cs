using jp.lilxyzw.editortoolbox.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Applies a background color to the ObjectMarker.")]
    internal class ObjectMarkerBackground : IHierarchyExtensionComponent
    {
        public int Priority => -1450;

        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect)
        {
            var marker = gameObject.GetComponent<ObjectMarker>();
            if(marker)
            {
                var rect = fullRect;
                if(marker.underline) rect.yMin = rect.yMax - 2;
                EditorGUI.DrawRect(rect, marker.color);
            }
        }
    }
}
