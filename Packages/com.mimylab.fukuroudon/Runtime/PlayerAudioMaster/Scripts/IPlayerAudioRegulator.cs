/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    public class IPlayerAudioRegulator : UdonSharpBehaviour
    {
        [Header("Options")]
        public bool enableChannelMode = false;
        [Min(0)]
        public int channel = 0;
        [Space]
        public string[] whiteListPlayerName = new string[0];

        [Header("Player Voice Settings")]
        public bool enablePlayerVoiceOverride = true;
        [Range(0f, 24f)]
        public float voiceGain = 15f;
        [Range(0f, 999999f)]
        public float voiceDistanceNear = 0f;
        [Range(0f, 999999f)]
        public float voiceDistanceFar = 25f;

        [Header("Player Voice Advance Settings")]
        [Range(0f, 1000f)]
        public float voiceVolumetricRadius = 0f;
        public bool voiceLowpass = true;

        [Header("Avatar Audio Settings")]
        public bool enableAvatarAudioOverride = false;
        [Range(0f, 10f)]
        public float avatarAudioGain = 10f;
        [Range(0f, 40f)]
        public float avatarAudioDistanceNear = 40f;
        [Range(0f, 40f)]
        public float avatarAudioDistanceFar = 40f;

        [Header("Avatar Audio Advance Settings")]
        [Range(0f, 40f)]
        public float avatarAudioVolumetricRadius = 40f;
        public bool avatarAudioForceSpatial = false;
        public bool avatarAudioCustomCurve = false;

        public bool CheckApplicable(VRCPlayerApi target)
        {
            if (!EligiblePlayer(target)) { return false; }

            return CheckApplicableInternal(target);
        }

        public bool OverridePlayerVoice(VRCPlayerApi target)
        {
            if (enablePlayerVoiceOverride)
            {
                target.SetVoiceGain(voiceGain);
                target.SetVoiceDistanceNear(voiceDistanceNear);
                target.SetVoiceDistanceFar(voiceDistanceFar);
                target.SetVoiceVolumetricRadius(voiceVolumetricRadius);
                target.SetVoiceLowpass(voiceLowpass);
            }

            return enablePlayerVoiceOverride;
        }

        public bool OverrideAvatarAudio(VRCPlayerApi target)
        {
            if (enableAvatarAudioOverride)
            {
                target.SetAvatarAudioGain(avatarAudioGain);
                target.SetAvatarAudioNearRadius(avatarAudioDistanceNear);
                target.SetAvatarAudioFarRadius(avatarAudioDistanceFar);
                target.SetAvatarAudioVolumetricRadius(avatarAudioVolumetricRadius);
                target.SetAvatarAudioForceSpatial(avatarAudioForceSpatial);
                target.SetAvatarAudioCustomCurve(avatarAudioCustomCurve);
            }

            return enableAvatarAudioOverride;
        }

        protected bool EligiblePlayer(VRCPlayerApi target)
        {
            if (whiteListPlayerName.Length == 0) { return true; }

            var result = false;
            var targetPlayerName = target.displayName;
            for (int i = 0; i < whiteListPlayerName.Length; i++)
            {
                if (whiteListPlayerName[i] == targetPlayerName)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        protected virtual bool CheckApplicableInternal(VRCPlayerApi target) { return false; }
    }
}
