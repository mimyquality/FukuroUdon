﻿/*
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
        private ObjectPoolManager target = null;

        [Tooltip("Set a value upper than world Respawn Hight Y.")]
        public Vector3 respawnPoint = new Vector3(0f, -99f, 0f);

        private void OnTriggerEnter(Collider other)
        {
            if (!target) { return; }
            if (!Utilities.IsValid(other)) { return; }

            var incommingObject = other.gameObject;
            if (!Networking.IsOwner(incommingObject)) { return; }

            if (System.Array.IndexOf(target.Pool, incommingObject) > -1)
            {
                var pickup = incommingObject.GetComponent<VRCPickup>();
                var objectSync = incommingObject.GetComponent<VRCObjectSync>();
                if (pickup) { pickup.Drop(); }
                if (objectSync) { objectSync.FlagDiscontinuity(); }
                incommingObject.transform.position = respawnPoint;

                Networking.SetOwner(Networking.LocalPlayer, target.gameObject);
                target.Return(incommingObject);
            }
        }
    }
}
