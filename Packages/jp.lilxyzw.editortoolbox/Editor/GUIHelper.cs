using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal static class GUIHelper
    {
        private static GUIStyle styleAssetLabel;
        private static GUIStyle StyleAssetLabel => styleAssetLabel ??= "AssetLabel";

        internal static void DrawLabel(ref Rect currentRect, string label)
        {
            currentRect.xMin += 4;
            currentRect.width = StyleAssetLabel.CalcSize(new GUIContent(label)).x;
            GUI.Label(currentRect, label, StyleAssetLabel);
            currentRect.xMin += currentRect.width;
        }

        // ドラッグで一括変更可能なボタン
        private static readonly int dToggleHash = "s_DToggleHash".GetHashCode();
        internal static bool DToggle(string usedFrom, string identifier, Rect position, bool value, int dir = 1)
        {
            var e = Event.current;
            if(e.type == EventType.Repaint)
            {
                int controlID = GUIUtility.GetControlID(dToggleHash, FocusType.Keyboard, position);
                EditorStyles.toggle.Draw(position, GUIContent.none, controlID, value, position.Contains(e.mousePosition));
            }
            return DButton(usedFrom, identifier, position, value, dir);
        }

        private static GUIStyle m_MiniLabel;
        private static GUIStyle MiniLabel => m_MiniLabel != null ? m_MiniLabel : m_MiniLabel = new GUIStyle(EditorStyles.miniLabel){fontStyle = FontStyle.Bold};
        private static Color m_PaleTextColor = default;
        private static Color PaleTextColor => m_PaleTextColor != default ? m_PaleTextColor : m_PaleTextColor = new Color(MiniLabel.normal.textColor.r, MiniLabel.normal.textColor.g, MiniLabel.normal.textColor.b, 0.1f);
        private static readonly int dToggleMiniLabelHash = "s_DToggleMiniLabelHash".GetHashCode();
        internal static bool DToggleMiniLabel(string label, string usedFrom, string identifier, Rect position, bool value, int dir = 1)
        {
            var e = Event.current;
            if(e.type == EventType.Repaint)
            {
                int controlID = GUIUtility.GetControlID(dToggleMiniLabelHash, FocusType.Keyboard, position);
                var color = MiniLabel.normal.textColor;
                if(!value) MiniLabel.normal.textColor = PaleTextColor;
                MiniLabel.Draw(position, new GUIContent(label), controlID, false, position.Contains(e.mousePosition));
                if(!value) MiniLabel.normal.textColor = color;
            }
            return DButton(usedFrom, identifier, position, value, dir);
        }

        // ドラッグで一括変更可能なボタン（本体）
        private static string dUsedFrom = "";
        private static float dStartPos = 0;
        private static readonly HashSet<string> dChangeds = new();
        private static bool DButton(string usedFrom, string identifier, Rect position, bool value, int dir)
        {
            var e = Event.current;
            bool IsClicked()
            {
                return EditorToolboxSettings.instance.hierarchyMouseButton.HasFlag(MouseButton.Left) && e.button == 0 ||
                EditorToolboxSettings.instance.hierarchyMouseButton.HasFlag(MouseButton.Right) && e.button == 1 ||
                EditorToolboxSettings.instance.hierarchyMouseButton.HasFlag(MouseButton.Middle) && e.button == 2;
            }
            if((e.type == EventType.MouseUp || e.type == EventType.MouseLeaveWindow) && dUsedFrom != "")
            {
                dUsedFrom = "";
            }
            else if(e.type == EventType.MouseDown && position.Contains(e.mousePosition) && IsClicked())
            {
                dUsedFrom = usedFrom;
                dStartPos = e.mousePosition[dir];
                dChangeds.Clear();
                dChangeds.Add(identifier);
                GUI.changed = true;
                value = !value;
                e.Use();
            }
            else if(e.type == EventType.MouseDrag && dUsedFrom == usedFrom && !dChangeds.Contains(identifier))
            {
                var pos = e.mousePosition[dir];
                var min = dir == 1 ? position.yMin : position.xMin;
                var max = dir == 1 ? position.yMax : position.xMax;
                if((min > dStartPos && pos > min) || (max < dStartPos && pos < max))
                {
                    dChangeds.Add(identifier);
                    GUI.changed = true;
                    value = !value;
                }
            }
            return value;
        }
    }
}
