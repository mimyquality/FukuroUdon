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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-drone")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to Drone")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToDrone : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private Transform _teleportTarget = null;
        [SerializeField]
        private bool _lerpOnRemote = false;
        [SerializeField]
        private bool _enableSetVelocity = false;
        [SerializeField]
        private Vector3 _velocity = Vector3.zero;

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                DoDroneAction();
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                DoDroneAction();
            }
        }

        private void DoDroneAction()
        {
            VRCDroneApi drone = Networking.LocalPlayer.GetDrone();

            if (_teleportTarget)
            {
                drone.TeleportTo(_teleportTarget.position, _teleportTarget.rotation, _lerpOnRemote);
            }
            if (_enableSetVelocity)
            {
                drone.SetVelocity(_velocity);
            }
        }
    }
}
