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
        private const int HardCap = 96;

        [UdonSynced]
        private int[] _playerIDList = new int[HardCap];

        private VRCPlayerApi _localPlayer;
        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _localPlayer = Networking.LocalPlayer;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.playerId < _localPlayer.playerId) { return; }
            if (!_localPlayer.IsOwner(this.gameObject)) { return; }

            RefreshPlayerList();
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!_localPlayer.IsOwner(this.gameObject)) { return; }

            RefreshPlayerList();
            RequestSerialization();
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public bool AssignPlayer(VRCPlayerApi target)
        {
            Initialize();

            if (!_localPlayer.IsOwner(this.gameObject)) { return false; }

            var result = false;
            var playerID = target.playerId;
            for (int i = 0; i < _playerIDList.Length; i++)
            {
                if (_playerIDList[i] == playerID) { return false; }
            }
            for (int j = 0; j < _playerIDList.Length; j++)
            {
                if (_playerIDList[j] <= 0)
                {
                    _playerIDList[j] = playerID;
                    result = true;
                    break;
                }
            }
            RequestSerialization();

            return result;
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleasePlayer(VRCPlayerApi target)
        {
            Initialize();

            if (!_localPlayer.IsOwner(this.gameObject)) { return; }

            var playerID = target.playerId;
            for (int i = 0; i < _playerIDList.Length; i++)
            {
                if (_playerIDList[i] == playerID)
                {
                    _playerIDList[i] = 0;
                }
            }
            RequestSerialization();
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleaseAllPlayer()
        {
            Initialize();

            if (!_localPlayer.IsOwner(this.gameObject)) { return; }

            System.Array.Clear(_playerIDList, 0, _playerIDList.Length);
            //_playerIDList = new int[HardCap];
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
            var tmpPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(tmpPlayers);

            var tmpPlayerIDList = new int[HardCap];
            int tmpPlayerID;
            for (int i = 0; i < tmpPlayers.Length; i++)
            {
                if (!Utilities.IsValid(tmpPlayers[i])) { continue; }

                tmpPlayerID = tmpPlayers[i].playerId;
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
        }
    }
}
