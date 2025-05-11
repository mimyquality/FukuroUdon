/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
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
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
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

            switch (switchType)
            {
                case ObjectPoolControllerSwitchType.Spawn:
                    target.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ObjectPoolManager.CallTryToSpawn));
                    break;
                case ObjectPoolControllerSwitchType.RandomSpawn:
                    target.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ObjectPoolManager.CallShuffle));
                    target.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ObjectPoolManager.CallTryToSpawn));
                    break;
                case ObjectPoolControllerSwitchType.SpawnAll:
                    target.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ObjectPoolManager.CallSpawnAll));
                    break;
                case ObjectPoolControllerSwitchType.Return:
                    target.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ObjectPoolManager.CallReturnFirst));
                    break;
                case ObjectPoolControllerSwitchType.ReturnAll:
                    target.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ObjectPoolManager.CallReturnAll));
                    break;
            }
        }
    }
}
