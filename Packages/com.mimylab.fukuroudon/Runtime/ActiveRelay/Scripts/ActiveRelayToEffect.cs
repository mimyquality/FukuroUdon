/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MimyLab
{
    [AddComponentMenu("Fukuro Udon/Active Relay/To Effect")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayToEffect : UdonSharpBehaviour
    {
        [Tooltip("Only particle emission module toggle")]
        [SerializeField]
        bool _invert = false;

        [Header("Settings when active")]
        [SerializeField]
        AudioSource _audioSourceForActive = null;
        [SerializeField]
        AudioClip _soundForActive = null;

        [SerializeField]
        ParticleSystem _particleForActive = null;
        [SerializeField]
        int _emitForActive = 0;

        [Header("Settings when inactive")]
        [SerializeField]
        AudioSource _audioSourceForInactive = null;
        [SerializeField]
        AudioClip _soundForInactive = null;

        [SerializeField]
        ParticleSystem _particleForInactive = null;
        [SerializeField]
        int _emitForInactive = 0;

        void OnEnable()
        {
            if (_audioSourceForActive)
            {
                _audioSourceForActive.PlayOneShot(_soundForActive);
            }

            if (_particleForActive)
            {
                if (_emitForActive > 0)
                {
                    _particleForActive.Emit(_emitForActive);
                }
                else
                {
                    var emission = _particleForActive.emission;
                    emission.enabled = !_invert;
                }
            }
        }

        void OnDisable()
        {
            if (_audioSourceForInactive)
            {
                _audioSourceForInactive.PlayOneShot(_soundForInactive);
            }

            if (_particleForInactive)
            {
                if (_emitForInactive > 0)
                {
                    _particleForInactive.Emit(_emitForInactive);
                }
                else
                {
                    var emission = _particleForInactive.emission;
                    emission.enabled = _invert;
                }
            }
        }
    }
}
