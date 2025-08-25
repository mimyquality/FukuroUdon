/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-particlecollision")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by ParticleCollision")]
    [RequireComponent(typeof(ParticleSystem))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByParticleCollision : ActiveRelayBy
    {
        [SerializeField]
        private Collider[] _reactiveColliders = new Collider[0];

        private void OnParticleCollision(GameObject other)
        {
            if (!Utilities.IsValid(other)) { return; }

            var collider = other.GetComponent<Collider>();
            if (!Utilities.IsValid(collider)) { return; }

            if (CheckAccept(collider)) { DoAction(Networking.LocalPlayer); }

        }

        private bool CheckAccept(Collider collider)
        {
            if (_reactiveColliders.Length < 1) { return false; }

            return System.Array.IndexOf(_reactiveColliders, collider) > -1;
        }
    }
}
