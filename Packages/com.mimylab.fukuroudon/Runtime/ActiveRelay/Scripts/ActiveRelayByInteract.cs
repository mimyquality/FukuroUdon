﻿/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Interact")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class ActiveRelayByInteract : ActiveRelayBy
    {
        [SerializeField]
        private bool _localOnly = true;

        [UdonSynced]
        private bool[] sync_objectsActive = new bool[0];

        private void Start()
        {
            sync_objectsActive = new bool[_gameObjects.Length];
            for (int i = 0; i < sync_objectsActive.Length; i++)
            {
                sync_objectsActive[i] = _gameObjects[i].activeSelf;
            }
        }

        public override void OnPreSerialization()
        {
            for (int i = 0; i < sync_objectsActive.Length; i++)
            {
                sync_objectsActive[i] = _gameObjects[i].activeSelf;
            }
        }

        public override void OnDeserialization()
        {
            if (_localOnly) { return; }

            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (_gameObjects[i].activeSelf != sync_objectsActive[i])
                {
                    _gameObjects[i].SetActive(sync_objectsActive[i]);
                }
            }
        }


        public override void Interact()
        {
            DoAction();
            Sync();
        }

        private void Sync()
        {
            if (_localOnly) { return; }

            if (!Networking.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
            RequestSerialization();
        }
    }
}
