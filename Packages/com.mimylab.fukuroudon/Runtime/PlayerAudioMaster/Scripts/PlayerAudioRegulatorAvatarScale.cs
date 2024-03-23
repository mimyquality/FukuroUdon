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
    using VRC.Udon;

    public class PlayerAudioRegulatorAvatarScale : IPlayerAudioRegulator
    {
        [SerializeField, Tooltip("meter")]
        private float _baseEyeHeight = 1.6f;

        [SerializeField, Range(-1.0f, 1.0f)]
        private float _underScaleMultiplier = 0.0f;
        [SerializeField, Range(-1.0f, 1.0f)]
        private float _overScaleMultiplier = 1.0f;

        private float _baseVoiceGain;
        private float _baseVoiceDistanceNear;
        private float _baseVoiceDistanceFar;
        private float _baseVoiceVolumetricRadius;

        private float _baseAvatarAudioGain;
        private float _baseAvatarAudioDistanceNear;
        private float _baseAvatarAudioDistanceFar;
        private float _baseAvatarAudioVolumetricRadius;

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

            

            return false;
        }
    }
}
