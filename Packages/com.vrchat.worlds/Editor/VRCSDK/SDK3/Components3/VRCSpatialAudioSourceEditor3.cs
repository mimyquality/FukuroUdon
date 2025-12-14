#if VRC_SDK_VRCSDK3

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VRCSpatialAudioSource = VRC.SDK3.Components.VRCSpatialAudioSource;

namespace VRC.SDK3.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRCSpatialAudioSource))]
    public class VRCSpatialAudioSourceEditor3 : VRCInspectorBase
    {

        private const string SHOW_ADVANCED_OPTIONS_KEY = "VRC.SDK3.Components.VRCSpatialAudioSource.ShowAdvancedOptions";
        
        private SerializedProperty propGain;
        private SerializedProperty propNear;
        private SerializedProperty propFar;
        private SerializedProperty propVolumetricRadius;
        private SerializedProperty propEnableSpatialization;
        private SerializedProperty propUseAudioSourceVolumeCurve;

        private PropertyField fieldEnableSpatialization;
        private PropertyField fieldUseAudioSourceVolumeCurve;
        
        private VRCSpatialAudioSource script;
        private AudioSource source;
        
        
        private void OnEnable()
        {
            script = (VRCSpatialAudioSource)target;
            
            source = script.GetComponent<AudioSource>();
            
            propGain = serializedObject.FindProperty(nameof(VRCSpatialAudioSource.Gain));
            propNear = serializedObject.FindProperty(nameof(VRCSpatialAudioSource.Near));
            propFar = serializedObject.FindProperty(nameof(VRCSpatialAudioSource.Far));
            propVolumetricRadius = serializedObject.FindProperty(nameof(VRCSpatialAudioSource.VolumetricRadius));
            propEnableSpatialization = serializedObject.FindProperty(nameof(VRCSpatialAudioSource.EnableSpatialization));
            propUseAudioSourceVolumeCurve = serializedObject.FindProperty(nameof(VRCSpatialAudioSource.UseAudioSourceVolumeCurve));
        }

        public override void BuildInspectorGUI()
        {
            base.BuildInspectorGUI();

            AddField(propGain);
            AddField(propFar);

            Foldout foldout = AddKeyedFoldout("Advanced Options", SHOW_ADVANCED_OPTIONS_KEY);
            foldout.Add(AddField(propNear));
            foldout.Add(AddField(propVolumetricRadius));

            fieldEnableSpatialization = AddField(propEnableSpatialization);
            fieldEnableSpatialization.RegisterValueChangeCallback(EnableSpatializationCallback);
            foldout.Add(fieldEnableSpatialization);

            fieldUseAudioSourceVolumeCurve = AddField(propUseAudioSourceVolumeCurve);
            foldout.Add(fieldUseAudioSourceVolumeCurve);
            
            UseAudioSourceVolumeCurveChanged();
        }

        private void EnableSpatializationCallback(SerializedPropertyChangeEvent evt) => UseAudioSourceVolumeCurveChanged();

        private void UseAudioSourceVolumeCurveChanged()
        {
            source.spatialize = propEnableSpatialization.boolValue;
            fieldUseAudioSourceVolumeCurve.SetVisible(propEnableSpatialization.boolValue);
        }
    }
}

#endif