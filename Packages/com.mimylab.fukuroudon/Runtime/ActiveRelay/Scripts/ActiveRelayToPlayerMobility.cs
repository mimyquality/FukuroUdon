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

    public enum ActiveRelayToPlayerMobilityImmobilizeState
    {
        NoChange = default,
        Enable,
        Disable
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-player-mobility")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to Player Mobility")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToPlayerMobility : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private bool _setWalkSpeed = false;
        [SerializeField, Range(0.0f, 5.0f)]
        private float _walkSpeed = 2.0f;
        [SerializeField]
        private bool _setRunSpeed = false;
        [SerializeField, Range(0.0f, 10.0f)]
        private float _runSpeed = 4.0f;
        [SerializeField]
        private bool _setStrafeSpeed = false;
        [SerializeField, Range(0.0f, 5.0f)]
        private float _strafeSpeed = 2.0f;
        [SerializeField]
        private bool _setJumpImpulse = false;
        [SerializeField, Range(0.0f, 10.0f)]
        private float _jumpImpulse = 3.0f;
        [SerializeField]
        private bool _setGravityStrength = false;
        [SerializeField, Range(0.0f, 10.0f)]
        private float _gravityStrength = 1.0f;
        [SerializeField]
        private ActiveRelayToPlayerMobilityImmobilizeState _immobilize = default;

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                ChangePlayerMobility(true);
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                ChangePlayerMobility(false);
            }
        }

        private void ChangePlayerMobility(bool value)
        {
            var localPlayer = Networking.LocalPlayer;

            if (_setWalkSpeed)
            {
                localPlayer.SetWalkSpeed(_walkSpeed);
            }
            if (_setRunSpeed)
            {
                localPlayer.SetRunSpeed(_runSpeed);
            }
            if (_setStrafeSpeed)
            {
                localPlayer.SetStrafeSpeed(_strafeSpeed);
            }
            if (_setJumpImpulse)
            {
                localPlayer.SetJumpImpulse(_jumpImpulse);
            }
            if (_setGravityStrength)
            {
                localPlayer.SetGravityStrength(_gravityStrength);
            }
            switch (_immobilize)
            {
                case ActiveRelayToPlayerMobilityImmobilizeState.Enable:
                    localPlayer.Immobilize(value);
                    break;
                case ActiveRelayToPlayerMobilityImmobilizeState.Disable:
                    localPlayer.Immobilize(!value);
                    break;
            }
        }
    }
}
