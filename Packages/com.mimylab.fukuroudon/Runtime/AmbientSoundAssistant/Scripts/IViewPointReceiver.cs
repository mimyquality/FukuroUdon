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

    public class IViewPointReceiver : UdonSharpBehaviour
    {
        public virtual void ReceiveViewPoint(Vector3 position, Quaternion rotation) { }
    }
}
