/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase.Editor.Attributes;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-gameobject")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to GameObject")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToGameObject : ActiveRelayTo
    {
        [SerializeField]
        private ActiveRelayActiveEvent _eventType = default;
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

        private protected override void OnEnable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Active)
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
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Inactive)
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
            foreach (GameObject item in _gameObjects)
            {
                if (!item) { continue; }

                item.SetActive(value);
            }
        }
    }
}
