/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator List")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerAudioRegulatorList : IPlayerAudioRegulator
    {
        [UdonSynced]
        private int[] _playerIDList = new int[PlayerAudioSupervisor.HardCap];

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            RefreshPlayerList();
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public bool AssignPlayer(VRCPlayerApi target)
        {
            if (!Networking.IsOwner(this.gameObject)) { return false; }

            var playerID = target.playerId;
            for (int i = 0; i < _playerIDList.Length; i++)
            {
                if (_playerIDList[i] == playerID) { return false; }
            }
            for (int j = 0; j < _playerIDList.Length; j++)
            {
                if (_playerIDList[j] > 0) { continue; }

                _playerIDList[j] = playerID;
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

            var playerID = target.playerId;
            for (int i = 0; i < _playerIDList.Length; i++)
            {
                if (_playerIDList[i] == playerID)
                {
                    _playerIDList[i] = 0;
                    RequestSerialization();
                }
            }
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleaseAllPlayer()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            _playerIDList = new int[PlayerAudioSupervisor.HardCap];
            RequestSerialization();
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            var playerID = target.playerId;
            for (int i = 0; i < _playerIDList.Length; i++)
            {
                if (_playerIDList[i] == playerID) { return true; }
            }
            return false;
        }

        private void RefreshPlayerList()
        {
            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[PlayerAudioSupervisor.HardCap]);
            var tmpPlayerIDList = new int[PlayerAudioSupervisor.HardCap];
            int tmpPlayerID;
            for (int i = 0; i < players.Length; i++)
            {
                if (!Utilities.IsValid(players[i])) { continue; }

                tmpPlayerID = players[i].playerId;
                for (int j = 0; j < _playerIDList.Length; j++)
                {
                    if (tmpPlayerID == _playerIDList[j])
                    {
                        tmpPlayerIDList[i] = tmpPlayerID;
                        break;
                    }
                }
            }

            _playerIDList = tmpPlayerIDList;
            RequestSerialization();
        }
    }
}
