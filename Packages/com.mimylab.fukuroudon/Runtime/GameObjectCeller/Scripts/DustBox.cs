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
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/Dust Box")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DustBox : UdonSharpBehaviour
    {
        [SerializeField]
        private ObjectPoolManager target = null;

        private void OnTriggerEnter(Collider other)
        {
            if (!target) { return; }

            if (!Utilities.IsValid(other)) { return; }
            var incommingObject = other.gameObject;
            var index = System.Array.IndexOf(target.Pool, incommingObject);
            if (index < 0) { return; }

            var pickup = incommingObject.GetComponent<VRCPickup>();
            if (pickup) { pickup.Drop(); }

            // リターン前に位置リセットしておく
            var objectSync = incommingObject.GetComponent<VRCObjectSync>();
            var rigidbody = incommingObject.GetComponent<Rigidbody>();
            if (objectSync)
            {
                if (Networking.IsOwner(incommingObject))
                {
                    objectSync.Respawn();
                }
            }
            else if (rigidbody)
            {
                rigidbody.position = target.StartPositions[index];
                rigidbody.rotation = target.StartRotations[index];
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.Sleep();
            }
            else
            {
                incommingObject.transform.SetPositionAndRotation(target.StartPositions[index], target.StartRotations[index]);
            }

            target.Return(incommingObject);
        }
    }
}
