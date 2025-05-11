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
    using VRC.Udon.Common.Interfaces;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/Dust Box")]
    [RequireComponent(typeof(Collider))]
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
            if (index < 0)
            {
                var returnEventTrigger = incommingObject.GetComponent<DustBoxReturnTrigger>();
                if (!returnEventTrigger) { return; }

                incommingObject = returnEventTrigger.returnTarget;
                if (!incommingObject) { return; }

                index = System.Array.IndexOf(target.Pool, incommingObject);
                if (index < 0) { return; }
            }

            var pickup = incommingObject.GetComponent<VRCPickup>();
            if (pickup) { pickup.Drop(); }

            // リターン前にオブジェクトの位置を戻しておく
            var rigidbody = incommingObject.GetComponent<Rigidbody>();
            if (rigidbody)
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

            if (Networking.IsOwner(incommingObject))
            {
                var objectSync = incommingObject.GetComponent<VRCObjectSync>();
                if (objectSync) { objectSync.Respawn(); }

                target.SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(ObjectPoolManager.CallReturn), index);
            }
        }
    }
}
