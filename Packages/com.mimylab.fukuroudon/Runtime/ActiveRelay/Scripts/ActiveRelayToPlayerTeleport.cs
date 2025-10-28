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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-player-teleport")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to Player Teleport")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToPlayerTeleport : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private Transform _teleportTarget = null;
        [SerializeField]
        private VRC_SceneDescriptor.SpawnOrientation _spawnOrientation = VRC_SceneDescriptor.SpawnOrientation.Default;
        [SerializeField]
        private bool _lerpOnRemote = false;
        [SerializeField]
        private bool _enableOnDeserializationBugFix = false;

        private void Reset()
        {
            _teleportTarget = this.transform;
        }

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                if (_enableOnDeserializationBugFix)
                {
                    SendCustomEventDelayedFrames(nameof(Teleport), 1);
                }
                else
                {
                    Teleport();
                }
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                if (_enableOnDeserializationBugFix)
                {
                    SendCustomEventDelayedFrames(nameof(Teleport), 1);
                }
                else
                {
                    Teleport();
                }
            }
        }

        private void Teleport()
        {
            var localPlayer = Networking.LocalPlayer;

            if (_teleportTarget)
            {
                localPlayer.TeleportTo(_teleportTarget.position, _teleportTarget.rotation, _spawnOrientation, _lerpOnRemote);
            }
        }
    }
}
