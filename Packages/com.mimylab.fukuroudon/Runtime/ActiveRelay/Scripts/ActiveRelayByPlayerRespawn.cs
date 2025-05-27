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

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Player Respawn")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByPlayerRespawn : ActiveRelayBy
    {
        [SerializeField]
        private bool _localOnly = true;

        private void Reset()
        {
            _actionType = ActiveRelayActivateType.Activate;
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (_localOnly && !player.isLocal) { return; }

            DoAction(player);
        }
    }
}
