/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using System;
using UnityEditor;
using UdonSharpEditor;
using UnityEngine;

namespace MimyLab.FukuroUdon
{
    [CustomEditor(typeof(AdvancedWorldSettings))]
    public class AdvancedWorldSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.UpdateIfRequiredOrScript();

            var movement = serializedObject.FindProperty("_initializeMovement");
            //EditorGUILayout.PropertyField(movement);
            EditorGUILayout.Space();
            movement.boolValue =
                EditorGUILayout.ToggleLeft(movement.displayName, movement.boolValue, EditorStyles.boldLabel);
            if (movement.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_walkSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_strafeSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_runSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_jumpImpulse"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_gravityStrength"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_immobile"));
            }

            var pickups = serializedObject.FindProperty("_initializePickups");
            //EditorGUILayout.PropertyField(pickups);
            EditorGUILayout.Space();
            pickups.boolValue =
                EditorGUILayout.ToggleLeft(pickups.displayName, pickups.boolValue, EditorStyles.boldLabel);
            if (pickups.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_enablePickups"));
            }

            var playerVoice = serializedObject.FindProperty("_initializePlayerVoice");
            //EditorGUILayout.PropertyField(playerVoice);
            EditorGUILayout.Space();
            playerVoice.boolValue =
                EditorGUILayout.ToggleLeft(playerVoice.displayName, playerVoice.boolValue, EditorStyles.boldLabel);
            if (playerVoice.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_voiceGain"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_voiceDistanceNear"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_voiceDistanceFar"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_voiceVolumetricRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_voiceLowpass"));
            }

            var avatarAudio = serializedObject.FindProperty("_initializeAvatarAudio");
            //EditorGUILayout.PropertyField(avatarAudio);
            EditorGUILayout.Space();
            avatarAudio.boolValue =
                EditorGUILayout.ToggleLeft(avatarAudio.displayName, avatarAudio.boolValue, EditorStyles.boldLabel);
            if (avatarAudio.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_avatarAudioGain"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_avatarAudioDistanceNear"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_avatarAudioDistanceFar"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_avatarAudioVolumetricRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_avatarAudioForceSpatial"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("_avatarAudioCustomCurve"));
            }

            var avatarScaling = serializedObject.FindProperty("_initializeAvatarScaling");
            //EditorGUILayout.PropertyField(avatarScaling);
            EditorGUILayout.Space();
            avatarScaling.boolValue =
                EditorGUILayout.ToggleLeft(avatarScaling.displayName, avatarScaling.boolValue, EditorStyles.boldLabel);
            if (avatarScaling.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_allowManualAvatarScaling"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_manualAvatarScalingRange"));
            }

            EditorGUILayout.Space();
            var avatarEyeHeight = serializedObject.FindProperty("_initializeAvatarEyeHeight");
            EditorGUILayout.PropertyField(avatarEyeHeight);
            if (avatarEyeHeight.enumValueFlag > 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_avatarEyeHeightLimit"));
            }

            var screenCamera = serializedObject.FindProperty("_initializeScreenCameraSettings");
            //EditorGUILayout.PropertyField(screenCameraSettings);
            EditorGUILayout.Space();
            screenCamera.boolValue =
                EditorGUILayout.ToggleLeft(screenCamera.displayName, screenCamera.boolValue, EditorStyles.boldLabel);
            if (screenCamera.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenAllowHDR"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenDepthTextureMode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenUseOcclusionCulling"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenAllowMSAA"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenCullingMask"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenClearFlags"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenBackgroundColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenLayerCullSpherical"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_screenLayerCullDistances"));
            }

            var photoCamera = serializedObject.FindProperty("_initializePhotoCameraSettings");
            //EditorGUILayout.PropertyField(photoCamera);
            EditorGUILayout.Space();
            photoCamera.boolValue =
                EditorGUILayout.ToggleLeft(photoCamera.displayName, photoCamera.boolValue, EditorStyles.boldLabel);
            if (photoCamera.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoAllowHDR"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoDepthTextureMode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoUseOcclusionCulling"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoAllowMSAA"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoClearFlags"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoBackgroundColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoLayerCullSpherical"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_photoLayerCullDistances"));
            }

            var qualitySettings = serializedObject.FindProperty("_initializeQualitySettings");
            //EditorGUILayout.PropertyField(qualitySettings);
            EditorGUILayout.Space();
            qualitySettings.boolValue = EditorGUILayout.ToggleLeft(
                qualitySettings.displayName, qualitySettings.boolValue, EditorStyles.boldLabel);
            if (qualitySettings.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_realtimeReflectionProbes"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_shadowmaskMode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_shadowDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_shadowCascade2Split"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_shadowCascade4Split0"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_shadowCascade4Split1"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_shadowCascade4Split2"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}