/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    
    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#mos-attach-trigger")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/MOS Attach Trigger")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MOSAttachTrigger : UdonSharpBehaviour
    {
        public ManualObjectSync[] target = System.Array.Empty<ManualObjectSync>();

        private void OnCollisionEnter(Collision other)
        {
            if (!Utilities.IsValid(other)) { return; }

            Snatch(other.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) { return; }

            Snatch(other);
        }

        private void Snatch(Collider collider)
        {
            if (!Networking.IsOwner(collider.gameObject)) { return; }

            var prospect = collider.GetComponent<ManualObjectSync>();
            if (!prospect) { return; }

            for (int i = 0; i < target.Length; i++)
            {
                if (prospect == target[i])
                {
                    prospect.Attach();
                }
            }
        }
    }
}
