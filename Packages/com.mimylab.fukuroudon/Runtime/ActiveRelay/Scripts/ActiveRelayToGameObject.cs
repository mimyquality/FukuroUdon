/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    public enum ActiveRelayEventType
    {
        ActiveAndInactive,
        Active,
        Inactive,
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to GameObject")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToGameObject : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];
        [SerializeField]
        private bool _invert = false;
        [SerializeField, Min(0.0f), Tooltip("sec, No delay if 0")]
        private float _delayTime = 0.0f;

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                if (_delayTime > 0.0f)
                {
                    if (_invert)
                    {
                        SendCustomEventDelayedSeconds(nameof(_DeactivateDelayed), _delayTime);
                    }
                    else
                    {
                        SendCustomEventDelayedSeconds(nameof(_ActivateDelayerd), _delayTime);
                    }
                    return;
                }

                ToggleActive(!_invert);
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                if (_delayTime > 0.0f)
                {
                    if (_invert)
                    {
                        SendCustomEventDelayedSeconds(nameof(_ActivateDelayerd), _delayTime);
                    }
                    else
                    {
                        SendCustomEventDelayedSeconds(nameof(_DeactivateDelayed), _delayTime);
                    }
                    return;
                }

                ToggleActive(_invert);
            }
        }

        public void _ActivateDelayerd()
        {
            ToggleActive(true);
        }

        public void _DeactivateDelayed()
        {
            ToggleActive(false);
        }

        private void ToggleActive(bool value)
        {
            foreach (var item in _gameObjects)
            {
                if (!item) { continue; }

                item.SetActive(value);
            }
        }
    }
}
