/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    using VRC.Udon;

    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to UdonBehaviour")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayToUdonBehaviour : UdonSharpBehaviour
    {
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

        private void OnEnable()
        {
            if (_customEventNameForActive != "")
            {
                foreach (var ub in _udonBehaviourForActive)
                {
                    if (ub) { ub.SendCustomEvent(_customEventNameForActive); }
                }
            }
        }

        private void OnDisable()
        {
            if (_customEventNameForInactive != "")
            {
                foreach (var ub in _udonBehaviourForInactive)
                {
                    if (ub) { ub.SendCustomEvent(_customEventNameForInactive); }
                }
            }
        }
    }
}
