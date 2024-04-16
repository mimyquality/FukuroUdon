/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
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

    [AddComponentMenu("Fukuro Udon/Active Relay/Active Relay to GameObject")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayToGameObject : UdonSharpBehaviour
    {
        [SerializeField]
        ActiveRelayEventType _eventType = default;
        [SerializeField]
        GameObject[] _gameObjects = new GameObject[0];
        [SerializeField]
        bool _invert = false;


        void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                ToggleActive(!_invert);
            }
        }

        void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                ToggleActive(_invert);
            }
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
