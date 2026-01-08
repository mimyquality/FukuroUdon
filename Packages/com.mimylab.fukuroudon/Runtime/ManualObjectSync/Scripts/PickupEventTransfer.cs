/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDK3.Components;
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#pickupevent-transfer")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Pickup Event Transfer")]
    //[RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class PickupEventTransfer : UdonSharpBehaviour
    {
        [SerializeField]
        private UdonBehaviour target = null;

        public override void Interact()
        {
            //if (target) { target.Interact(); }
            //if (target) { target.SendCustomEvent(nameof(UdonBehaviour.Interact)); }
            if (target) { target.SendCustomEvent("_interact"); }
        }

        public override void OnPickup()
        {
            //if (target) { target.OnPickup(); }
            //if (target) { target.SendCustomEvent(nameof(UdonBehaviour.OnPickup)); }
            if (target) { target.SendCustomEvent("_onPickup"); }
        }

        public override void OnPickupUseDown()
        {
            //if (target) { target.OnPickupUseDown(); }
            //if (target) { target.SendCustomEvent(nameof(UdonBehaviour.OnPickupUseDown)); }
            if (target) { target.SendCustomEvent("_onPickupUseDown"); }
        }

        public override void OnPickupUseUp()
        {
            //if (target) { target.OnPickupUseUp(); }
            //if (target) { target.SendCustomEvent(nameof(UdonBehaviour.OnPickupUseUp)); }
            if (target) { target.SendCustomEvent("_onPickupUseUp"); }
        }

        public override void OnDrop()
        {
            //if (target) { target.OnDrop(); }
            //if (target) { target.SendCustomEvent(nameof(UdonBehaviour.OnDrop)); }
            if (target) { target.SendCustomEvent("_onDrop"); }
        }
    }
}
