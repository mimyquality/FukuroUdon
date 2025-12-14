using UdonSharpEditor;
using UnityEditor;
using static Silksprite.Kogapen.KogapenEditorUtils;

namespace Silksprite.Kogapen
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KogapenSlate))]
    public class KogapenSlateEditor : Editor
    {
        SerializedProperty _serializedSync;
        
        SerializedProperty _serializedSwatch;
        SerializedProperty _serializedColorCodeInput;
        SerializedProperty _serializedRedSlider;
        SerializedProperty _serializedGreenSlider;
        SerializedProperty _serializedBlueSlider;
        SerializedProperty _serializedRedBackground;
        SerializedProperty _serializedGreenBackground;
        SerializedProperty _serializedBlueBackground;

        void OnEnable()
        {
            _serializedSync = serializedObject.FindProperty(nameof(KogapenSlate.sync));
            _serializedSwatch = serializedObject.FindProperty(nameof(KogapenSlate.swatch));
            _serializedColorCodeInput = serializedObject.FindProperty(nameof(KogapenSlate.colorCodeInput));
            _serializedRedSlider = serializedObject.FindProperty(nameof(KogapenSlate.redSlider));
            _serializedGreenSlider = serializedObject.FindProperty(nameof(KogapenSlate.greenSlider));
            _serializedBlueSlider = serializedObject.FindProperty(nameof(KogapenSlate.blueSlider));
            _serializedRedBackground = serializedObject.FindProperty(nameof(KogapenSlate.redBackground));
            _serializedGreenBackground = serializedObject.FindProperty(nameof(KogapenSlate.greenBackground));
            _serializedBlueBackground = serializedObject.FindProperty(nameof(KogapenSlate.blueBackground));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            Header("Unity References");
            EditorGUILayout.PropertyField(_serializedSync);
            EditorGUILayout.PropertyField(_serializedSwatch);
            EditorGUILayout.PropertyField(_serializedColorCodeInput);
            EditorGUILayout.PropertyField(_serializedRedSlider);
            EditorGUILayout.PropertyField(_serializedGreenSlider);
            EditorGUILayout.PropertyField(_serializedBlueSlider);
            EditorGUILayout.PropertyField(_serializedRedBackground);
            EditorGUILayout.PropertyField(_serializedGreenBackground);
            EditorGUILayout.PropertyField(_serializedBlueBackground);
        }
    }
}
