/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-trigger")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Trigger")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByTrigger : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayCollisionEventType _eventType = default;
        [SerializeField]
        private Collider[] _reactiveColliders = new Collider[0];

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) { return; }

            switch (_eventType)
            {
                case ActiveRelayCollisionEventType.EnterAndExit:
                case ActiveRelayCollisionEventType.Enter:
                    if (CheckAccept(other)) { DoAction(Networking.LocalPlayer); }
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Utilities.IsValid(other)) { return; }

            switch (_eventType)
            {
                case ActiveRelayCollisionEventType.EnterAndExit:
                case ActiveRelayCollisionEventType.Exit:
                    if (CheckAccept(other)) { DoAction(Networking.LocalPlayer); }
                    break;
            }
        }

        private bool CheckAccept(Collider collider)
        {
            if (_reactiveColliders.Length < 1) { return false; }

            return System.Array.IndexOf(_reactiveColliders, collider) > -1;
        }
    }
}
