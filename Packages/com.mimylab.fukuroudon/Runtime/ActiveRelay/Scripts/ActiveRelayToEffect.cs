/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    public enum ActiveRelayToEffectAudioSourceState
    {
        NoChange = default,
        Play,
        Pause,
        Stop
    }

    public enum ActiveRelayToEffectEmissionModule
    {
        NoChange = default,
        Enable,
        Disable
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to Effect")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToEffect : UdonSharpBehaviour
    {
        [Header("Settings when active")]
        [SerializeField]
        private AudioSource _audioSourceForActive = null;
        [SerializeField]
        private ActiveRelayToEffectAudioSourceState _audioSourceStateForActive = default;
        [SerializeField]
        private AudioClip _soundForActive = null;

        [Space]
        [SerializeField]
        private ParticleSystem _particleForActive = null;
        [SerializeField]
        private ActiveRelayToEffectEmissionModule _emissionModuleForActive = default;
        [SerializeField]
        private int _emitForActive = 0;

        [Header("Settings when inactive")]
        [SerializeField]
        private AudioSource _audioSourceForInactive = null;
        [SerializeField]
        private ActiveRelayToEffectAudioSourceState _audioSourceStateForInactive = default;
        [SerializeField]
        private AudioClip _soundForInactive = null;

        [Space]
        [SerializeField]
        private ParticleSystem _particleForInactive = null;
        [SerializeField]
        private ActiveRelayToEffectEmissionModule _emissionModuleForInactive = default;
        [SerializeField]
        private int _emitForInactive = 0;

        private void OnEnable()
        {
            if (_audioSourceForActive)
            {
                switch (_audioSourceStateForActive)
                {
                    case ActiveRelayToEffectAudioSourceState.Play: _audioSourceForActive.Play(); break;
                    case ActiveRelayToEffectAudioSourceState.Pause: _audioSourceForActive.Pause(); break;
                    case ActiveRelayToEffectAudioSourceState.Stop: _audioSourceForActive.Stop(); break;
                }

                if (_soundForActive)
                {
                    _audioSourceForActive.PlayOneShot(_soundForActive);
                }
            }

            if (_particleForActive)
            {
                var emission = _particleForActive.emission;
                switch (_emissionModuleForActive)
                {
                    case ActiveRelayToEffectEmissionModule.Enable: emission.enabled = true; break;
                    case ActiveRelayToEffectEmissionModule.Disable: emission.enabled = false; break;
                }

                if (_emitForActive > 0)
                {
                    _particleForActive.Emit(_emitForActive);
                }
            }
        }

        private void OnDisable()
        {
            if (_audioSourceForInactive)
            {
                switch (_audioSourceStateForInactive)
                {
                    case ActiveRelayToEffectAudioSourceState.Play: _audioSourceForInactive.Play(); break;
                    case ActiveRelayToEffectAudioSourceState.Pause: _audioSourceForInactive.Pause(); break;
                    case ActiveRelayToEffectAudioSourceState.Stop: _audioSourceForInactive.Stop(); break;
                }

                if (_soundForInactive)
                {
                    _audioSourceForInactive.PlayOneShot(_soundForInactive);
                }
            }

            if (_particleForInactive)
            {
                var emission = _particleForInactive.emission;
                switch (_emissionModuleForInactive)
                {
                    case ActiveRelayToEffectEmissionModule.Enable: emission.enabled = true; break;
                    case ActiveRelayToEffectEmissionModule.Disable: emission.enabled = false; break;
                }

                if (_emitForInactive > 0)
                {
                    _particleForInactive.Emit(_emitForInactive);
                }
            }
        }
    }
}
