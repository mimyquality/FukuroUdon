/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;

    abstract public class IViewPointReceiver : UdonSharpBehaviour
    {
        internal Transform viewPointTracker = null;

        public abstract void ReceiveViewPoint(Vector3 position, Quaternion rotation);

        public virtual void OnViewPointChanged()
        {
            if (viewPointTracker)
            {
                ReceiveViewPoint(viewPointTracker.position, viewPointTracker.rotation);
            }
        }
    }
}
