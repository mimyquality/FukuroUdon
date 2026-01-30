/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-with-drop")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay with/ActiveRelay with Drop")]
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayWithDrop : ActiveRelayTo
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = ActiveRelayEventType.Inactive;

        private VRCPickup _pickup;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _pickup = GetComponent<VRCPickup>();

            _initialized = true;
        }

        private protected override void OnEnable()
        {
            Initialize();

            if (_eventType == ActiveRelayEventType.ActiveAndInactive ||
                _eventType == ActiveRelayEventType.Active)
            {
                DropThis();
            }
        }

        private protected override void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive ||
                _eventType == ActiveRelayEventType.Inactive)
            {
                DropThis();
            }
        }

        private void DropThis()
        {
            if (_pickup.IsHeld)
            {
                _pickup.Drop();
            }
        }
    }
}
