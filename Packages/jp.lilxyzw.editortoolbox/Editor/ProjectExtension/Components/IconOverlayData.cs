using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{

    // ディレクトリが空だとUnityのバグでエラーになるので Assets/../ は必須
    [FilePath("Assets/../jp.lilxyzw.editortoolbox.IconOverlayData.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class IconOverlayData : ScriptableSingleton<IconOverlayData>
    {
        public IconOverlayItem[] items = new IconOverlayItem[]{};
        private static Dictionary<string, Texture2D> m_dic;
        internal static Dictionary<string, Texture2D> Dic => m_dic ??= InitDic();
        private static HashSet<Texture2D> tempIcons = new();

        private static Dictionary<string, Texture2D> InitDic()
        {
            return instance.items.Where(i => i.target && i.icon).GroupBy(i => i.target).ToDictionary(
                g => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(g.Key)),
                g => GetClipIcon(g.First()));
        }

        private static Texture2D GetClipIcon(IconOverlayItem item)
        {
            if(!item.clipBottom && !item.clipTop && item.scale == new Vector2(1,1) && item.offset == Vector2.zero) return item.icon;

            var objIcon = AssetPreview.GetMiniThumbnail(item.target);

            var material = new Material(Shader.Find("Hidden/_lil/CopyAlpha"));
            material.SetTexture("_TextureMain", item.icon);
            material.SetTexture("_TextureBack", objIcon);
            material.SetFloat("clipBottom", item.clipBottom ? 1 : 0);
            material.SetFloat("clipTop", item.clipTop ? 1 : 0);
            material.SetTextureScale("_TextureMain", item.scale);
            material.SetTextureOffset("_TextureMain", item.offset);
            var tex = GraphicUtils.ProcessTexture(material, null, objIcon.width, objIcon.height, true) as Texture2D;
            tempIcons.Add(tex);

            return tex;
        }

        [Serializable]
        public class IconOverlayItem : DirectElements
        {
            public Object target;
            public Texture2D icon;
            public bool clipBottom;
            public bool clipTop;
            public Vector2 scale = new(1,1);
            public Vector2 offset = Vector2.zero;
        }

        [CustomEditor(typeof(IconOverlayData))]
        public class EditorToolboxSettingsEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var wide = EditorGUIUtility.wideMode;
                EditorGUIUtility.wideMode = true;
                EditorGUI.BeginChangeCheck();
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.NextVisible(true); // m_Script
                while(iterator.NextVisible(false))
                    L10n.PropertyField(iterator);

                if(EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    instance.Save(true);

                    foreach(var icon in tempIcons) DestroyImmediate(icon);
                    tempIcons.Clear();
                    m_dic = InitDic();
                    EditorApplication.RepaintProjectWindow();
                }
                EditorGUIUtility.wideMode = wide;
            }
        }

        private class IconOverlayEditor : EditorWindow
        {
            private Editor _editor;
            public Vector2 scrollPos;

            [MenuItem(Common.MENU_HEAD + "Icon Overlay Editor")]
            static void Init() => GetWindow(typeof(IconOverlayEditor)).Show();

            void OnGUI()
            {
                if(!_editor)
                {
                    instance.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
                    Editor.CreateCachedEditor(instance, null, ref _editor);
                }
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                _editor.OnInspectorGUI();
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
