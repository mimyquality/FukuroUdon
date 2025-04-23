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
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator Persistence")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioRegulatorPersistence : IPlayerAudioRegulator
    {
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
        }

        public bool AssignPlayer(VRCPlayerApi target)
        {
            var playerId = target.playerId;
            if (System.Array.IndexOf(_playerIdList, playerId) > -1) { return false; }

            var lastIndex = System.Array.IndexOf(_playerIdList, 0);
            if (lastIndex > -1)
            {
                _playerIdList[lastIndex] = playerId;

                return true;
            }

            return false;
        }

        public void ReleasePlayer(VRCPlayerApi target)
        {
            var playerId = target.playerId;
            int index;
            while ((index = System.Array.IndexOf(_playerIdList, playerId)) > -1)
            {
                _playerIdList[index] = 0;
            }
        }

        public void ReleaseAllPlayer()
        {
            System.Array.Clear(_playerIdList, 0, _playerIdList.Length);
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            if (System.Array.IndexOf(_playerIdList, target.playerId) > -1) { return true; }

            return false;
        }
    }
}
