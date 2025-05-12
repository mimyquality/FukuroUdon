/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/Dust Box Return Trigger")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DustBoxReturnTrigger : UdonSharpBehaviour
    {
        public GameObject returnTarget = null;
    }
}
