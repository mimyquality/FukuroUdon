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
    public class PlayerAudioRegulatorRegister : PlayerAudioRegulator
    {
        public const int MaxPlayerCount = 90;
        public const int ExtendPlayerCount = 30;

        internal PARRegisterPlayer localParRegisterPlayer;

        private int[] _playerIds = new int[MaxPlayerCount];

        public int[] PlayerIds { get => _playerIds; }

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
            int playerId = target.playerId;
            if (System.Array.IndexOf(_playerIds, playerId) > -1) { return; }

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
                    var tmp_PlayerIds = new int[vacantIndex + ExtendPlayerCount];
                    _playerIds.CopyTo(tmp_PlayerIds, 0);
                    _playerIds = tmp_PlayerIds;
                }
            }

            _playerIds[vacantIndex] = playerId;
        }

        internal void _OnPlayerReleased(VRCPlayerApi target)
        {
            int playerId = target.playerId;
            int index;
            while ((index = System.Array.IndexOf(_playerIds, playerId)) > -1)
            {
                _playerIds[index] = 0;
            }
        }

        protected override bool CheckUniqueApplicable(VRCPlayerApi target)
        {
            return System.Array.IndexOf(_playerIds, target.playerId) > -1;
        }
    }
}
