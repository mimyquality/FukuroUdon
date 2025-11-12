/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/GameObject-Celler#dust-box-return-trigger")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/Dust Box Return Trigger")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DustBoxReturnTrigger : UdonSharpBehaviour
    {
        public GameObject returnTarget = null;
    }
}
