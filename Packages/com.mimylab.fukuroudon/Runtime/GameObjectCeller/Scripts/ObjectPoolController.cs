/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.UdonNetworkCalling;
    using VRC.SDKBase;
    using VRC.Udon.Common.Interfaces;

    public enum ObjectPoolControllerSwitchType
    {
        Spawn,
        RandomSpawn,
        SpawnAll,
        Return,
        ReturnAll
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/ObjectPool Controller")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectPoolController : UdonSharpBehaviour
    {
        [SerializeField]
        ObjectPoolManager target = null;

        [Header("Settings")]
        [SerializeField]
        ObjectPoolControllerSwitchType switchType = default;

        public override void Interact()
        {
            if (!target) { return; }

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ExecuteObjectPoolEvent));
        }

        [NetworkCallable]
        public void ExecuteObjectPoolEvent()
        {
            if (!target) { return; }
            if (!Networking.IsOwner(target.gameObject)) { return; }

            switch (switchType)
            {
                case ObjectPoolControllerSwitchType.Spawn:
                    target.TryToSpawn();
                    break;
                case ObjectPoolControllerSwitchType.RandomSpawn:
                    target.Shuffle();
                    target.TryToSpawn();
                    break;
                case ObjectPoolControllerSwitchType.SpawnAll:
                    target.SpawnAll();
                    break;
                case ObjectPoolControllerSwitchType.Return:
                    target.Return();
                    break;
                case ObjectPoolControllerSwitchType.ReturnAll:
                    target.ReturnAll();
                    break;
            }
        }
    }
}
