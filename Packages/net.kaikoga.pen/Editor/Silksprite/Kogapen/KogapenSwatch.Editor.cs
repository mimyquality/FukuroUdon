using UdonSharpEditor;
using UnityEditor;
using static Silksprite.Kogapen.KogapenEditorUtils;

namespace Silksprite.Kogapen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KogapenSwatch))]
    public class KogapenSwatchEditor : Editor
    {
        SerializedProperty _serializedColor;
        SerializedProperty _serializedSwatchMesh;
        SerializedProperty _serializedLineRenderer;

        void OnEnable()
        {
            _serializedColor = serializedObject.FindProperty(nameof(KogapenSwatch.color));
            _serializedSwatchMesh = serializedObject.FindProperty(nameof(KogapenSwatch.swatchMesh));
            _serializedLineRenderer = serializedObject.FindProperty(nameof(KogapenSwatch.lineRenderer));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            Header("Config");
            EditorGUILayout.PropertyField(_serializedColor);

            Header("Unity References");
            EditorGUILayout.PropertyField(_serializedSwatchMesh);
            EditorGUILayout.PropertyField(_serializedLineRenderer);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
