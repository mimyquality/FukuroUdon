using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays the layer and tag of an object.")]
    internal class LayerAndTag : IHierarchyExtensionComponent
    {
        private static GUIStyle m_styleMiniText;
        private static GUIStyle StyleMiniText => m_styleMiniText != null ? m_styleMiniText : m_styleMiniText = new GUIStyle(EditorStyles.label){fontSize = (int)(EditorStyles.label.fontSize*0.75f)};
        private static GUIStyle m_styleSemiMiniText;
        private static GUIStyle StyleSemiMiniText => m_styleSemiMiniText != null ? m_styleSemiMiniText : m_styleSemiMiniText = new GUIStyle(EditorStyles.label){fontSize = (int)(EditorStyles.label.fontSize*0.85f)};

        public int Priority => 200;

        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect)
        {
            var layer = new GUIContent(LayerMask.LayerToName(gameObject.layer));
            var tag = new GUIContent(gameObject.tag);
            if (EditorToolboxSettings.instance.hierarchyLayerAndTagSideBySide)
            {
                var width = 128f;
                currentRect.x -= width;
                currentRect.width = width;
                var rectMini = currentRect;
                rectMini.width = currentRect.width * 0.5f;

                GUI.enabled = gameObject.layer != 0;
                GUI.Label(rectMini, layer, StyleSemiMiniText);
                rectMini.x += rectMini.width;

                GUI.enabled = gameObject.tag != "Untagged";
                GUI.Label(rectMini, tag, StyleSemiMiniText);
                GUI.enabled = true;
            }
            else
            {
                var width = 64f;
                //var width = EditorStyles.miniLabel.CalcSize(layer).x;
                currentRect.x -= width;
                currentRect.width = width;
                var rectMini = currentRect;
                rectMini.height = currentRect.height*0.6f;

                GUI.enabled = gameObject.layer != 0;
                GUI.Label(rectMini, layer, StyleMiniText);
                rectMini.y += currentRect.height*0.5f;

                GUI.enabled = gameObject.tag != "Untagged";
                GUI.Label(rectMini, tag, StyleMiniText);
                GUI.enabled = true;

                currentRect.x -= 8;
            }
        }
    }
}
