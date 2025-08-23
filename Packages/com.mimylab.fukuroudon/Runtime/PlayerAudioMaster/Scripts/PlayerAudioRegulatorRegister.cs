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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#pa-regulator-register")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator Register")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioRegulatorRegister : IPlayerAudioRegulator
    {
        internal PARRegisterPlayer localParRegisterPlayer;

        private int[] _playerIds = new int[PlayerAudioSupervisor.HardCap];

        public int[] PlayerIds { get => _playerIds; }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayerIdList), 1);
        }
        public void _RefreshPlayerIdList()
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
        }

        /// <summary>
        /// Assign LocalPlayer to this PAR Register.
        /// </summary>
        public bool AssignPlayer()
        {
            if (localParRegisterPlayer)
            {
                localParRegisterPlayer.IsAssigned = true;
                localParRegisterPlayer.RequestSerialization();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Release LocalPlayer to this PAR Register.
        /// </summary>
        public void ReleasePlayer()
        {
            if (localParRegisterPlayer)
            {
                localParRegisterPlayer.IsAssigned = false;
                localParRegisterPlayer.RequestSerialization();
            }
        }

        internal void _OnPlayerAssigned(VRCPlayerApi target)
        {
            var playerId = target.playerId;
            if (System.Array.IndexOf(_playerIds, playerId) > -1) { return; }

            var lastIndex = System.Array.IndexOf(_playerIds, 0);
            if (lastIndex < 0) { return; }

            _playerIds[lastIndex] = playerId;
        }

        internal void _OnPlayerReleased(VRCPlayerApi target)
        {
            var playerId = target.playerId;
            int index;
            while ((index = System.Array.IndexOf(_playerIds, playerId)) > -1)
            {
                _playerIds[index] = 0;
            }
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            return System.Array.IndexOf(_playerIds, target.playerId) > -1;
        }
    }
}
