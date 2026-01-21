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
    using VRC.SDKBase.Editor.Attributes;
    using VRC.Udon.Common.Interfaces;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-gameobject-in-playerobject")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to GameObject in PlayerObject")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToGameObjectInPlayerObject : ActiveRelayTo
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private NetworkEventTarget _acceptPlayerType = NetworkEventTarget.Self;
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];
        [SerializeField]
        private bool _invert = false;

        [Space]
        [HelpBox("Disable delay if Dlelay Time is 0", HelpBoxAttribute.MessageType.Info)]
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _delayTime = 0.0f;
        [SerializeField]
        private bool _delayLatestOnly = false;

        private int _activateDelayedCount = 0;
        private int _deactivateDelayedCount = 0;

        private VRCPlayerApi[] _playersEmpty = new VRCPlayerApi[0];
        private VRCPlayerApi[] _playersSolo = new VRCPlayerApi[1];

        private protected override void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                if (_delayTime > 0.0f)
                {
                    if (_invert)
                    {
                        _deactivateDelayedCount++;
                        SendCustomEventDelayedSeconds(nameof(_DeactivateDelayed), _delayTime);
                    }
                    else
                    {
                        _activateDelayedCount++;
                        SendCustomEventDelayedSeconds(nameof(_ActivateDelayerd), _delayTime);
                    }
                    return;
                }

                ToggleActive(!_invert);
            }
        }

        private protected override void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                if (_delayTime > 0.0f)
                {
                    if (_invert)
                    {
                        _activateDelayedCount++;
                        SendCustomEventDelayedSeconds(nameof(_ActivateDelayerd), _delayTime);
                    }
                    else
                    {
                        _deactivateDelayedCount++;
                        SendCustomEventDelayedSeconds(nameof(_DeactivateDelayed), _delayTime);
                    }
                    return;
                }

                ToggleActive(_invert);
            }
        }

        public void _ActivateDelayerd()
        {
            _activateDelayedCount--;
            if (_delayLatestOnly && _activateDelayedCount > 0) { return; }

            ToggleActive(true);
        }

        public void _DeactivateDelayed()
        {
            _deactivateDelayedCount--;
            if (_delayLatestOnly && _deactivateDelayedCount > 0) { return; }

            ToggleActive(false);
        }

        private void ToggleActive(bool value)
        {
            var othersOnly = false;
            VRCPlayerApi[] players;
            switch (_acceptPlayerType)
            {
                case NetworkEventTarget.All:
                    players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
                    break;
                case NetworkEventTarget.Owner:
                    _playersSolo[0] = Networking.GetOwner(this.gameObject);
                    players = _playersSolo;
                    break;
                case NetworkEventTarget.Others:
                    players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
                    othersOnly = true;
                    break;
                case NetworkEventTarget.Self:
                    _playersSolo[0] = Networking.LocalPlayer;
                    players = _playersSolo;
                    break;
                default:
                    players = _playersEmpty;
                    break;
            }

            for (int i = 0; i < players.Length; i++)
            {
                if (!Utilities.IsValid(players[i])) { continue; }
                if (othersOnly && players[i].isLocal) { continue; }

                foreach (GameObject reference in _gameObjects)
                {
                    if (!reference) { continue; }

                    var target = (Transform)players[i].FindComponentInPlayerObjects(reference.transform);
                    if (Utilities.IsValid(target))
                    {
                        target.gameObject.SetActive(value);
                    }
                }
            }
        }
    }
}
