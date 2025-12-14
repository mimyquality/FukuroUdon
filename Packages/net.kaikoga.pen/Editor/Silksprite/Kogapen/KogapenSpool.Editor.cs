using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using static Silksprite.Kogapen.KogapenEditorUtils;

namespace Silksprite.Kogapen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KogapenSpool))]
    public class KogapenSpoolEditor : Editor
    {
        SerializedProperty _serializedStyli;
        SerializedProperty _serializedDynamicStyli;

        void OnEnable()
        {
            _serializedStyli = serializedObject.FindProperty(nameof(KogapenSpool.styli));
            _serializedDynamicStyli = serializedObject.FindProperty(nameof(KogapenSpool.dynamicStyli));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            Header("Unity References");
            EditorGUILayout.PropertyField(_serializedStyli);
            EditorGUILayout.PropertyField(_serializedDynamicStyli);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
