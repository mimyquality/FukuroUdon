/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator List")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerAudioRegulatorList : IPlayerAudioRegulator
    {
        [UdonSynced]
        private int[] _playerIdList = new int[PlayerAudioSupervisor.HardCap];

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayerList), 1);
        }
        public void _RefreshPlayerList()
        {
            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[PlayerAudioSupervisor.HardCap]);
            var tmpPlayerIdList = new int[PlayerAudioSupervisor.HardCap];
            var tmpCount = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (!Utilities.IsValid(players[i])) { continue; }

                var tmpPlayerId = players[i].playerId;
                if (System.Array.IndexOf(_playerIdList, tmpPlayerId) > -1)
                {
                    tmpPlayerIdList[tmpCount++] = tmpPlayerId;
                }
            }
            tmpPlayerIdList.CopyTo(_playerIdList, 0);
            RequestSerialization();
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public bool AssignPlayer(VRCPlayerApi target)
        {
            if (!Networking.IsOwner(this.gameObject)) { return false; }

            var playerId = target.playerId;
            if (System.Array.IndexOf(_playerIdList, playerId) > -1) { return false; }

            var lastIndex = System.Array.IndexOf(_playerIdList, 0);
            if (lastIndex > -1)
            {
                _playerIdList[lastIndex] = playerId;
                RequestSerialization();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleasePlayer(VRCPlayerApi target)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            var playerId = target.playerId;
            int index;
            while ((index = System.Array.IndexOf(_playerIdList, playerId)) > -1)
            {
                _playerIdList[index] = 0;
                RequestSerialization();
            }
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleaseAllPlayer()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            System.Array.Clear(_playerIdList, 0, _playerIdList.Length);
            RequestSerialization();
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            if (System.Array.IndexOf(_playerIdList, target.playerId) > -1) { return true; }

            return false;
        }
    }
}
