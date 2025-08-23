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
    using VRC.SDK3.Components;

    public enum PlayerAudioRegulatorPickupEventType
    {
        None,
        OnPickup,
        OnPickupUseDown,
        OnPickupUseUp,
        OnDrop
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#par-switch-controller-for-pickup")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PAR Switch Controller for Pickup")]
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class PARSwitchController : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerAudioRegulatorSwitch linkedSwitch;

        public PlayerAudioRegulatorPickupEventType assignEvent = PlayerAudioRegulatorPickupEventType.OnPickupUseDown;
        public PlayerAudioRegulatorPickupEventType releaseEvent = PlayerAudioRegulatorPickupEventType.OnPickupUseUp;

        private VRCPickup _pickup;
        private VRCPlayerApi _localPlayer;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _pickup = GetComponent<VRCPickup>();
            _localPlayer = Networking.LocalPlayer;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void OnPickup()
        {
            Initialize();
            Networking.SetOwner(_localPlayer, linkedSwitch.gameObject);

            if (assignEvent == PlayerAudioRegulatorPickupEventType.OnPickup)
            {
                linkedSwitch.AssignPlayer(_localPlayer);
            }

            if (releaseEvent == PlayerAudioRegulatorPickupEventType.OnPickup)
            {
                linkedSwitch.ReleasePlayer();
            }
        }

        public override void OnPickupUseDown()
        {
            Initialize();

            if (assignEvent == PlayerAudioRegulatorPickupEventType.OnPickupUseDown)
            {
                linkedSwitch.AssignPlayer(_localPlayer);
            }

            if (releaseEvent == PlayerAudioRegulatorPickupEventType.OnPickupUseDown)
            {
                linkedSwitch.ReleasePlayer();
            }
        }

        public override void OnPickupUseUp()
        {
            Initialize();

            if (assignEvent == PlayerAudioRegulatorPickupEventType.OnPickupUseUp)
            {
                linkedSwitch.AssignPlayer(_localPlayer);
            }

            if (releaseEvent == PlayerAudioRegulatorPickupEventType.OnPickupUseUp)
            {
                linkedSwitch.ReleasePlayer();
            }
        }

        public override void OnDrop()
        {
            Initialize();

            if (assignEvent == PlayerAudioRegulatorPickupEventType.OnDrop)
            {
                linkedSwitch.AssignPlayer(_localPlayer);
            }

            if (releaseEvent == PlayerAudioRegulatorPickupEventType.OnDrop)
            {
                linkedSwitch.ReleasePlayer();
            }
        }
    }
}
