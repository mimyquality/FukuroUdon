﻿/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Rendering;

    [System.Flags]
    public enum AdvancedWorldSettingsInitializeEyeHeightType
    {
        Join = 1 << 0,
        AvatarChange = 1 << 1
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/Advanced World Settings")]
    [DefaultExecutionOrder(-1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AdvancedWorldSettings : UdonSharpBehaviour
    {
        [Header("Movement")]
        [SerializeField] private bool _initializeMovement = true;
        [SerializeField][Range(0f, 5f)] private float _walkSpeed = 2f;
        [SerializeField][Range(0f, 5f)] private float _strafeSpeed = 2f;
        [SerializeField][Range(0f, 10f)] private float _runSpeed = 4f;
        [SerializeField][Range(0f, 10f)] private float _jumpImpulse = 3f;
        [SerializeField][Range(0f, 10f)] private float _gravityStrength = 1f;
        [SerializeField] private bool _immobile = false;

        [Header("Pickups")]
        [SerializeField] private bool _initializePickups = true;
        [SerializeField] private bool _enablePickups = true;

        [Header("Player Voice")]
        [SerializeField] private bool _initializePlayerVoice = true;
        [SerializeField][Range(0f, 24f)] private float _voiceGain = 15f;
        [SerializeField][Range(0f, 999999f)] private float _voiceDistanceNear = 0f;
        [SerializeField][Range(0f, 999999f)] private float _voiceDistanceFar = 25f;
        [Space]
        [SerializeField][Range(0f, 999999f)] private float _voiceVolumetricRadius = 0f;
        [SerializeField] private bool _voiceLowpass = true;

        [Header("Avatar Audio")]
        [Tooltip("Note that this is compared to the audio source's settings, and the smaller value is used.")]
        [SerializeField] private bool _initializeAvatarAudio = true;
        [SerializeField][Range(0f, 10f)] private float _avatarAudioGain = 10f;
        [SerializeField][Min(0f)] private float _avatarAudioDistanceNear = 0f;
        [SerializeField][Min(0f)] private float _avatarAudioDistanceFar = 40f;
        [Space]
        [SerializeField][Min(0f)] private float _avatarAudioVolumetricRadius = 0f;
        [SerializeField] private bool _avatarAudioForceSpatial = false;
        [SerializeField] private bool _avatarAudioCustomCurve = false;

        [Header("Avatar Scaling")]
        [SerializeField] private bool _initializeAvatarScaling = true;
        [SerializeField] private bool _allowManualAvatarScaling = true;
        [SerializeField][Range(0.2f, 5f)] private float _avatarEyeHeightMinimum = 0.2f;
        [SerializeField][Range(0.2f, 5f)] private float _avatarEyeHeightMaximum = 5f;
        [Space]
        [Tooltip("When the button is checked, the Avatar Eye Height is Clamped at that point.")]
        [SerializeField][EnumFlag] private AdvancedWorldSettingsInitializeEyeHeightType _initializeAvatarEyeHight = 0;
        [SerializeField][Range(0.1f, 100f)] private float _avatarEyeHeightLowerLimit = 1.3f;
        [SerializeField][Range(0.1f, 100f)] private float _avatarEyeHeightUpperLimit = 1.3f;

        [Header("Screen Camera Settings")]
        [SerializeField] private bool _initializeScreenCameraSettings = false;
        [SerializeField] private bool _screenAllowHDR = false;
        [SerializeField] private DepthTextureMode _screenDepthTextureMode = DepthTextureMode.None;
        [SerializeField] private bool _screenUseOcclusionCulling = true;
        [SerializeField] private bool _screenAllowMSAA = true;
        [SerializeField] private CameraClearFlags _screenClearFlags = CameraClearFlags.Skybox;
        [Tooltip("The color to use when ClearFlags is set to SolidColor.")]
        [SerializeField] private Color _screenBackgroundColor = Color.black;
        [SerializeField] private bool _screenLayerCullSpherical = false;
        [Tooltip("A value of 0 for a layer means it will use the value of FarClipPlane.")]
        [SerializeField] private float[] _screenLayerCullDistances = new float[32];

        [Header("Photo Camera Settings")]
        [SerializeField] private bool _initializePhotoCameraSettings = false;
        [SerializeField] private bool _photoAllowHDR = false;
        [SerializeField] private DepthTextureMode _photoDepthTextureMode = DepthTextureMode.None;
        [SerializeField] private bool _photoUseOcclusionCulling = true;
        [SerializeField] private bool _photoAllowMSAA = true;
        [SerializeField] private CameraClearFlags _photoClearFlags = CameraClearFlags.Skybox;
        [Tooltip("The color to use when ClearFlags is set to SolidColor.")]
        [SerializeField] private Color _photoBackgroundColor = Color.black;
        [SerializeField] private bool _photoLayerCullSpherical = false;
        [Tooltip("A value of 0 for a layer means it will use the value of FarClipPlane.")]
        [SerializeField] private float[] _photoLayerCullDistances = new float[32];

        [Header("Quality Settings")]
        [SerializeField] private bool _initializeQualitySettings = false;
        [SerializeField][Range(0.1f, 10000f)] private float _shadowDistance = 50.0f;


        private bool _hasAvatarChanged = false;
        private bool _hasFirstAvatarChanged = false;

        private void Start()
        {
            if (_initializeScreenCameraSettings)
            {
                var screenCamera = VRCCameraSettings.ScreenCamera;
                screenCamera.AllowHDR = _screenAllowHDR;
                screenCamera.DepthTextureMode = _screenDepthTextureMode;
                screenCamera.UseOcclusionCulling = _screenUseOcclusionCulling;
                screenCamera.AllowMSAA = _screenAllowMSAA;
                screenCamera.ClearFlags = _screenClearFlags;
                screenCamera.BackgroundColor = _screenBackgroundColor;
                screenCamera.LayerCullSpherical = _screenLayerCullSpherical;
                screenCamera.LayerCullDistances = _screenLayerCullDistances;
            }

            if (_initializePhotoCameraSettings &&
                Utilities.IsValid(VRCCameraSettings.PhotoCamera))
            {
                var photoCamera = VRCCameraSettings.PhotoCamera;
                photoCamera.AllowHDR = _photoAllowHDR;
                photoCamera.DepthTextureMode = _photoDepthTextureMode;
                photoCamera.UseOcclusionCulling = _photoUseOcclusionCulling;
                photoCamera.AllowMSAA = _photoAllowMSAA;
                photoCamera.ClearFlags = _photoClearFlags;
                photoCamera.BackgroundColor = _photoBackgroundColor;
                photoCamera.LayerCullSpherical = _photoLayerCullSpherical;
                photoCamera.LayerCullDistances = _photoLayerCullDistances;
            }

            if (_initializeQualitySettings)
            {
                VRCQualitySettings.SetShadowDistance(_shadowDistance);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (_initializeMovement)
                {
                    player.SetWalkSpeed(_walkSpeed);
                    player.SetStrafeSpeed(_strafeSpeed);
                    player.SetRunSpeed(_runSpeed);
                    player.SetJumpImpulse(_jumpImpulse);
                    player.SetGravityStrength(_gravityStrength);
                    player.Immobilize(_immobile);
                }

                if (_initializePickups)
                {
                    player.EnablePickups(_enablePickups);
                }

                if (_initializeAvatarScaling)
                {
                    player.SetManualAvatarScalingAllowed(_allowManualAvatarScaling);
                    player.SetAvatarEyeHeightMinimumByMeters(_avatarEyeHeightMinimum);
                    player.SetAvatarEyeHeightMaximumByMeters(_avatarEyeHeightMaximum);
                }
            }

            if (_initializePlayerVoice)
            {
                player.SetVoiceGain(_voiceGain);
                player.SetVoiceDistanceNear(_voiceDistanceNear);
                player.SetVoiceDistanceFar(_voiceDistanceFar);
                player.SetVoiceVolumetricRadius(_voiceVolumetricRadius);
                player.SetVoiceLowpass(_voiceLowpass);
            }

            if (_initializeAvatarAudio)
            {
                player.SetAvatarAudioGain(_avatarAudioGain);
                player.SetAvatarAudioNearRadius(_avatarAudioDistanceNear);
                player.SetAvatarAudioFarRadius(_avatarAudioDistanceFar);
                player.SetAvatarAudioVolumetricRadius(_avatarAudioVolumetricRadius);
                player.SetAvatarAudioForceSpatial(_avatarAudioForceSpatial);
                player.SetAvatarAudioCustomCurve(_avatarAudioCustomCurve);
            }
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _hasAvatarChanged = true;

                // 同じ目の高さのアバターに変更した場合はOnAvatarEyeHeightChanged()が発火しない
                SendCustomEventDelayedSeconds(nameof(_CloseAvatarChangeProcessing), 0.2f);
            }
        }

        public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float prevEyeHeightAsMeters)
        {
            if (!player.isLocal) { return; }
            if (!_hasAvatarChanged) { return; }

            if (_hasFirstAvatarChanged)
            {
                if (((int)_initializeAvatarEyeHight & (int)AdvancedWorldSettingsInitializeEyeHeightType.AvatarChange) > 0)
                {
                    ClampAvatarEyeHeight(player);
                }
            }
            else
            {
                if (((int)_initializeAvatarEyeHight & (int)AdvancedWorldSettingsInitializeEyeHeightType.Join) > 0)
                {
                    ClampAvatarEyeHeight(player);
                }

                _hasFirstAvatarChanged = true;
            }

            _CloseAvatarChangeProcessing();
        }

        public void _CloseAvatarChangeProcessing()
        {
            _hasAvatarChanged = false;
        }

        private void ClampAvatarEyeHeight(VRCPlayerApi localPlayer)
        {
            var avatarEyeHeight = localPlayer.GetAvatarEyeHeightAsMeters();
            avatarEyeHeight = Mathf.Clamp(avatarEyeHeight, _avatarEyeHeightLowerLimit, _avatarEyeHeightUpperLimit);
            localPlayer.SetAvatarEyeHeightByMeters(avatarEyeHeight);
        }
    }
}
