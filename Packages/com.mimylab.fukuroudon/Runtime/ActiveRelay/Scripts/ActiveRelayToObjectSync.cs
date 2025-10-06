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
    using VRC.Udon.Common.Interfaces;
    using VRC.SDK3.Components;
    using VRC.SDK3.UdonNetworkCalling;

    public enum ActiveRelayActionType
    {
        NoChange,
        Normal,
        Invert
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-objectsync")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to ObjectSync")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayToObjectSync : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private VRCObjectSync[] _vrcObjectSyncs = new VRCObjectSync[0];
        [SerializeField]
        private ManualObjectSync[] _manualObjectSyncs = new ManualObjectSync[0];
        [SerializeField]
        private ActiveRelayActionType _isKinematic = default;
        [SerializeField]
        private ActiveRelayActionType _useGravity = default;
        [SerializeField, Tooltip("Normal : Respawn when OnEnable\nImvert : Respawn when OnDisable")]
        private ActiveRelayActionType _respawn = default;

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                if (_isKinematic == ActiveRelayActionType.Normal)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetKinematicEnable));
                }
                if (_isKinematic == ActiveRelayActionType.Invert)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetKinematicDisable));
                }

                if (_useGravity == ActiveRelayActionType.Normal)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetGravityEnable));
                }
                if (_useGravity == ActiveRelayActionType.Invert)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetGravityDisable));
                }

                if (_respawn == ActiveRelayActionType.Normal)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(DoRespawn));
                }
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                if (_isKinematic == ActiveRelayActionType.Invert)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetKinematicEnable));
                }
                if (_isKinematic == ActiveRelayActionType.Normal)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetKinematicDisable));
                }

                if (_useGravity == ActiveRelayActionType.Invert)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetGravityEnable));
                }
                if (_useGravity == ActiveRelayActionType.Normal)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetGravityDisable));
                }

                if (_respawn == ActiveRelayActionType.Invert)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(DoRespawn));
                }
            }
        }

        [NetworkCallable(1)]
        public void SetKinematicEnable()
        {
            for (int i = 0; i < _vrcObjectSyncs.Length; i++)
            {
                if (!_vrcObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_vrcObjectSyncs[i].gameObject)) { continue; }

                _vrcObjectSyncs[i].SetKinematic(true);
            }

            for (int i = 0; i < _manualObjectSyncs.Length; i++)
            {
                if (!_manualObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_manualObjectSyncs[i].gameObject)) { continue; }

                _manualObjectSyncs[i].IsKinematic = true;
            }
        }

        [NetworkCallable(1)]
        public void SetKinematicDisable()
        {
            for (int i = 0; i < _vrcObjectSyncs.Length; i++)
            {
                if (!_vrcObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_vrcObjectSyncs[i].gameObject)) { continue; }

                _vrcObjectSyncs[i].SetKinematic(false);
            }

            for (int i = 0; i < _manualObjectSyncs.Length; i++)
            {
                if (!_manualObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_manualObjectSyncs[i].gameObject)) { continue; }

                _manualObjectSyncs[i].IsKinematic = false;
            }
        }

        [NetworkCallable(1)]
        public void SetGravityEnable()
        {
            for (int i = 0; i < _vrcObjectSyncs.Length; i++)
            {
                if (!_vrcObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_vrcObjectSyncs[i].gameObject)) { continue; }

                _vrcObjectSyncs[i].SetGravity(true);
            }

            for (int i = 0; i < _manualObjectSyncs.Length; i++)
            {
                if (!_manualObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_manualObjectSyncs[i].gameObject)) { continue; }

                _manualObjectSyncs[i].UseGravity = true;
            }
        }

        [NetworkCallable(1)]
        public void SetGravityDisable()
        {
            for (int i = 0; i < _vrcObjectSyncs.Length; i++)
            {
                if (!_vrcObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_vrcObjectSyncs[i].gameObject)) { continue; }

                _vrcObjectSyncs[i].SetGravity(false);
            }

            for (int i = 0; i < _manualObjectSyncs.Length; i++)
            {
                if (!_manualObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_manualObjectSyncs[i].gameObject)) { continue; }

                _manualObjectSyncs[i].UseGravity = false;
            }
        }

        [NetworkCallable(1)]
        public void DoRespawn()
        {
            for (int i = 0; i < _vrcObjectSyncs.Length; i++)
            {
                if (!_vrcObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_vrcObjectSyncs[i].gameObject)) { continue; }

                _vrcObjectSyncs[i].Respawn();
            }

            for (int i = 0; i < _manualObjectSyncs.Length; i++)
            {
                if (!_manualObjectSyncs[i]) { continue; }
                if (!Networking.IsOwner(_manualObjectSyncs[i].gameObject)) { continue; }

                _manualObjectSyncs[i].Respawn();
            }
        }
    }
}
