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

    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator Area")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioRegulatorArea : IPlayerAudioRegulator
    {
        private bool _isInvalid = true;
        private Collider _collider;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            this.gameObject.layer = 5;
            if (!GetComponent<Collider>())
            {
                _collider = this.gameObject.AddComponent<BoxCollider>();
                _collider.isTrigger = true;
            }
        }
#endif

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            _isInvalid = false;
        }

        private void OnDisable()
        {
            _isInvalid = true;
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            if (_isInvalid || !_collider.enabled) { return false; }

            var pos = target.GetPosition();

            return _collider.ClosestPoint(pos) == pos;
        }
    }
}
