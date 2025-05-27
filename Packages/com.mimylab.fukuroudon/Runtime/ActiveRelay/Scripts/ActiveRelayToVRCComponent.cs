/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Components;
    using VRCStation = VRC.SDK3.Components.VRCStation;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to VRCComponent")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToVRCComponent : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField, Tooltip("Toggle \"ChangeAvatarOnUse\" bool value")]
        private VRCAvatarPedestal[] _avatarPedestals = new VRCAvatarPedestal[0];
        [SerializeField, Tooltip("Toggle pickupable")]
        private VRCPickup[] _pickups = new VRCPickup[0];
        [SerializeField, Tooltip("Toggle disableStationExit")]
        private VRCStation[] _stations = new VRCStation[0];
        [SerializeField]
        private bool _invert = false;

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                ToggleComponents(!_invert);
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                ToggleComponents(_invert);
            }
        }

        private void ToggleComponents(bool value)
        {
            for (int i = 0; i < _avatarPedestals.Length; i++)
            {
                if (!_avatarPedestals[i]) { continue; }

                _avatarPedestals[i].ChangeAvatarsOnUse = value;
            }

            for (int i = 0; i < _pickups.Length; i++)
            {
                if (!_pickups[i]) { continue; }

                _pickups[i].pickupable = value;
            }

            for (int i = 0; i < _stations.Length; i++)
            {
                if (!_stations[i]) { continue; }

                _stations[i].disableStationExit = value;
            }
        }
    }
}
