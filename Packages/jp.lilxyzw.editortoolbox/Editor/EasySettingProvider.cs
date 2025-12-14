using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    public abstract class EasySettingProvider : SettingsProvider
    {
        public abstract ScriptableObject SO { get; }
        private Editor _editor;

        public EasySettingProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords){}
        public static SettingsProvider Create(string path) => new EditorToolboxSettingsProvider($"Preferences/{path}", SettingsScope.User, null);

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            SO.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            Editor.CreateCachedEditor(SO, null, ref _editor);
        }

        public override void OnGUI(string searchContext) => _editor.OnInspectorGUI();
    }
}
