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
    using VRC.SDK3.Components;

    public enum SwivelChairPlayerPlatform
    {
        VR,
        Desktop,
        Mobile
    }

    public enum SwivelChairInputMode
    {
        Disable,
        Vertical,
        Horizontal,
        CasterMove
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Swivel-Chair-2#swivel-chair2")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/Swivel Chair 2")]
    [DefaultExecutionOrder(-100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SwivelChair2 : UdonSharpBehaviour
    {
        [Header("Additional Settings")]
        [SerializeField]
        private VRCPickup _pickup;
        [SerializeField]
        private SC2Caster _caster;

        internal SC2SeatAdjuster _seatAdjuster;
        internal SC2InputManager _inputManager;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _seatAdjuster = GetComponentInChildren<SC2SeatAdjuster>(true);
            _inputManager = GetComponentInChildren<SC2InputManager>(true);
            if (!_pickup) { _pickup = GetComponentInParent<VRCPickup>(); }
            if (!_caster) { _caster = GetComponentInParent<SC2Caster>(); }

            _seatAdjuster._swivelChair2 = this;
            _inputManager._seatAdjuster = _seatAdjuster;
            _inputManager._caster = _caster;

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
            if (!_seatAdjuster._isSitting) { _seatAdjuster.DisableInteractive = false; }

            if (_caster)
            {
                VRCPlayerApi stationOwner = Networking.GetOwner(_seatAdjuster.gameObject);
                Networking.SetOwner(stationOwner, _caster.gameObject);
            }
        }

        public void OnSitDown()
        {
            _seatAdjuster.DisableInteractive = true;
            _inputManager.enabled = true;
            if (_pickup) { _pickup.pickupable = false; }
            if (_caster) { Networking.SetOwner(Networking.LocalPlayer, _caster.gameObject); }
        }

        public void OnStandUp()
        {
            _seatAdjuster.DisableInteractive = false;
            _inputManager.enabled = false;
            if (_pickup) { _pickup.pickupable = true; }
        }
    }
}
