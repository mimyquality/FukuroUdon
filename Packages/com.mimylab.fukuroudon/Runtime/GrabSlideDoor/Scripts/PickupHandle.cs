/*
Copyright (c) 2021 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
//using VRC.Udon;

namespace MimyLab
{
    [AddComponentMenu("Fukuro Udon/Pickup Handle")]
    [RequireComponent(typeof(VRCPickup))]
    public class PickupHandle : UdonSharpBehaviour
    {
        [SerializeField]
        Transform returnPoint = null;

        Vector3 returnPosition;
        Quaternion returnRotation;

        private void Start()
        {
            returnPosition = this.transform.position;
            returnRotation = this.transform.rotation;
            if (returnPoint)
            {
                returnPoint.SetPositionAndRotation(this.transform.position, this.transform.rotation);
            }

        }

        public override void OnPickup()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

        public override void OnDrop()
        {
            if (returnPoint)
            {
                this.transform.SetPositionAndRotation(returnPoint.position, returnPoint.rotation);
            }
            else
            {
                this.transform.SetPositionAndRotation(returnPosition, returnRotation);
            }
        }
    }
}