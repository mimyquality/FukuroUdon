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
    using VRC.Udon;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator Register")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioRegulatorRegister : IPlayerAudioRegulator
    {
        [SerializeField, Tooltip("Set to listen \"OnPlayerAssignChanged\" event.")]
        private UdonBehaviour[] _eventListeners = new UdonBehaviour[0];

        [HideInInspector]
        public int[] assignedPlayerIds = new int[PlayerAudioSupervisor.HardCap];

        internal PARRegisterPlayer localParRegisterPlayer;

        private int[] _playerIdList = new int[PlayerAudioSupervisor.HardCap];

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayerIdList), 1);
        }
        public void _RefreshPlayerIdList()
        {
            var players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
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
            _playerIdList.CopyTo(assignedPlayerIds, 0);

            SendPlayerAssignChanged();
        }

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
            if (System.Array.IndexOf(_playerIdList, playerId) > -1) { return; }

            var lastIndex = System.Array.IndexOf(_playerIdList, 0);
            if (lastIndex < 0) { return; }

            _playerIdList[lastIndex] = playerId;
            _playerIdList.CopyTo(assignedPlayerIds, 0);

            SendPlayerAssignChanged();
        }

        internal void _OnPlayerReleased(VRCPlayerApi target)
        {
            var playerId = target.playerId;
            int index;
            while ((index = System.Array.IndexOf(_playerIdList, playerId)) > -1)
            {
                _playerIdList[index] = 0;
            }
            _playerIdList.CopyTo(assignedPlayerIds, 0);

            SendPlayerAssignChanged();
        }

        internal void _AddEventListner(UdonBehaviour ub)
        {
            var newEventListeners = new UdonBehaviour[_eventListeners.Length + 1];
            _eventListeners.CopyTo(newEventListeners, 0);
            newEventListeners[newEventListeners.Length - 1] = ub;
            _eventListeners = newEventListeners;
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            return System.Array.IndexOf(_playerIdList, target.playerId) > -1;
        }

        private void SendPlayerAssignChanged()
        {
            for (int i = 0; i < _eventListeners.Length; i++)
            {
                if (_eventListeners[i])
                {
                    _eventListeners[i].SendCustomEvent("OnPlayerAssignChanged");
                }
            }
        }
    }
}
