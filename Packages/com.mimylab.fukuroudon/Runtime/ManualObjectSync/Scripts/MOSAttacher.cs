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

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/MOS Attacher")]
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class MOSAttacher : UdonSharpBehaviour
    {
        public ManualObjectSync[] target = new ManualObjectSync[0];

        private void OnCollisionEnter(Collision other)
        {
            if (!Utilities.IsValid(other)) { return; }

            Snatche(other.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other)) { return; }

            Snatche(other);
        }

        private void Snatche(Collider collider)
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
