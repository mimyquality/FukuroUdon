/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    using VRC.SDK3.Components;

    public enum SwivelChairPlayerPlatform
    {
        PCVR,
        Desktop,
        Quest,
        Android
    }

    public enum SwivelChairInputMode
    {
        Vertical,
        Horizontal,
        CasterMove
    }

    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/Swivel Chair 2")]
    [DefaultExecutionOrder(-1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SwivelChair2 : UdonSharpBehaviour
    {
        [Header("Require References")]
        [SerializeField]
        private SC2SeatAdjuster _seatAdjuster;
        [SerializeField]
        private SC2InputManager _inputManager;

        [Header("Additional Settings")]
        [SerializeField]
        private VRCPickup _pickup;
        [SerializeField]
        private SC2Caster _caster;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_seatAdjuster) { _seatAdjuster = GetComponentInChildren<SC2SeatAdjuster>(); }
            if (!_inputManager) { _inputManager = GetComponentInChildren<SC2InputManager>(); }
            if (!_pickup) { _pickup = GetComponentInChildren<VRCPickup>(); }
            if (!_caster) { _caster = GetComponentInChildren<SC2Caster>(); }

            _seatAdjuster.manager = this;
            _inputManager.seatAdjuster = _seatAdjuster;
            _inputManager.caster = _caster;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();

            _inputManager.enabled = false;
        }

        public void OnSitDown()
        {
            Initialize();
            Networking.SetOwner(Networking.LocalPlayer, _seatAdjuster.gameObject);
            if (_caster) { Networking.SetOwner(Networking.LocalPlayer, _caster.gameObject); }

            _seatAdjuster.DisableInteractive = true;
            _inputManager.enabled = true;
            if (_pickup) { _pickup.pickupable = false; }
        }

        public void OnStandUp()
        {
            _seatAdjuster.DisableInteractive = false;
            _inputManager.enabled = false;
            if (_pickup) { _pickup.pickupable = true; }
        }
    }
}
