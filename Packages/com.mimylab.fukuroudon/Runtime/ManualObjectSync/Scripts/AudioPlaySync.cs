/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#audio-play-sync")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Audio Play Sync")]
    [RequireComponent(typeof(AudioSource))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AudioPlaySync : UdonSharpBehaviour
    {
        private const float TimeTolerance = 0.05f;   // 単位：sec

        [SerializeField, Min(1.0f), Tooltip("sec")]
        private float _playCheckInterval = 5.0f;
        [SerializeField]
        private bool _syncVolume = false;

        [UdonSynced]
        private bool sync_isPlaying = false;
        [UdonSynced]
        private double sync_latestPlayStartTime = 0.0d;
        [UdonSynced]
        private float sync_volume = 0.5f;

        private AudioSource _audioSource;
        private double _latestPlayStartTime = 0.0d;

        private VRCTweenHandle _audioCheckHandle;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _audioSource = GetComponent<AudioSource>();

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            BeginAudioCheck();
        }

        private void OnDestroy()
        {
            gameObject.KillAllTweens();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            BeginAudioCheck();
        }

        public override void OnDeserialization()
        {
            Initialize();

            if (!_audioSource) { return; }

            if (sync_isPlaying && !_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            if (!sync_isPlaying && _audioSource.isPlaying)
            {
                _audioSource.Pause();
            }

            if (_latestPlayStartTime != sync_latestPlayStartTime)
            {
                _latestPlayStartTime = sync_latestPlayStartTime;

                float audioTime = (float)Networking.CalculateServerDeltaTime(Networking.GetServerTimeInSeconds(), sync_latestPlayStartTime);
                float audioLength = _audioSource.clip.length;
                if (audioTime > audioLength)
                {
                    audioTime -= audioLength;
                }
                _audioSource.time = Mathf.Clamp(audioTime, 0.0f, audioLength);
            }

            if (_syncVolume)
            {
                _audioSource.volume = Mathf.Clamp01(sync_volume);
            }
        }

        private void BeginAudioCheck()
        {
            if (_audioCheckHandle.IsActive) { _audioCheckHandle.Kill(); }
            if (Networking.IsOwner(this.gameObject))
            {
                float duration = _playCheckInterval + Random.Range(0.0f, _playCheckInterval);
                _audioCheckHandle = VRCTween.DelayedCall(this, nameof(_RepeatAudioCheck), duration);
            }
        }

        public void _RepeatAudioCheck()
        {
            if (!this.isActiveAndEnabled) { return; }
            if (!Networking.IsOwner(this.gameObject)) { return; }

            if (CheckAudioChange()) { RequestSerialization(); }

            _audioCheckHandle.SetDuration(_playCheckInterval).Restart();
        }

        private bool CheckAudioChange()
        {
            var result = false;

            if (!_audioSource) { return result; }

            if (sync_isPlaying != _audioSource.isPlaying)
            {
                sync_isPlaying = _audioSource.isPlaying;
                result = true;
            }

            if (sync_isPlaying)
            {
                double currentPlayStartTime = Networking.GetServerTimeInSeconds() - _audioSource.time;
                double differenceTime = Networking.CalculateServerDeltaTime(currentPlayStartTime, sync_latestPlayStartTime);
                if (!(-TimeTolerance <= differenceTime && differenceTime <= TimeTolerance))
                {
                    sync_latestPlayStartTime = currentPlayStartTime;
                    result = true;
                }
            }

            if (_syncVolume)
            {
                if (sync_volume != _audioSource.volume)
                {
                    sync_volume = _audioSource.volume;
                    result = true;
                }
            }

            return result;
        }
    }
}
