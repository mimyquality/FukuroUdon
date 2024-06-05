/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Interact")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByInteract : ActiveRelayBy
    {
        public override void Interact()
        {
            DoAction();
        }
    }
}
