
namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    public enum PlayerAudioRegulatorSwitchMode
    {
        Toggle,
        ButtonON,
        ButtonOFF
    }

    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator Switch")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerAudioRegulatorSwitch : IPlayerAudioRegulator
    {
        [Header("Specific settings")]
        public PlayerAudioRegulatorSwitchMode switchMode = default;

        [UdonSynced]
        private int _assignedPlayerID = -1;

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
                if (_assignedPlayerID == _localPlayer.playerId)
                {
                    ReleasePlayer();
                }
                else if (_assignedPlayerID > 0)
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

            _assignedPlayerID = target.playerId;
            RequestSerialization();

        }

        /// <summary>
        /// Need this gameObject ownership.
        /// </summary>
        public void ReleasePlayer()
        {
            Initialize();
            
            if (!_localPlayer.IsOwner(this.gameObject)) { return; }

            _assignedPlayerID = -1;
            RequestSerialization();
        }

        protected override bool CheckApplicableInternal(VRCPlayerApi target)
        {
            return (target.playerId == _assignedPlayerID);
        }
    }
}
