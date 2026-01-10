/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.Dynamics;
    using VRC.SDKBase;
    using VRC.SDK3.Dynamics.Contact.Components;
    using VRC.Udon.Common.Interfaces;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-contact")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Contact")]
    [RequireComponent(typeof(VRCContactReceiver))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByContact : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayCollisionEventType _eventType = default;
        [SerializeField]
        private NetworkEventTarget _acceptPlayerType = NetworkEventTarget.All;
        [SerializeField]
        [Min(0.0f), Tooltip("Minimum collision velocity to trigger OnEnter. m/s")]
        private float _minVelocity = 0.0f;

        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            switch (_eventType)
            {
                case ActiveRelayCollisionEventType.EnterAndExit:
                case ActiveRelayCollisionEventType.Enter:
                    if (contactInfo.enterVelocity.sqrMagnitude < _minVelocity * _minVelocity) { return; }

                    VRCPlayerApi player = contactInfo.contactSender.player;
                    player = Utilities.IsValid(player) ? player : Networking.LocalPlayer;
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        override public void OnContactExit(ContactExitInfo contactInfo)
        {
            switch (_eventType)
            {
                case ActiveRelayCollisionEventType.EnterAndExit:
                case ActiveRelayCollisionEventType.Exit:
                    VRCPlayerApi player = contactInfo.contactSender.player;
                    player = Utilities.IsValid(player) ? player : Networking.LocalPlayer;
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        private bool CheckAccept(VRCPlayerApi player)
        {
            switch (_acceptPlayerType)
            {
                case NetworkEventTarget.All:
                    return true;
                case NetworkEventTarget.Owner:
                    return player.IsOwner(this.gameObject);
                case NetworkEventTarget.Others:
                    return !player.isLocal;
                case NetworkEventTarget.Self:
                    return player.isLocal;
                default:
                    return false;
            }
        }
    }
}
