using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    [FilePath("./jp.lilxyzw.editortoolbox.AssetMarkerData.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class AssetMarkerData : ScriptableSingleton<AssetMarkerData>
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.update -= AssetMarkerButton.AddButton;
            EditorApplication.update += AssetMarkerButton.AddButton;
            EditorToolboxSettingsEditor.update -= AssetMarkerButton.UpdateButton;
            EditorToolboxSettingsEditor.update += AssetMarkerButton.UpdateButton;
        }

        [Serializable]
        internal class AssetMarkerColor
        {
            public string guid;
            public Color color;
        }
        public List<AssetMarkerColor> colors = new();
        private Dictionary<string, Color> colorDic;

        public bool TryGetColor(string guid, out Color color)
        {
            colorDic ??= colors.ToDictionary(c => c.guid, c => c.color);
            return colorDic.TryGetValue(guid, out color);
        }

        [MenuItem("Assets/CheckDic")]
        private static void CheckDic()
        {
            Debug.Log(string.Join("\r\n", instance.colors.Select(c => c.guid)));
            Debug.Log(string.Join("\r\n", instance.colorDic.Select(c => c.Key)));
        }

        private class AssetMarkerWindow : EditorWindow
        {
            internal static void Init() => CreateInstance<AssetMarkerWindow>().ShowAuxWindow();
            private static readonly Color presetColor0 = Color.HSVToRGB(0.05f, 0.7f, 0.9f);
            private static readonly Color presetColor1 = Color.HSVToRGB(0.15f, 0.7f, 0.9f);
            private static readonly Color presetColor2 = Color.HSVToRGB(0.25f, 0.7f, 0.9f);
            private static readonly Color presetColor3 = Color.HSVToRGB(0.35f, 0.7f, 0.9f);
            private static readonly Color presetColor4 = Color.HSVToRGB(0.45f, 0.7f, 0.9f);
            private static readonly Color presetColor5 = Color.HSVToRGB(0.55f, 0.7f, 0.9f);
            private static readonly Color presetColor6 = Color.HSVToRGB(0.65f, 0.7f, 0.9f);
            private static readonly Color presetColor7 = Color.HSVToRGB(0.75f, 0.7f, 0.9f);
            private static readonly Color presetColor8 = Color.HSVToRGB(0.85f, 0.7f, 0.9f);
            private static readonly Color presetColor9 = Color.HSVToRGB(0.95f, 0.7f, 0.9f);

            void OnGUI()
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button(EditorGUIUtility.IconContent("FolderEmpty Icon"), EditorStyles.label, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight), GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight)))
                {
                    Remove();
                }
                DrawColorButton(presetColor0);
                DrawColorButton(presetColor1);
                DrawColorButton(presetColor2);
                DrawColorButton(presetColor3);
                DrawColorButton(presetColor4);
                DrawColorButton(presetColor5);
                DrawColorButton(presetColor6);
                DrawColorButton(presetColor7);
                DrawColorButton(presetColor8);
                DrawColorButton(presetColor9);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            private static GUIContent iconContent = null;
            private void DrawColorButton(Color color)
            {
                iconContent ??= new GUIContent(GraphicUtils.ProcessTexture("Hidden/_lil/IconMaker", EditorGUIUtility.IconContent("Folder Icon").image));
                var colorB = GUI.color;
                GUI.color = color;
                var pressed = GUILayout.Button(iconContent, EditorStyles.label, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight), GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
                GUI.color = colorB;
                if(pressed) Apply(color);
            }

            private void Apply(Color color)
            {
                var cols = Selection.assetGUIDs.Select(g => new AssetMarkerColor{guid = g, color = color});
                instance.colors = instance.colors.Where(c => !Selection.assetGUIDs.Contains(c.guid)).Union(cols).ToList();
                foreach(var c in cols) instance.colorDic[c.guid] = c.color;
                instance.Save(true);
                EditorApplication.RepaintProjectWindow();
            }

            private void Remove()
            {
                instance.colors = instance.colors.Where(c => !Selection.assetGUIDs.Contains(c.guid)).ToList();
                instance.colorDic = instance.colorDic.Where(c => !Selection.assetGUIDs.Contains(c.Key)).ToDictionary(c => c.Key, c => c.Value);
                instance.Save(true);
                EditorApplication.RepaintProjectWindow();
            }
        }

        private class AssetMarkerButton
        {
            private static ToolbarButton button;
            private static Texture tex = EditorGUIUtility.IconContent("Favorite").image;

            internal static void AddButton()
            {
                if(button == null) InitButton();
                ProjectToolbarExtension.Add(button);
                EditorApplication.update -= AddButton;
            }

            private static void InitButton()
            {
                button = new();
                var icon = new Image{image = tex};
                button.Add(icon);
                button.clicked += () => AssetMarkerWindow.Init();
                button.style.width = 24;
                UpdateButton();
            }

            internal static void UpdateButton()
            {
                if(button == null)
                {
                    InitButton();
                    return;
                }
                button.style.display = EditorToolboxSettings.instance.projectComponents.Any(c => c == typeof(AssetMarker).FullName) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
