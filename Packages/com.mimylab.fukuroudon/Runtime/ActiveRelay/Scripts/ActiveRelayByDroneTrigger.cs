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
    using VRC.Udon.Common.Interfaces;

    public enum ActiveRelayDroneEventType
    {
        DroneEnterAndExit,
        DroneEnter,
        DroneExit
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-dronetrigger")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Drone Trigger")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByDroneTrigger : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayDroneEventType _eventType = default;
        [SerializeField]
        private NetworkEventTarget _acceptPlayerType = NetworkEventTarget.All;

        public override void OnDroneTriggerEnter(VRCDroneApi drone)
        {
            VRCPlayerApi player = drone.GetPlayer();

            switch (_eventType)
            {
                case ActiveRelayDroneEventType.DroneEnterAndExit:
                case ActiveRelayDroneEventType.DroneEnter:
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        public override void OnDroneTriggerExit(VRCDroneApi drone)
        {
            VRCPlayerApi player = drone.GetPlayer();

            switch (_eventType)
            {
                case ActiveRelayDroneEventType.DroneEnterAndExit:
                case ActiveRelayDroneEventType.DroneExit:
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
