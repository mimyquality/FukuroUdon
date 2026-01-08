/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;
    using VRC.SDK3.UdonNetworkCalling;
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#resetswitch-for-objectsync")]
    [Icon(ComponentIconPath.FukuroUdon)]
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
        private ManualObjectSync[] _objectSyncs_mos;
        private UdonBehaviour[][] _objectSyncs_udon;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _objectSyncs_vrc = new VRCObjectSync[resetObjects.Length];
            _objectSyncs_mos = new ManualObjectSync[resetObjects.Length];
            _objectSyncs_udon = new UdonBehaviour[resetObjects.Length][];
            for (int i = 0; i < resetObjects.Length; i++)
            {
                if (!resetObjects[i]) { continue; }

                _objectSyncs_vrc[i] = resetObjects[i].GetComponent<VRCObjectSync>();
                _objectSyncs_mos[i] = resetObjects[i].GetComponent<ManualObjectSync>();
                _objectSyncs_udon[i] = resetObjects[i].GetComponents<UdonBehaviour>();
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
            ResetObjectsPosition();
        }

        public void ResetObjectsPosition()
        {
            if (Time.time < _lastResetTime + _interval) { return; }

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(CallResetObjectsPosition));
        }

        [NetworkCallable]
        public void CallResetObjectsPosition()
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

                if (_objectSyncs_mos[i])
                {
                    _objectSyncs_mos[i].Respawn();
                    continue;
                }

                if (_objectSyncs_udon[i] != null)
                {
                    for (int j = 0; j < _objectSyncs_udon[i].Length; j++)
                    {
                        if (_objectSyncs_udon[i][j])
                        {
                            _objectSyncs_udon[i][j].SendCustomEvent("Respawn");
                        }
                    }
                }
            }
        }
    }
}
