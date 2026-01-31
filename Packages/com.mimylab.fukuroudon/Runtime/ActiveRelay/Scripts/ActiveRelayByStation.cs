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
    using VRCStation = VRC.SDK3.Components.VRCStation;

    public enum ActiveRelayPlayerEvent
    {
        PlayerEnterAndExit,
        PlayerEnter,
        PlayerExit
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-station")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Station")]
    [RequireComponent(typeof(VRCStation))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByStation : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayPlayerEvent _eventType = default;
        [SerializeField]
        private NetworkEventTarget _acceptPlayerType = NetworkEventTarget.All;

        public override void OnStationEntered(VRCPlayerApi player)
        {
            switch (_eventType)
            {
                case ActiveRelayPlayerEvent.PlayerEnterAndExit:
                case ActiveRelayPlayerEvent.PlayerEnter:
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        public override void OnStationExited(VRCPlayerApi player)
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
