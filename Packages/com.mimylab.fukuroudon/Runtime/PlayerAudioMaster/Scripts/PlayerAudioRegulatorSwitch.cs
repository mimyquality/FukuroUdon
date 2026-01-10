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

    public enum PlayerAudioRegulatorSwitchMode
    {
        Toggle,
        ButtonON,
        ButtonOFF
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#pa-regulator-switch")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator Switch")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerAudioRegulatorSwitch : PlayerAudioRegulator
    {
        [Header("Option Settings")]
        public PlayerAudioRegulatorSwitchMode switchMode = default;

        [UdonSynced]
        private int _assignedPlayerId = -1;

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

        public override void Interact()
        {
            if (!EligiblePlayer(_localPlayer)) { return; }

            Networking.SetOwner(_localPlayer, this.gameObject);

            if (switchMode == PlayerAudioRegulatorSwitchMode.Toggle)
            {
                if (_assignedPlayerId == _localPlayer.playerId)
                {
                    ReleasePlayer();
                }
                else if (_assignedPlayerId > 0)
                {
                    AssignPlayer(_localPlayer);
                }
            }
            else if (switchMode == PlayerAudioRegulatorSwitchMode.ButtonON)
            {
                AssignPlayer(_localPlayer);
            }
            else if (switchMode == PlayerAudioRegulatorSwitchMode.ButtonOFF)
            {
                ReleasePlayer();
            }
        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void AssignPlayer(VRCPlayerApi target)
        {
            Initialize();

            if (!_localPlayer.IsOwner(this.gameObject)) { return; }

            _assignedPlayerId = target.playerId;
            RequestSerialization();

        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleasePlayer()
        {
            Initialize();

            if (!_localPlayer.IsOwner(this.gameObject)) { return; }

            _assignedPlayerId = -1;
            RequestSerialization();
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            return target.playerId == _assignedPlayerId;
        }
    }
}
