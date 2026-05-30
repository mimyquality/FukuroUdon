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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#pa-regulator-avatarscale")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator AvatarScale")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioRegulatorAvatarScale : PlayerAudioRegulator
    {
        [Header("Option Settings")]
        [SerializeField, Min(0.1f), Tooltip("meter")]
        private float _baseEyeHeight = 1.6f;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _underScaleMultiplier = 0.0f;
        [SerializeField, Range(0.0f, 5.0f)]
        private float _overScaleMultiplier = 1.0f;

        private float _baseVoiceDistanceNear;
        private float _baseVoiceDistanceFar;
        private float _baseVoiceVolumetricRadius;

        private float _baseAvatarAudioDistanceNear;
        private float _baseAvatarAudioDistanceFar;
        private float _baseAvatarAudioVolumetricRadius;

        public override bool NeedRealtimeOverride { get => true; }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _baseVoiceDistanceNear = voiceDistanceNear;
            _baseVoiceDistanceFar = voiceDistanceFar;
            _baseVoiceVolumetricRadius = voiceVolumetricRadius;

            _baseAvatarAudioDistanceNear = avatarAudioDistanceNear;
            _baseAvatarAudioDistanceFar = avatarAudioDistanceFar;
            _baseAvatarAudioVolumetricRadius = avatarAudioVolumetricRadius;

            _initialized = true;
        }

        protected override bool CheckUniqueApplicable(VRCPlayerApi target)
        {
            Initialize();

            float avatarScale = target.GetAvatarEyeHeightAsMeters() / _baseEyeHeight;
            float multiply = avatarScale < 1.0f ? 1f - _underScaleMultiplier * (1 - avatarScale) : _overScaleMultiplier * (avatarScale - 1f) + 1f;

            voiceDistanceNear = Mathf.Clamp(multiply * _baseVoiceDistanceNear, 0f, 1000000f);
            voiceDistanceFar = Mathf.Clamp(multiply * _baseVoiceDistanceFar, 0f, 1000000f);
            voiceVolumetricRadius = Mathf.Clamp(multiply * _baseVoiceVolumetricRadius, 0f, 1000f);

            avatarAudioDistanceNear = Mathf.Max(multiply * _baseAvatarAudioDistanceNear, 0f);
            avatarAudioDistanceFar = Mathf.Max(multiply * _baseAvatarAudioDistanceFar, 0f);
            avatarAudioVolumetricRadius = Mathf.Max(multiply * _baseAvatarAudioVolumetricRadius, 0f);

            return true;
        }
    }
}
