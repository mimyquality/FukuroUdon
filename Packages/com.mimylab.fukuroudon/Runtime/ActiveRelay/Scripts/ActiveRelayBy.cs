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

    public enum ActiveRelayActivateType
    {
        ToggleActive,
        Activate,
        Inactivate
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayBy : UdonSharpBehaviour
    {
        [SerializeField]
        protected GameObject[] _gameObjects = new GameObject[0];
        [SerializeField]
        protected ActiveRelayActivateType _actionType = default;

        public void DoAction()
        {
            switch (_actionType)
            {
                case ActiveRelayActivateType.ToggleActive: ToggleActive(); break;
                case ActiveRelayActivateType.Activate: SetActive(true); break;
                case ActiveRelayActivateType.Inactivate: SetActive(false); break;
            }
        }

        protected void ToggleActive()
        {
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (_gameObjects[i])
                {
                    _gameObjects[i].SetActive(!_gameObjects[i].activeSelf);
                }
            }
        }

        protected void SetActive(bool value)
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
