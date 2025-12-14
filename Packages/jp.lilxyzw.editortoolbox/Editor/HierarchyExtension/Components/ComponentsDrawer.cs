using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Displays the object's components. You can turn components on or off by clicking their icons.")]
    internal class ComponentsDrawer : IHierarchyExtensionComponent
    {
        private const int ICON_SIZE = 16;

        public int Priority => 1100;

        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect)
        {
            var components = gameObject.GetComponents<Component>().Where(c => c is not Transform).ToArray();
            if(components.Length > 0)
            {
                var iconCount = components.Length;
                bool isTooMany = currentRect.x - ICON_SIZE * iconCount < fullRect.x + 100;
                if(isTooMany)
                {
                    int count = (int)((currentRect.x - fullRect.x - 100) / ICON_SIZE)-1;
                    if(count < 0) count = 0;
                    if(count > iconCount) count = iconCount;
                    Array.Resize(ref components, count);
                    iconCount = count + 1;
                }
                currentRect.x -= ICON_SIZE * iconCount;
                currentRect.width = ICON_SIZE;
                var xmin = currentRect.x;

                foreach(var component in components)
                {
                    switch(component)
                    {
                        case Renderer c: GUI.enabled = c.enabled; break;
                        case Behaviour c: GUI.enabled = c.enabled; break;
                        case Collider c: GUI.enabled = c.enabled; break;
                    }
                    if(component) GUI.Box(currentRect, AssetPreview.GetMiniThumbnail(component), GUIStyle.none);
                    else          GUI.Box(currentRect, HierarchyExtension.MissingScriptIcon(), GUIStyle.none);
                    GUI.enabled = true;

                    if((component is Renderer || component is Behaviour || component is Collider) && CheckMouseDown(currentRect))
                    {
                        using var so = new SerializedObject(component);
                        using var m_Enabled = so.FindProperty("m_Enabled");
                        m_Enabled.boolValue = !m_Enabled.boolValue;
                        so.ApplyModifiedProperties();
                    }

                    currentRect.x += ICON_SIZE;
                }

                if(isTooMany)
                {
                    EditorGUI.LabelField(currentRect, "...");
                    currentRect.x += ICON_SIZE;
                }

                currentRect.x = xmin - 8;
            }
        }

        private bool CheckMouseDown(Rect rect)
        {
            var e = Event.current;
            if(!rect.Contains(e.mousePosition) || EditorToolboxSettings.instance.hierarchyMouseButton == MouseButton.None) return false;
            EditorGUI.DrawRect(rect, new Color(1,1,1,0.5f));
            if(e.type != EventType.MouseDown) return false;

            if(
                EditorToolboxSettings.instance.hierarchyMouseButton.HasFlag(MouseButton.Left) && e.button == 0 ||
                EditorToolboxSettings.instance.hierarchyMouseButton.HasFlag(MouseButton.Right) && e.button == 1 ||
                EditorToolboxSettings.instance.hierarchyMouseButton.HasFlag(MouseButton.Middle) && e.button == 2
            )
            {
                e.Use();
                return true;
            }
            return false;
        }
    }
}
