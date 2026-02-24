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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Fix-Pickup-Up#pickup-platform-override")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Fix Pickup Up/Pickup Platform Override")]
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class PickupPlatformOverride : UdonSharpBehaviour
    {
        [Header("VR")]
        [SerializeField]
        private bool _vrAutoHold = false;
        [SerializeField]
        private string _vrUseText = string.Empty;
        [SerializeField]
        private string _vrInteractionText = string.Empty;

        [Header("Desktop")]
        [SerializeField]
        private bool _desktopAutoHold = true;
        [SerializeField]
        private string _desktopUseText = string.Empty;
        [SerializeField]
        private string _desktopInteractionText = string.Empty;

        [Header("Mobile")]
        [SerializeField]
        private bool _mobileAutoHold = true;
        [SerializeField]
        private string _mobileUseText = string.Empty;
        [SerializeField]
        private string _mobileInteractionText = string.Empty;

        private VRCPickup _pickup;

        private void Reset()
        {
            var pickup = GetComponent<VRCPickup>();
            _vrUseText = pickup.UseText;
            _vrInteractionText = pickup.InteractionText;
            _desktopUseText = pickup.UseText;
            _desktopInteractionText = pickup.InteractionText;
            _mobileUseText = pickup.UseText;
            _mobileInteractionText = pickup.InteractionText;
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _pickup = GetComponent<VRCPickup>();

            _initialized = true;
        }

        public override void OnInputMethodChanged(VRCInputMethod inputMethod)
        {
            Initialize();

            switch (inputMethod)
            {
                // VR
                case VRCInputMethod.Oculus:
                case VRCInputMethod.ViveXr:
                case VRCInputMethod.Index:
                case VRCInputMethod.HPMotionController:
                case VRCInputMethod.Osc:
                case VRCInputMethod.QuestHands:
                case VRCInputMethod.OpenXRGeneric:
                case VRCInputMethod.Pico:
                case VRCInputMethod.SteamVR2:
                    _pickup.AutoHold = _vrAutoHold ? VRCPickup.AutoHoldMode.Yes : VRCPickup.AutoHoldMode.No;
                    _pickup.UseText = _vrUseText;
                    _pickup.InteractionText = _vrInteractionText;
                    break;

                // モバイル
                case VRCInputMethod.Touch:
                    _pickup.AutoHold = _mobileAutoHold ? VRCPickup.AutoHoldMode.Yes : VRCPickup.AutoHoldMode.No;
                    _pickup.UseText = _mobileUseText;
                    _pickup.InteractionText = _mobileInteractionText;
                    break;

                // Desktop、その他
                /* 
                case VRCInputMethod.Keyboard:
                case VRCInputMethod.Mouse:
                case VRCInputMethod.Controller:
                case VRCInputMethod.Gaze:
                case VRCInputMethod.Vive:
                case VRCInputMethod.Generic:
                */
                default:
                    _pickup.AutoHold = _desktopAutoHold ? VRCPickup.AutoHoldMode.Yes : VRCPickup.AutoHoldMode.No;
                    _pickup.UseText = _desktopUseText;
                    _pickup.InteractionText = _desktopInteractionText;
                    break;
            }
        }
    }
}
