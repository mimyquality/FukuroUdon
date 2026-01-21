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
    using VRC.SDK3.UdonNetworkCalling;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#pa-regulator-list")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator List")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerAudioRegulatorList : PlayerAudioRegulator
    {
        [UdonSynced]
        private int[] _playerIds = new int[PlayerAudioRegulatorRegister.MaxPlayerCount];

        public int[] PlayerIds { get => _playerIds; }

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
            if (targetPlayerId < 1) { return false; }
            if (System.Array.IndexOf(_playerIds, targetPlayerId) > -1) { return false; }

            int vacantIndex = System.Array.IndexOf(_playerIds, 0);
            // 空きがないのでリフレッシュ
            if (vacantIndex < 0)
            {
                for (int i = 0; i < _playerIds.Length; i++)
                {
                    if (!Utilities.IsValid(VRCPlayerApi.GetPlayerById(_playerIds[i])))
                    {
                        _playerIds[i] = 0;
                    }
                }

                vacantIndex = System.Array.IndexOf(_playerIds, 0);
                // それでも空きがないので拡張
                if (vacantIndex < 0)
                {
                    vacantIndex = _playerIds.Length;
                    var tmp_PlayerIds = new int[vacantIndex + PlayerAudioRegulatorRegister.ExtendPlayerCount];
                    _playerIds.CopyTo(tmp_PlayerIds, 0);
                    _playerIds = tmp_PlayerIds;
                }
            }

            _playerIds[vacantIndex] = targetPlayerId;
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
            if (targetPlayerId < 1) { return; }

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

        protected override bool CheckUniqueApplicable(VRCPlayerApi target)
        {
            return System.Array.IndexOf(_playerIds, target.playerId) > -1;
        }
    }
}
