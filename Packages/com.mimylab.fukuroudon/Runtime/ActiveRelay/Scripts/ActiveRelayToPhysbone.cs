/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Dynamics.PhysBone.Components;
    using VRC.SDK3.UdonNetworkCalling;
    using VRC.Udon.Common.Interfaces;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-physbone")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to Physbone")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ActiveRelayToPhysbone : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private VRCPhysBone[] _physbones = new VRCPhysBone[0];
        [SerializeField]
        private bool _releaseGrabs = true;
        [SerializeField]
        private bool _releasePoses = true;

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                if (_releaseGrabs) { SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReleaseGrabs)); }
                if (_releasePoses) { SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReleasePoses)); }
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                if (_releaseGrabs) { SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReleaseGrabs)); }
                if (_releasePoses) { SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReleasePoses)); }
            }
        }

        [NetworkCallable(1)]
        public void ReleaseGrabs()
        {
            for (int i = 0; i < _physbones.Length; i++)
            {
                if (!_physbones[i]) { continue; }

                _physbones[i].ReleaseGrabs();
            }
        }

        [NetworkCallable(1)]
        public void ReleasePoses()
        {
            for (int i = 0; i < _physbones.Length; i++)
            {
                if (!_physbones[i]) { continue; }

                _physbones[i].ReleasePoses();
            }
        }
    }
}
