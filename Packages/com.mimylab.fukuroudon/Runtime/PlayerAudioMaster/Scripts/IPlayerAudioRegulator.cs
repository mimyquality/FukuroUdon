/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    public enum PlayerAudioRegulatorChannelUncmatchMode
    {
        Default,
        Fallback,
        Passthrough,
        Pretend,
    }

    public class IPlayerAudioRegulator : UdonSharpBehaviour
    {
        [Header("Options")]
        public bool enableChannelMode = false;
        [Min(0)]
        public int channel = 0;
        public PlayerAudioRegulatorChannelUncmatchMode channelUnmatchMode = default;
        public IPlayerAudioRegulator unmatchFallback = null;
        [Space]
        public bool othersOnly = false;
        public string[] allowedPlayerNameList = new string[0];

        [Header("Player Voice Settings")]
        public bool enablePlayerVoiceOverride = true;
        [Range(0f, 24f)]
        public float voiceGain = 15f;
        [Range(0f, 999999.9f)]
        public float voiceDistanceNear = 0f;
        [Range(0f, 999999.9f)]
        public float voiceDistanceFar = 25f;

        [Header("Player Voice Advance Settings")]
        [Range(0f, 1000f)]
        public float voiceVolumetricRadius = 0f;
        public bool voiceLowpass = true;

        [Header("Avatar Audio Settings")]
        public bool enableAvatarAudioOverride = false;
        [Range(0f, 10f)]
        public float avatarAudioGain = 10f;
        [Min(0f)]
        public float avatarAudioDistanceNear = 0f;
        [Min(0f)]
        public float avatarAudioDistanceFar = 40f;

        [Header("Avatar Audio Advance Settings")]
        [Min(0f)]
        public float avatarAudioVolumetricRadius = 0f;
        public bool avatarAudioForceSpatial = false;
        public bool avatarAudioCustomCurve = false;

        public virtual bool NeedRealtimeOverride { get => false; }

        public bool CheckApplicable(VRCPlayerApi target)
        {
            if (!enabled || !gameObject.activeInHierarchy) { return false; }
            if (othersOnly && target.isLocal) { return false; }
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
            if (allowedPlayerNameList.Length == 0) { return true; }
            if (System.Array.IndexOf(allowedPlayerNameList, target.displayName) > -1) { return true; }

            return false;
        }

        protected virtual bool CheckApplicableInternal(VRCPlayerApi target) { return false; }
    }
}
