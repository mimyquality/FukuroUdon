using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using static Silksprite.Kogapen.KogapenEditorUtils;

namespace Silksprite.Kogapen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KogapenStream))]
    public class KogapenStreamEditor : Editor
    {
        KogapenStream _stream;

        void OnEnable()
        {
            _stream = (KogapenStream)target;
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            Header("Runtime Variables");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Sync", _stream.sync, typeof(KogapenSync), true);
                EditorGUILayout.IntField("Stream Id", _stream.streamId);
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Available in Play Mode only", MessageType.Info);
            }
        }
    }
}
