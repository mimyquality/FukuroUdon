/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
//using VRC.Udon;
//using VRC.SDK3.Components;

namespace MimyLab
{
    [System.Flags]
    public enum AdvancedWorldSettingsInitializeEyeHeightType
    {
        Join = 1 << 0,
        AvatarChange = 1 << 1
    }

    [AddComponentMenu("Fukuro Udon/Advanced World Settings")]
    [DefaultExecutionOrder(-100)]
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
        [SerializeField][Range(0f, 40f)] private float _avatarAudioDistanceNear = 40f;
        [SerializeField][Range(0f, 40f)] private float _avatarAudioDistanceFar = 40f;
        [Space]
        [SerializeField][Range(0f, 40f)] private float _avatarAudioVolumetricRadius = 40f;
        [SerializeField] private bool _avatarAudioForceSpatial = false;
        [SerializeField] private bool _avatarAudioCustomCurve = false;

        [Header("Avatar Scaling")]
        [SerializeField] private bool _initializeAvatarScaling = true;
        [SerializeField] private bool _allowManualAvatarScaling = true;
        [SerializeField][Range(0.2f, 5f)] private float _avatarEyeHeightMinimum = 0.2f;
        [SerializeField][Range(0.2f, 5f)] private float _avatarEyeHeightMaximum = 5f;
        [Space]
        [Tooltip("When the button is checked, the Avatar Eye Height is initialized at that point.")]
        [SerializeField][EnumFlag] private AdvancedWorldSettingsInitializeEyeHeightType _initializeAvatarEyeHight = 0;
        [SerializeField][Range(0.1f, 100f)] private float _avatarEyeHeight = 1.3f;

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

                if (((int)_initializeAvatarEyeHight & (int)AdvancedWorldSettingsInitializeEyeHeightType.Join) > 0)
                {
                    player.SetAvatarEyeHeightByMeters(_avatarEyeHeight);
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

        public void OnAvatarChanged(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }

            if (((int)_initializeAvatarEyeHight & (int)AdvancedWorldSettingsInitializeEyeHeightType.AvatarChange) > 0)
            {
                player.SetAvatarEyeHeightByMeters(_avatarEyeHeight);
            }
        }
    }
}
