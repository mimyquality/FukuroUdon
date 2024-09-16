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
    using VRC.SDK3.Components;

    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Reset Switch for ObjectSync")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ResetSwitchForObjectSync : UdonSharpBehaviour
    {
        public GameObject[] resetObjects;

        [SerializeField]
        [Min(0.0f), Tooltip("sec")]
        private float _interval = 5.0f;

        private float _lastResetTime;
        private VRCObjectSync[] _objectSyncs_vrc;
        private UdonBehaviour[] _objectSyncs_udon;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _objectSyncs_vrc = new VRCObjectSync[resetObjects.Length];
            _objectSyncs_udon = new UdonBehaviour[resetObjects.Length];
            for (int i = 0; i < resetObjects.Length; i++)
            {
                if (!resetObjects[i]) { continue; }

                _objectSyncs_vrc[i] = resetObjects[i].GetComponent<VRCObjectSync>();
                _objectSyncs_udon[i] = resetObjects[i].GetComponent<UdonBehaviour>();
            }

            _initialized = true;
        }
        /* 
        private void Start()
        {
            Initialize();
        } */

        public override void Interact()
        {
            if (Time.time < _lastResetTime + _interval) { return; }

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ResetObjectsPosition));
        }

        public void ResetObjectsPosition()
        {
            Initialize();

            var resetTime = Time.time;
            if (resetTime < _lastResetTime + _interval) { return; }
            _lastResetTime = resetTime;

            for (int i = 0; i < resetObjects.Length; i++)
            {
                if (!resetObjects[i]) { continue; }
                if (!Networking.IsOwner(resetObjects[i])) { continue; }

                if (_objectSyncs_vrc[i])
                {
                    _objectSyncs_vrc[i].Respawn();
                    continue;
                }

                if (_objectSyncs_udon[i])
                {
                    _objectSyncs_udon[i].SendCustomEvent("Respawn");
                    continue;
                }
            }
        }
    }
}
