using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using static Silksprite.Kogapen.KogapenEditorUtils;

namespace Silksprite.Kogapen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KogapenSync))]
    public class KogapenSyncEditor : Editor
    {
        SerializedProperty _serializedSyncId;
        SerializedProperty _serializedStreams;
        SerializedProperty _serializedSpools;
        SerializedProperty _serializedLocalStrokesContainer;
        SerializedProperty _serializedStrokesContainer;
        SerializedProperty _serializedStrokePrefab;
        SerializedProperty _serializedStrokeMaterialPC;
        SerializedProperty _serializedStrokeMaterialQuest;
        SerializedProperty _serializedStrokePCIsRoundedTrail;
        SerializedProperty _serializedStrokeDefaultWidth;
        SerializedProperty _serializedStrokeLayerMask;
        SerializedProperty _serializedDamperLayerMask;

        void OnEnable()
        {
            if (FindObjectsOfType<KogapenSync>().Length > 1)
            {
                _serializedSyncId = serializedObject.FindProperty(nameof(KogapenSync.syncId));
            }
            _serializedStreams = serializedObject.FindProperty(nameof(KogapenSync.streams));
            _serializedSpools = serializedObject.FindProperty(nameof(KogapenSync.spools));
            _serializedLocalStrokesContainer = serializedObject.FindProperty(nameof(KogapenSync.localStrokesContainer));
            _serializedStrokesContainer = serializedObject.FindProperty(nameof(KogapenSync.strokesContainer));
            _serializedStrokePrefab = serializedObject.FindProperty(nameof(KogapenSync.strokePrefab));
            _serializedStrokeMaterialPC = serializedObject.FindProperty(nameof(KogapenSync.strokeMaterialPC));
            _serializedStrokeMaterialQuest = serializedObject.FindProperty(nameof(KogapenSync.strokeMaterialQuest));
            _serializedStrokePCIsRoundedTrail = serializedObject.FindProperty(nameof(KogapenSync.strokePCIsRoundedTrail));
            _serializedStrokeDefaultWidth = serializedObject.FindProperty(nameof(KogapenSync.strokeDefaultWidth));
            _serializedStrokeLayerMask = serializedObject.FindProperty(nameof(KogapenSync.strokeLayerMask));
            _serializedDamperLayerMask = serializedObject.FindProperty(nameof(KogapenSync.damperLayerMask));
        }

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            if (_serializedSyncId != null) EditorGUILayout.PropertyField(_serializedSyncId);

            Header("Stroke Config");
            EditorGUILayout.PropertyField(_serializedStrokeMaterialPC);
            EditorGUILayout.PropertyField(_serializedStrokeMaterialQuest);
            EditorGUILayout.PropertyField(_serializedStrokePCIsRoundedTrail);
            EditorGUILayout.PropertyField(_serializedStrokeDefaultWidth);

            Header("Unity References");
            EditorGUILayout.PropertyField(_serializedStreams);
            EditorGUILayout.PropertyField(_serializedSpools);
            EditorGUILayout.PropertyField(_serializedLocalStrokesContainer);
            EditorGUILayout.PropertyField(_serializedStrokesContainer);
            EditorGUILayout.PropertyField(_serializedStrokePrefab);

            Header("Advanced Settings");
            EditorGUILayout.PropertyField(_serializedStrokeLayerMask);
            EditorGUILayout.PropertyField(_serializedDamperLayerMask);
            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                Header("Runtime Variables");
                using (new EditorGUI.DisabledScope(true))
                {
                    // NOTE: not CopyUdonToProxy() because runtime only variables are not serialized
                    var sync = (KogapenSync)target;
                    var leftStylus = sync.GetProgramVariable(nameof(KogapenSync.localLeftStylus)) as KogapenStylus;
                    var rightStylus = sync.GetProgramVariable(nameof(KogapenSync.localRightStylus)) as KogapenStylus;
                    EditorGUILayout.ObjectField(nameof(KogapenSync.localLeftStylus), leftStylus, typeof(KogapenStylus), true);
                    EditorGUILayout.ObjectField(nameof(KogapenSync.localLeftStylus), rightStylus, typeof(KogapenStylus), true);
                    EditorGUILayout.ObjectField(nameof(KogapenSync.myStream), sync.GetProgramVariable(nameof(KogapenSync.myStream)) as Object, typeof(KogapenStylus), true);
                }
            }
        }
    }
}
