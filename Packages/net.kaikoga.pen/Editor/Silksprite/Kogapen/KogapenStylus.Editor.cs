using System.Linq;
using UnityEditor;
using UnityEngine;
using static Silksprite.Kogapen.KogapenEditorUtils;

namespace Silksprite.Kogapen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KogapenStylus))]
    public class KogapenStylusEditor : Editor
    {
        SerializedProperty _serializedSync;
        SerializedProperty _serializedPickup;
        SerializedProperty _serializedStylusKind;
        SerializedProperty _serializedPickerKind;
        SerializedProperty _serializedStrokePreviewKind;
        SerializedProperty _serializedEraserTapMatchPlayer;
        SerializedProperty _serializedEraserTapMatchColor;
        SerializedProperty _serializedEraserSwipeMatchPlayer;
        SerializedProperty _serializedEraserSwipeMatchColor;
        SerializedProperty _serializedPenMesh;
        SerializedProperty _serializedTrailRenderer;
        SerializedProperty _serializedColor;
        SerializedProperty _serializedEraserMesh;
        SerializedProperty _serializedEraserCollider;
        SerializedProperty _serializedRespawnMethodName;

        bool _hasSyncComponent;

        void OnEnable()
        {
            _serializedSync = serializedObject.FindProperty(nameof(KogapenStylus.sync));
            _serializedPickup = serializedObject.FindProperty(nameof(KogapenStylus.pickup));
            _serializedStylusKind = serializedObject.FindProperty(nameof(KogapenStylus.stylusKind));
            _serializedPickerKind = serializedObject.FindProperty(nameof(KogapenStylus.pickerKind));
            _serializedStrokePreviewKind = serializedObject.FindProperty(nameof(KogapenStylus.strokePreviewKind));
            _serializedEraserTapMatchPlayer = serializedObject.FindProperty(nameof(KogapenStylus.eraserTapMatchPlayer));
            _serializedEraserTapMatchColor = serializedObject.FindProperty(nameof(KogapenStylus.eraserTapMatchColor));
            _serializedEraserSwipeMatchPlayer = serializedObject.FindProperty(nameof(KogapenStylus.eraserSwipeMatchPlayer));
            _serializedEraserSwipeMatchColor = serializedObject.FindProperty(nameof(KogapenStylus.eraserSwipeMatchColor));
            _serializedPenMesh = serializedObject.FindProperty(nameof(KogapenStylus.penMesh));
            _serializedColor = serializedObject.FindProperty(nameof(KogapenStylus.color));
            _serializedTrailRenderer = serializedObject.FindProperty(nameof(KogapenStylus.trailRenderer));
            _serializedEraserMesh = serializedObject.FindProperty(nameof(KogapenStylus.eraserMesh));
            _serializedEraserCollider = serializedObject.FindProperty(nameof(KogapenStylus.eraserCollider));
            _serializedRespawnMethodName = serializedObject.FindProperty(nameof(KogapenStylus.respawnMethodName));

            _hasSyncComponent = ((KogapenStylus)target).GetComponents<Component>()
                .Any(component => component.GetType().Name.Contains("Sync"));
        }

        public override void OnInspectorGUI()
        {
            if (DrawAutoSyncTypeUdonSharpBehaviourHeader(target)) return;

            EditorGUILayout.PropertyField(_serializedSync);

            Header("Config");
            EditorGUILayout.PropertyField(_serializedColor);
            EditorGUILayout.PropertyField(_serializedStylusKind);
            EditorGUILayout.PropertyField(_serializedPickerKind);
            if (_hasSyncComponent ? _serializedPickerKind.enumValueIndex == (int)KogapenStylusPickerKind.Local :  _serializedPickerKind.enumValueIndex == (int)KogapenStylusPickerKind.Global)
            {
                var syncWarningMessage = _hasSyncComponent ?
                    "This stylus looks like global but picker kind is Local. Stroke color may get out of sync when picker is used." :
                    "This stylus looks like local but picker kind is Global. Stroke color may get out of sync when picker is used.";
                EditorGUILayout.HelpBox(syncWarningMessage, MessageType.Warning);
            }
            EditorGUILayout.PropertyField(_serializedStrokePreviewKind);
            if (!_hasSyncComponent && _serializedStrokePreviewKind.enumValueIndex == (int)KogapenStrokePreviewKind.Trail)
            {
                var previewWarningMessage = "This stylus looks like local but stroke preview kind is Trail. Stroke preview may not work.";
                EditorGUILayout.HelpBox(previewWarningMessage, MessageType.Warning);
            }
            EditorGUILayout.PropertyField(_serializedEraserTapMatchPlayer);
            EditorGUILayout.PropertyField(_serializedEraserTapMatchColor);
            EditorGUILayout.PropertyField(_serializedEraserSwipeMatchPlayer);
            EditorGUILayout.PropertyField(_serializedEraserSwipeMatchColor);

            Header("Unity References");
            EditorGUILayout.PropertyField(_serializedPickup);
            EditorGUILayout.PropertyField(_serializedPenMesh);
            EditorGUILayout.PropertyField(_serializedTrailRenderer);
            EditorGUILayout.PropertyField(_serializedEraserMesh);
            EditorGUILayout.PropertyField(_serializedEraserCollider);
            EditorGUILayout.PropertyField(_serializedRespawnMethodName);

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                Header("Runtime Variables");
                using (new EditorGUI.DisabledScope(true))
                {
                    // NOTE: not CopyUdonToProxy() because runtime only variables are not serialized
                    var stylus = (KogapenStylus)target;
                    var localIsUsing = (bool)stylus.GetProgramVariable(nameof(KogapenStylus.localIsUsing));
                    var localIsDamper = (bool)stylus.GetProgramVariable(nameof(KogapenStylus.localIsDamper));
                    var localOrientation = (KogapenOrientation)stylus.GetProgramVariable(nameof(KogapenStylus.localOrientation));
                    var localIsStylusDown = (bool)stylus.GetProgramVariable(nameof(KogapenStylus.localIsStylusDown));
                    var localEraserMode = (KogapenEraserMode)stylus.GetProgramVariable(nameof(KogapenStylus.localEraserMode));
                    EditorGUILayout.Toggle(nameof(KogapenStylus.localIsUsing), localIsUsing);
                    EditorGUILayout.Toggle(nameof(KogapenStylus.localIsDamper), localIsDamper);
                    EditorGUILayout.EnumPopup(nameof(KogapenStylus.localOrientation), localOrientation);
                    EditorGUILayout.Toggle(nameof(KogapenStylus.localIsStylusDown), localIsStylusDown);
                    EditorGUILayout.EnumPopup(nameof(KogapenStylus.localEraserMode), localEraserMode);
                }
            }

        }
    }
}
