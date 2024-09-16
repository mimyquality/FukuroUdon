/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.Udon;
    //using VRC.SDK3.Components;

    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator AvatarScale")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioRegulatorAvatarScale : IPlayerAudioRegulator
    {
        [SerializeField, Min(0.1f), Tooltip("meter")]
        private float _baseEyeHeight = 1.6f;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _underScaleMultiplier = 0.0f;
        [SerializeField, Range(0.0f, 5.0f)]
        private float _overScaleMultiplier = 1.0f;

        private float _baseVoiceGain;
        private float _baseVoiceDistanceNear;
        private float _baseVoiceDistanceFar;
        private float _baseVoiceVolumetricRadius;

        private float _baseAvatarAudioGain;
        private float _baseAvatarAudioDistanceNear;
        private float _baseAvatarAudioDistanceFar;
        private float _baseAvatarAudioVolumetricRadius;

        public override bool NeedRealtimeOverride { get => true; }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _baseVoiceGain = voiceGain;
            _baseVoiceDistanceNear = voiceDistanceNear;
            _baseVoiceDistanceFar = voiceDistanceFar;
            _baseVoiceVolumetricRadius = voiceVolumetricRadius;

            _baseAvatarAudioGain = avatarAudioGain;
            _baseAvatarAudioDistanceNear = avatarAudioDistanceNear;
            _baseAvatarAudioDistanceFar = avatarAudioDistanceFar;
            _baseAvatarAudioVolumetricRadius = avatarAudioVolumetricRadius;

            _initialized = true;
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            Initialize();

            var targetScale = target.GetAvatarEyeHeightAsMeters() / _baseEyeHeight;
            var multiply = targetScale < 1.0f ? 1f - _underScaleMultiplier * (1 - targetScale) : _overScaleMultiplier * targetScale;

            voiceGain = Mathf.Clamp(multiply * _baseVoiceGain, 0f, 24f);
            voiceDistanceNear = Mathf.Clamp(multiply * _baseVoiceDistanceNear, 0f, 999999.9f);
            voiceDistanceFar = Mathf.Clamp(multiply * _baseVoiceDistanceFar, 0f, 999999.9f);
            voiceVolumetricRadius = Mathf.Clamp(multiply * _baseVoiceVolumetricRadius, 0f, 1000f);

            avatarAudioGain = Mathf.Clamp(multiply * _baseAvatarAudioGain, 0f, 10f);
            avatarAudioDistanceNear = Mathf.Max(multiply * _baseAvatarAudioDistanceNear, 0f);
            avatarAudioDistanceFar = Mathf.Max(multiply * _baseAvatarAudioDistanceFar, 0f);
            avatarAudioVolumetricRadius = Mathf.Max(multiply * _baseAvatarAudioVolumetricRadius, 0f);

            return true;
        }
    }
}
