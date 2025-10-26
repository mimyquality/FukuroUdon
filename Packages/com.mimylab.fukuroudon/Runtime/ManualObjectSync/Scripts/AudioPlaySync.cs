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
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#Audio-Play-Sync")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Audio Play Sync")]
    [RequireComponent(typeof(AudioSource))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AudioPlaySync : UdonSharpBehaviour
    {
        private const double TimeTolerance = 0.05d;   // 単位：sec

        [SerializeField, Min(0.0f), Tooltip("sec")]
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
        private double _latestPlayStartTime = 0.0f;

        private bool _isWaitingAudioCheck = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _audioSource = GetComponent<AudioSource>();

            _initialized = true;
        }
        private void Start()
        {
            Initialize();

            if (Networking.IsOwner(this.gameObject))
            {
                BeginAudioCheck();
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                BeginAudioCheck();
            }
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
                _audioSource.time = (float)Networking.CalculateServerDeltaTime(Networking.GetServerTimeInSeconds(), sync_latestPlayStartTime);
            }

            if (_syncVolume)
            {
                _audioSource.volume = sync_volume;
            }
        }

        private void BeginAudioCheck()
        {
            if (_isWaitingAudioCheck) { return; }

            _isWaitingAudioCheck = true;
            SendCustomEventDelayedSeconds(nameof(_RepeatAudioCheck), Random.Range(0.0f, _playCheckInterval));
        }
        public void _RepeatAudioCheck()
        {
            _isWaitingAudioCheck = false;

            if (CheckAudioChange())
            {
                RequestSerialization();
            }

            _isWaitingAudioCheck = true;
            SendCustomEventDelayedSeconds(nameof(_RepeatAudioCheck), _playCheckInterval);
        }

        private bool CheckAudioChange()
        {
            var result = false;

            if (!_audioSource) { return result; }
            if (!Networking.IsOwner(this.gameObject)) { return result; }

            if (sync_isPlaying != _audioSource.isPlaying)
            {
                sync_isPlaying = _audioSource.isPlaying;
                result = true;
            }

            if (sync_isPlaying)
            {
                var currentPlayStartTime = Networking.GetServerTimeInSeconds() - _audioSource.time;
                var differenceTime = Networking.CalculateServerDeltaTime(currentPlayStartTime, sync_latestPlayStartTime);
                if (differenceTime > TimeTolerance || differenceTime < -TimeTolerance)
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
