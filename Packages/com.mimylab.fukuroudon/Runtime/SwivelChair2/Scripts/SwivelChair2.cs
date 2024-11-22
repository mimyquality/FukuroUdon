/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
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
        StandaloneVR,
        Mobile
    }

    public enum SwivelChairInputMode
    {
        Disable,
        Vertical,
        Horizontal,
        CasterMove
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/Swivel Chair 2")]
    [DefaultExecutionOrder(-100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SwivelChair2 : UdonSharpBehaviour
    {
        internal SC2SeatAdjuster seatAdjuster;
        internal SC2InputManager inputManager;

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

            seatAdjuster = GetComponentInChildren<SC2SeatAdjuster>(true);
            inputManager = GetComponentInChildren<SC2InputManager>(true);
            if (!_pickup) { _pickup = GetComponentInParent<VRCPickup>(); }
            if (!_caster) { _caster = GetComponentInChildren<SC2Caster>(true); }

            seatAdjuster.swivelChair2 = this;
            inputManager.seatAdjuster = seatAdjuster;
            inputManager.caster = _caster;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();

            inputManager.enabled = false;
        }

        public override void OnPickup()
        {
            seatAdjuster.DisableInteractive = true;

            if (_caster)
            {
                Networking.SetOwner(Networking.LocalPlayer, _caster.gameObject);
            }
        }

        public override void OnDrop()
        {
            if (!_isSit) { seatAdjuster.DisableInteractive = false; }

            if (_caster)
            {
                var stationOwner = Networking.GetOwner(seatAdjuster.gameObject);
                Networking.SetOwner(stationOwner, _caster.gameObject);
            }
        }

        public void OnSitDown()
        {
            _isSit = true;
            seatAdjuster.DisableInteractive = true;
            inputManager.enabled = true;
            if (_pickup) { _pickup.pickupable = false; }
            if (_caster) { Networking.SetOwner(Networking.LocalPlayer, _caster.gameObject); }
        }

        public void OnStandUp()
        {
            _isSit = false;
            seatAdjuster.DisableInteractive = false;
            inputManager.enabled = false;
            if (_pickup) { _pickup.pickupable = true; }
        }
    }
}
