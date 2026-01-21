/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    public enum ActiveRelayActivateType
    {
        ToggleActive,
        Activate,
        Deactivate
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ActiveRelayBy : UdonSharpBehaviour
    {
        public ActiveRelayBy[] _activeRelays = new ActiveRelayBy[0];
        public string[] allowedPlayerNameList = new string[0];

        [SerializeField]
        private protected GameObject[] _gameObjects = new GameObject[0];
        [SerializeField]
        private protected ActiveRelayActivateType _actionType = default;

        private protected bool DoAction(VRCPlayerApi player)
        {
            if (allowedPlayerNameList.Length > 0)
            {
                if (System.Array.IndexOf(allowedPlayerNameList, player.displayName) < 0)
                {
                    return false;
                }
            }

            switch (_actionType)
            {
                case ActiveRelayActivateType.ToggleActive: ToggleActive(); break;
                case ActiveRelayActivateType.Activate: SetActive(true); break;
                case ActiveRelayActivateType.Deactivate: SetActive(false); break;
            }

            return true;
        }

        private void ToggleActive()
        {
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (_gameObjects[i])
                {
                    _gameObjects[i].SetActive(!_gameObjects[i].activeSelf);
                }
            }
        }

        private void SetActive(bool value)
        {
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (_gameObjects[i])
                {
                    _gameObjects[i].SetActive(value);
                }
            }
        }
    }
}
