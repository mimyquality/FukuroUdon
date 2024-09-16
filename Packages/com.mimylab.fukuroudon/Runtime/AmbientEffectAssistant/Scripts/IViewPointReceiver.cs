/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    [Icon(ComponentIconPath.FukuroUdon)]
    abstract public class IViewPointReceiver : UdonSharpBehaviour
    {
        public abstract void ReceiveViewPoint(Vector3 position, Quaternion rotation);
    }
}
