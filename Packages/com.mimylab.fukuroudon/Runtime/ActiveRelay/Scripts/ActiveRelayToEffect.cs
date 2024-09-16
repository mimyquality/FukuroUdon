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

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to Effect")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToEffect : UdonSharpBehaviour
    {
        [Tooltip("Only particle emission module toggle")]
        [SerializeField]
        private bool _invert = false;

        [Header("Settings when active")]
        [SerializeField]
        private AudioSource _audioSourceForActive = null;
        [SerializeField]
        private AudioClip _soundForActive = null;

        [SerializeField]
        private ParticleSystem _particleForActive = null;
        [SerializeField]
        private int _emitForActive = 0;

        [Header("Settings when inactive")]
        [SerializeField]
        private AudioSource _audioSourceForInactive = null;
        [SerializeField]
        private AudioClip _soundForInactive = null;

        [SerializeField]
        private ParticleSystem _particleForInactive = null;
        [SerializeField]
        private int _emitForInactive = 0;

        private void OnEnable()
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

        private void OnDisable()
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
