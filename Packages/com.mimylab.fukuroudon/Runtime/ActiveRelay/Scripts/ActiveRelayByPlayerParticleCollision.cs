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
    using VRC.Udon.Common.Interfaces;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-playerparticlecollision")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Player ParticleCollision")]
    [RequireComponent(typeof(ParticleSystem))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByPlayerParticleCollision : ActiveRelayBy
    {
        [SerializeField]
        private NetworkEventTarget _acceptPlayerType = NetworkEventTarget.All;

        public override void OnPlayerParticleCollision(VRCPlayerApi player)
        {
            if (CheckAccept(player)) { DoAction(player); }
        }

        private bool CheckAccept(VRCPlayerApi player)
        {
            switch (_acceptPlayerType)
            {
                case NetworkEventTarget.All:
                    return true;
                case NetworkEventTarget.Owner:
                    return player.IsOwner(this.gameObject);
                case NetworkEventTarget.Others:
                    return !player.isLocal;
                case NetworkEventTarget.Self:
                    return player.isLocal;
                default:
                    return false;
            }
        }
    }
}
