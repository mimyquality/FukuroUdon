/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;

    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Player Collision")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByPlayerCollision : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayPlayerEventType _eventType = default;

        public override void OnPlayerCollisionEnter(VRCPlayerApi player)
        {
            switch (_eventType)
            {
                case ActiveRelayPlayerEventType.PlayerEnterAndExit:
                case ActiveRelayPlayerEventType.PlayerEnter:
                    DoAction();
                    break;
                case ActiveRelayPlayerEventType.LocalPlayerEnterAndExit:
                case ActiveRelayPlayerEventType.LocalPlayerEnter:
                    if (player.isLocal) { DoAction(); }
                    break;
            }
        }

        public override void OnPlayerCollisionExit(VRCPlayerApi player)
        {
            switch (_eventType)
            {
                case ActiveRelayPlayerEventType.PlayerEnterAndExit:
                case ActiveRelayPlayerEventType.PlayerExit:
                    DoAction();
                    break;
                case ActiveRelayPlayerEventType.LocalPlayerEnterAndExit:
                case ActiveRelayPlayerEventType.LocalPlayerExit:
                    if (player.isLocal) { DoAction(); }
                    break;
            }
        }
    }
}
