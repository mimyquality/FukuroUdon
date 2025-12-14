using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{
    internal partial class L10n : ScriptableSingleton<L10n>
    {
        // Layout helper
        public static float GetTextWidth(string key)
            => Common.GetTextWidth(G(key));

        public static float CalcHeight(string key, GUIStyle style)
            => style.CalcHeight(G(key), EditorGUIUtility.currentViewWidth - 10);

        public static float CalcHeight(string key, Texture tex, GUIStyle style)
            => style.CalcHeight(G(key, tex), EditorGUIUtility.currentViewWidth - 10);

        // LabelField
        public static void LabelField(string key, params GUILayoutOption[] options)
            => EditorGUILayout.LabelField(G(key), options);

        public static void LabelField(string[] key, params GUILayoutOption[] options)
            => EditorGUILayout.LabelField(G(key), options);

        public static void LabelField(string key, GUIStyle style, params GUILayoutOption[] options)
            => EditorGUILayout.LabelField(G(key), style, options);

        public static void LabelField(string[] key, GUIStyle style, params GUILayoutOption[] options)
            => EditorGUILayout.LabelField(G(key), style, options);

        public static void LabelField(Rect position, string key, GUIStyle style)
            => EditorGUI.LabelField(position, G(key), style);

        public static void LabelField(Rect position, string[] key, GUIStyle style)
            => EditorGUI.LabelField(position, G(key), style);

        // Button
        public static bool Button(string key, params GUILayoutOption[] options)
            => GUILayout.Button(G(key), options);

        public static bool Button(string[] key, params GUILayoutOption[] options)
            => GUILayout.Button(G(key), options);

        public static bool Button(Rect rect, string key)
            => GUI.Button(rect, G(key));

        public static bool ButtonLimited(string key)
            => GUILayout.Button(G(key), GUILayout.MaxWidth(GetTextWidth(key)+16));

        // Toggle
        public static bool Toggle(string key, bool value, params GUILayoutOption[] options)
            => GUILayout.Toolbar(value ? 0 : -1, new[]{G(key)}, options) != -1;

        public static bool ToggleLeft(string key, bool value, params GUILayoutOption[] options)
            => EditorGUILayout.ToggleLeft(G(key), value, options);

        // Other
        public static int IntField(string key, int value, params GUILayoutOption[] options)
            => EditorGUILayout.IntField(G(key), value, options);

        public static float FloatField(string key, float value, params GUILayoutOption[] options)
            => EditorGUILayout.FloatField(G(key), value, options);

        public static Vector2 Vector2Field(string key, Vector2 value, params GUILayoutOption[] options)
            => EditorGUILayout.Vector2Field(G(key), value, options);

        public static string TextField(string key, string value, params GUILayoutOption[] options)
            => EditorGUILayout.TextField(G(key), value, options);

        public static Enum EnumPopup(string key, Enum selected, params GUILayoutOption[] options)
            => EditorGUILayout.EnumPopup(G(key), selected, options);

        public static Object ObjectField(string key, Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
            => EditorGUILayout.ObjectField(G(key), obj, objType, allowSceneObjects, options);

        public static Object ObjectField(string[] key, Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
            => EditorGUILayout.ObjectField(G(key), obj, objType, allowSceneObjects, options);

        public static bool PropertyField(SerializedProperty property, params GUILayoutOption[] options)
            => EditorGUILayout.PropertyField(property, G(property.displayName, property.tooltip), options);

        public static bool PropertyField(Rect position, SerializedProperty property)
            => EditorGUI.PropertyField(position, property, G(property.displayName, property.tooltip));
    }
}
