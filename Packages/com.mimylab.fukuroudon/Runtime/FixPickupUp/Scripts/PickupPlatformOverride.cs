/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;

    public enum PickupPlatformOverridePlatform
    {
        VR,
        Desktop,
        Mobile
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Fix-Pickup-Up#pickup-platform-override")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Fix Pickup Up/Pickup Platform Override")]
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class PickupPlatformOverride : UdonSharpBehaviour
    {
        [SerializeField]
        private PickupPlatformOverridePlatform _overridePlatform = PickupPlatformOverridePlatform.VR;

        [Space]
        [SerializeField]
        private VRCPickup.AutoHoldMode _autoHold = VRCPickup.AutoHoldMode.Sometimes;
        [SerializeField]
        private string _useText = string.Empty;
        [SerializeField]
        private string _interactionText = string.Empty;
        [SerializeField]
        private float _proximity = 2.0f;
        [SerializeField]
        private VRCPickup.PickupOrientation _orientation = VRCPickup.PickupOrientation.Any;
        [SerializeField]
        private bool _allowManipulationWhenEquipped = true;

        private void Reset()
        {
            var pickup = GetComponent<VRCPickup>();
            _proximity = pickup.proximity;
            _orientation = pickup.orientation;
            _allowManipulationWhenEquipped = pickup.allowManipulationWhenEquipped;
        }

        private void Start()
        {
            PickupPlatformOverridePlatform currentPlatform = Networking.LocalPlayer.IsUserInVR() ? PickupPlatformOverridePlatform.VR : PickupPlatformOverridePlatform.Desktop;
            if (InputManager.GetLastUsedInputMethod() == VRCInputMethod.Touch) { currentPlatform = PickupPlatformOverridePlatform.Mobile; }

            if (currentPlatform == _overridePlatform)
            {
                var pickup = GetComponent<VRCPickup>();
                pickup.AutoHold = _autoHold;
                if (!string.IsNullOrEmpty(_useText)) { pickup.UseText = _useText; }
                if (!string.IsNullOrEmpty(_interactionText)) { pickup.InteractionText = _interactionText; }
                pickup.proximity = _proximity;
                pickup.orientation = _orientation;
                pickup.allowManipulationWhenEquipped = _allowManipulationWhenEquipped;
            }
        }
    }
}
