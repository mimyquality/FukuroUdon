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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-playertrigger")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Player Trigger")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByPlayerTrigger : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayPlayerEvent _eventType = default;
        [SerializeField]
        private NetworkEventTarget _acceptPlayerType = NetworkEventTarget.All;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            switch (_eventType)
            {
                case ActiveRelayPlayerEvent.PlayerEnterAndExit:
                case ActiveRelayPlayerEvent.PlayerEnter:
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            switch (_eventType)
            {
                case ActiveRelayPlayerEvent.PlayerEnterAndExit:
                case ActiveRelayPlayerEvent.PlayerExit:
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
