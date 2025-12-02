/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.UdonNetworkCalling;
    using VRC.SDKBase;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#pa-regulator-list")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator List")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerAudioRegulatorList : IPlayerAudioRegulator
    {
        [UdonSynced]
        private int[] _playerIds = new int[PlayerAudioSupervisor.MaxPlayerCount];

        public int[] PlayerIds { get => _playerIds; }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayerList), 1);
        }
        public void _RefreshPlayerList()
        {
            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
            var tmpPlayerIds = new int[PlayerAudioSupervisor.MaxPlayerCount];
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

        /******************************
         public Method
         ******************************/
        public bool AssignPlayer(VRCPlayerApi target)
        {
            if (!Networking.IsOwner(this.gameObject)) { return false; }
            if (!Utilities.IsValid(target)) { return false; }

            return AssignPlayer(target.playerId);
        }

        public bool AssignPlayer(int targetPlayerId)
        {
            if (!Networking.IsOwner(this.gameObject)) { return false; }
            if (System.Array.IndexOf(_playerIds, targetPlayerId) > -1) { return false; }

            var lastIndex = System.Array.IndexOf(_playerIds, 0);
            if (lastIndex < 0) { return false; }

            _playerIds[lastIndex] = targetPlayerId;
            RequestSerialization();

            return true;
        }

        public void ReleasePlayer(VRCPlayerApi target)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }
            if (!Utilities.IsValid(target)) { return; }

            ReleasePlayer(target.playerId);
        }

        public void ReleasePlayer(int targetPlayerId)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            int index;
            while ((index = System.Array.IndexOf(_playerIds, targetPlayerId)) > -1)
            {
                _playerIds[index] = 0;
                RequestSerialization();
            }
        }

        public void ReleaseAllPlayer()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            System.Array.Clear(_playerIds, 0, _playerIds.Length);
            RequestSerialization();
        }

        /******************************
         For SendCustomnetworkEvent Method
         ******************************/
        [NetworkCallable]
        public void CallAssignPlayer(int targetPlayerId) { AssignPlayer(targetPlayerId); }

        [NetworkCallable]
        public void CallReleasePlayer(int targetPlayerId) { ReleasePlayer(targetPlayerId); }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            return System.Array.IndexOf(_playerIds, target.playerId) > -1;
        }
    }
}
