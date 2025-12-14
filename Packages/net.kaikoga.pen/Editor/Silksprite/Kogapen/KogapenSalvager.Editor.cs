using UdonSharpEditor;
using UnityEditor;
using static Silksprite.Kogapen.KogapenEditorUtils;

namespace Silksprite.Kogapen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KogapenSalvager))]
    public class KogapenSalvagerEditor : Editor
    {
        SerializedProperty _serializedSync;
        SerializedProperty _serializedMaxPacketStrokes;

        void OnEnable()
        {
            _serializedSync = serializedObject.FindProperty(nameof(KogapenSalvager.sync));
            _serializedMaxPacketStrokes = serializedObject.FindProperty(nameof(KogapenSalvager.maxPacketStrokes));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            EditorGUILayout.PropertyField(_serializedSync);

            Header("Advanced Settings");
            EditorGUILayout.PropertyField(_serializedMaxPacketStrokes);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
