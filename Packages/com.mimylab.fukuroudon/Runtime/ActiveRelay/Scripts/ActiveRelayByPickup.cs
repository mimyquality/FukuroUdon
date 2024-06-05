/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    public enum ActiveRelayPickupEventType
    {
        Pickup,
        PickupUseDown,
        PickupUseUp,
        Drop
    }

    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Pickup")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByPickup : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayPickupEventType _eventType = default;

        public override void OnPickup()
        {
            if (_eventType == ActiveRelayPickupEventType.Pickup)
            {
                DoAction();
            }
        }

        public override void OnPickupUseDown()
        {
            if (_eventType == ActiveRelayPickupEventType.PickupUseDown)
            {
                DoAction();
            }
        }

        public override void OnPickupUseUp()
        {
            if (_eventType == ActiveRelayPickupEventType.PickupUseUp)
            {
                DoAction();
            }
        }

        public override void OnDrop()
        {
            if (_eventType == ActiveRelayPickupEventType.Drop)
            {
                DoAction();
            }
        }
    }
}
