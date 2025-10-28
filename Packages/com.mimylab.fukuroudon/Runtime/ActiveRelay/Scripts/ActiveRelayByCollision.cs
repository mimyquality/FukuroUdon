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

    public enum ActiveRelayCollisionEventType
    {
        EnterAndExit,
        Enter,
        Exit
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-collision")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Collision")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByCollision : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayCollisionEventType _eventType = default;
        [SerializeField]
        private Collider[] _reactiveColliders = new Collider[0];

        private void OnCollisionEnter(Collision collision)
        {
            if (!Utilities.IsValid(collision.collider)) { return; }

            switch (_eventType)
            {
                case ActiveRelayCollisionEventType.EnterAndExit:
                case ActiveRelayCollisionEventType.Enter:
                    if (CheckAccept(collision.collider)) { DoAction(Networking.LocalPlayer); }
                    break;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!Utilities.IsValid(collision.collider)) { return; }

            switch (_eventType)
            {
                case ActiveRelayCollisionEventType.EnterAndExit:
                case ActiveRelayCollisionEventType.Exit:
                    if (CheckAccept(collision.collider)) { DoAction(Networking.LocalPlayer); }
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
