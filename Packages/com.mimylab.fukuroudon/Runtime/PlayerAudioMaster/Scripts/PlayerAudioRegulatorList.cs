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

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator List")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerAudioRegulatorList : IPlayerAudioRegulator
    {
        [UdonSynced]
        private int[] _playerIds = new int[PlayerAudioSupervisor.HardCap];

        public int[] PlayerIds { get => _playerIds; }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayerList), 1);
        }
        public void _RefreshPlayerList()
        {
            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
            var tmpPlayerIds = new int[PlayerAudioSupervisor.HardCap];
            var tmpCount = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (!Utilities.IsValid(players[i])) { continue; }

                var tmpPlayerId = players[i].playerId;
                if (System.Array.IndexOf(_playerIds, tmpPlayerId) > -1)
                {
                    tmpPlayerIds[tmpCount++] = tmpPlayerId;
                }
            }
            tmpPlayerIds.CopyTo(_playerIds, 0);
            RequestSerialization();
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public bool AssignPlayer(VRCPlayerApi target)
        {
            if (!Networking.IsOwner(this.gameObject)) { return false; }

            var playerId = target.playerId;
            if (System.Array.IndexOf(_playerIds, playerId) > -1) { return false; }

            var lastIndex = System.Array.IndexOf(_playerIds, 0);
            if (lastIndex < 0) { return false; }

            _playerIds[lastIndex] = playerId;
            RequestSerialization();

            return true;
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleasePlayer(VRCPlayerApi target)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            var playerId = target.playerId;
            int index;
            while ((index = System.Array.IndexOf(_playerIds, playerId)) > -1)
            {
                _playerIds[index] = 0;
                RequestSerialization();
            }
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleaseAllPlayer()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            System.Array.Clear(_playerIds, 0, _playerIds.Length);
            RequestSerialization();
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            return System.Array.IndexOf(_playerIds, target.playerId) > -1;
        }
    }
}
