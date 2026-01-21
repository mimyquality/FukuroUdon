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

    public enum ActiveRelayToAvatarScalingSetMode
    {
        Meters = default,
        Multiply,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-avatarscaling")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to AvatarScaling")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToAvatarScaling : ActiveRelayTo
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;

        [SerializeField]
        private ActiveRelayToAvatarScalingSetMode _setMode = default;
        [SerializeField, Range(0.1f, 100f)]
        private float _avatarEyeHeight = 1.3f;

        private protected override void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                SetAvatarEyeHeight();
            }
        }

        private protected override void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                SetAvatarEyeHeight();
            }
        }

        private void SetAvatarEyeHeight()
        {
            switch (_setMode)
            {
                case ActiveRelayToAvatarScalingSetMode.Meters:
                    Networking.LocalPlayer.SetAvatarEyeHeightByMeters(_avatarEyeHeight);
                    break;
                case ActiveRelayToAvatarScalingSetMode.Multiply:
                    Networking.LocalPlayer.SetAvatarEyeHeightByMultiplier(_avatarEyeHeight);
                    break;
            }
        }
    }
}
