/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.Udon;
    using VRC.Udon.Common.Interfaces;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-udonbehaviour")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to UdonBehaviour")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToUdonBehaviour : ActiveRelayTo
    {
        [SerializeField]
        private NetworkEventTarget _networkEventTarget = NetworkEventTarget.Self;

        [Header("Settings when active")]
        [SerializeField]
        private UdonBehaviour[] _udonBehaviourForActive = new UdonBehaviour[0];
        [SerializeField]
        private string _customEventNameForActive = "";

        [Header("Settings when inactive")]
        [SerializeField]
        private UdonBehaviour[] _udonBehaviourForInactive = new UdonBehaviour[0];
        [SerializeField]
        private string _customEventNameForInactive = "";

        private protected override void OnEnable()
        {
            if (_customEventNameForActive == "") { return; }

            switch (_networkEventTarget)
            {
                case NetworkEventTarget.All:
                case NetworkEventTarget.Owner:
                case NetworkEventTarget.Others:
                    foreach (UdonBehaviour ub in _udonBehaviourForActive)
                    {
                        if (ub) { ub.SendCustomNetworkEvent(_networkEventTarget, _customEventNameForActive); }
                    }
                    break;
                default:
                    foreach (UdonBehaviour ub in _udonBehaviourForActive)
                    {
                        if (ub) { ub.SendCustomEvent(_customEventNameForActive); }
                    }
                    break;
            }
        }

        private protected override void OnDisable()
        {
            if (_customEventNameForInactive == "") { return; }

            switch (_networkEventTarget)
            {
                case NetworkEventTarget.All:
                case NetworkEventTarget.Owner:
                case NetworkEventTarget.Others:
                    foreach (UdonBehaviour ub in _udonBehaviourForInactive)
                    {
                        if (ub) { ub.SendCustomNetworkEvent(_networkEventTarget, _customEventNameForInactive); }
                    }
                    break;
                default:
                    foreach (UdonBehaviour ub in _udonBehaviourForInactive)
                    {
                        if (ub) { ub.SendCustomEvent(_customEventNameForInactive); }
                    }
                    break;
            }
        }
    }
}
