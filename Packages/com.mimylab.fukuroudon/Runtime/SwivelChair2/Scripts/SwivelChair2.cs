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

        private bool _isSit = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_seatAdjuster) { _seatAdjuster = GetComponentInChildren<SC2SeatAdjuster>(true); }
            if (!_inputManager) { _inputManager = GetComponentInChildren<SC2InputManager>(true); }
            if (!_pickup) { _pickup = GetComponentInParent<VRCPickup>(); }
            if (!_caster) { _caster = GetComponentInChildren<SC2Caster>(true); }

            _seatAdjuster.swivelChair2 = this;
            _inputManager.seatAdjuster = _seatAdjuster;
            _inputManager.caster = _caster;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();

            _inputManager.enabled = false;
        }

        public override void OnPickup()
        {
            _seatAdjuster.DisableInteractive = true;

            if (_caster)
            {
                Networking.SetOwner(Networking.LocalPlayer, _caster.gameObject);
            }
        }

        public override void OnDrop()
        {
            if (!_isSit) { _seatAdjuster.DisableInteractive = false; }

            if (_caster)
            {
                var stationOwner = Networking.GetOwner(_seatAdjuster.gameObject);
                Networking.SetOwner(stationOwner, _caster.gameObject);
            }
        }

        public void OnSitDown()
        {
            _isSit = true;
            _seatAdjuster.DisableInteractive = true;
            _inputManager.enabled = true;
            if (_pickup) { _pickup.pickupable = false; }
            if (_caster) { Networking.SetOwner(Networking.LocalPlayer, _caster.gameObject); }
        }

        public void OnStandUp()
        {
            _isSit = false;
            _seatAdjuster.DisableInteractive = false;
            _inputManager.enabled = false;
            if (_pickup) { _pickup.pickupable = true; }
        }
    }
}
