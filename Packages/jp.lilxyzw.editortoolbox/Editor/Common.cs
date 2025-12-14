using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal static class Common
    {
        internal const string MENU_HEAD = "Tools/lilEditorToolbox/";
        internal static readonly string PREFERENCE_FOLDER = $"{UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder}/jp.lilxyzw";

        internal static bool SkipScan(Object obj)
        {
            return obj is GameObject ||
                // Skip - Component
                obj is Transform ||
                // Skip - Asset
                obj is Mesh ||
                obj is Texture ||
                obj is Shader ||
                obj is TextAsset ||
                obj.GetType() == typeof(Object);
        }

        internal static string ToDisplayName(string name)
            => string.Concat(name.Select(c => char.IsUpper(c) ? $" {c}" : $"{c}")).TrimStart();

        internal static float GetTextWidth(string text) => GetTextWidth(GetContent(text));
        internal static float GetTextWidth(GUIContent content) => EditorStyles.label.CalcSize(content).x;

        private static Dictionary<string, GUIContent> tempContents = new();
        private static GUIContent GetContent(string text)
        {
            if(tempContents.TryGetValue(text, out var content)) return content;
            return tempContents[text] = new GUIContent(text);
        }
    }

    internal class ToggleLeftAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(ToggleLeftAttribute))]
    internal class ToggleLeftDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var boolValue = EditorGUI.ToggleLeft(position, label, property.boolValue);
            if(EditorGUI.EndChangeCheck()) property.boolValue = boolValue;
        }
    }
}
