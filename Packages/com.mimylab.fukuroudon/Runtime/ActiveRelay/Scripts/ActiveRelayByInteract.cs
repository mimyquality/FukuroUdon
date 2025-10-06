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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-interact")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Interact")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class ActiveRelayByInteract : ActiveRelayBy
    {
        [SerializeField]
        private bool _localOnly = true;

        [UdonSynced]
        private bool[] sync_objectsActive = new bool[0];

        public override void OnPreSerialization()
        {
            if (sync_objectsActive.Length != _gameObjects.Length)
            {
                sync_objectsActive = new bool[_gameObjects.Length];
            }
            for (int i = 0; i < sync_objectsActive.Length; i++)
            {
                sync_objectsActive[i] = _gameObjects[i] && _gameObjects[i].activeSelf;
            }
        }

        public override void OnDeserialization()
        {
            if (_localOnly) { return; }
            if (_gameObjects.Length != sync_objectsActive.Length) { return; }

            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (!_gameObjects[i]) { continue; }

                if (_gameObjects[i].activeSelf != sync_objectsActive[i])
                {
                    _gameObjects[i].SetActive(sync_objectsActive[i]);
                }
            }
        }

        public override void Interact()
        {
            if (DoAction(Networking.LocalPlayer))
            {
                Sync();
            }
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
