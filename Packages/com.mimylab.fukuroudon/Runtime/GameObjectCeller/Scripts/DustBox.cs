/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/Dust Box")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DustBox : UdonSharpBehaviour
    {
        [SerializeField]
        ObjectPoolManager target = null;

        [Tooltip("Set a value upper than world Respawn Hight Y.")]
        public Vector3 respawnPoint = new Vector3(0f, -99f, 0f);

        void OnTriggerEnter(Collider other)
        {
            if (!target) { return; }
            if (!Utilities.IsValid(other)) { return; }

            var incommingObject = other.gameObject;
            if (!Networking.IsOwner(incommingObject)) { return; }

            Networking.SetOwner(Networking.LocalPlayer, target.gameObject);
            var pool = target.Pool;
            for (int i = 0; i < pool.Length; i++)
            {
                if (incommingObject == pool[i])
                {
                    var pickup = incommingObject.GetComponent<VRCPickup>();
                    var objectSync = incommingObject.GetComponent<VRCObjectSync>();
                    if (pickup) { pickup.Drop(); }
                    if (objectSync) { objectSync.FlagDiscontinuity(); }

                    incommingObject.transform.position = respawnPoint;
                    target.Return(incommingObject);

                    break;
                }
            }
        }
    }
}
