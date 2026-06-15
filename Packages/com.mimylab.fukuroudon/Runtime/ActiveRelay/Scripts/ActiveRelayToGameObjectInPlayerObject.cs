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
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-gameobject-in-playerobject")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to GameObject in PlayerObject")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToGameObjectInPlayerObject : ActiveRelayTo
    {
        [SerializeField]
        private ActiveRelayActiveEvent _eventType = default;
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

        private VRCTweenHandle _activateTweenHandle;
        private VRCTweenHandle _deactivateTweenHandle;
        private VRCPlayerApi[] _playersEmpty = new VRCPlayerApi[0];
        private VRCPlayerApi[] _playersSolo = new VRCPlayerApi[1];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (_delayTime > 0.0f)
            {
                _activateTweenHandle = VRCTween.DelayedCall(this, nameof(_ActivateDelayed), _delayTime)
                    .Pause();
                _deactivateTweenHandle = VRCTween.DelayedCall(this, nameof(_DeactivateDelayed), _delayTime)
                    .Pause();
            }

            _initialized = true;
        }

        private protected override void OnEnable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Active)
            {
                if (_delayTime > 0.0f)
                {
                    ToggleActiveDelayed(!_invert);
                    return;
                }

                ToggleActive(!_invert);
            }
        }

        private protected override void OnDisable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Inactive)
            {
                if (_delayTime > 0.0f)
                {
                    ToggleActiveDelayed(_invert);
                    return;
                }

                ToggleActive(_invert);
            }
        }

        private void OnDestroy()
        {
            gameObject.KillAllTweens();
        }

        private void ToggleActiveDelayed(bool value)
        {
            if (value)
            {
                if (_delayLatestOnly)
                {
                    Initialize();
                    _activateTweenHandle.SetDuration(_delayTime).Restart();
                }
                else
                {
                    VRCTween.DelayedCall(this, nameof(_ActivateDelayed), _delayTime);
                }
            }
            else
            {
                if (_delayLatestOnly)
                {
                    Initialize();
                    _deactivateTweenHandle.SetDuration(_delayTime).Restart();
                }
                else
                {
                    VRCTween.DelayedCall(this, nameof(_DeactivateDelayed), _delayTime);
                }
            }
        }

        public void _ActivateDelayed()
        {
            ToggleActive(true);
        }

        public void _DeactivateDelayed()
        {
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
