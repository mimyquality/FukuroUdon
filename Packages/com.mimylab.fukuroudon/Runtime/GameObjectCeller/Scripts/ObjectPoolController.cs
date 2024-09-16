/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

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
                case ObjectPoolControllerSwitchType.Spawn: Spawn(); break;
                case ObjectPoolControllerSwitchType.RandomSpawn: RandomSpawn(); break;
                case ObjectPoolControllerSwitchType.SpawnAll: SpawnAll(); break;
                case ObjectPoolControllerSwitchType.Return: ReturnFirst(); break;
                case ObjectPoolControllerSwitchType.ReturnAll: ReturnAll(); break;
            }
        }

        void Spawn()
        {
            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(target.TryToSpawn));
        }

        void RandomSpawn()
        {
            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(target.Shuffle));
            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(target.TryToSpawn));
        }

        void SpawnAll()
        {
            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(target.SpawnAll));
        }

        void ReturnFirst()
        {
            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(target.Return));
        }

        void ReturnAll()
        {
            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(target.ReturnAll));
        }
    }
}
