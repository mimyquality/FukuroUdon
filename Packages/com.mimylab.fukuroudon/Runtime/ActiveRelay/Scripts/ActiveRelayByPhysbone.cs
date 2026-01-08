/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.Dynamics;
    using VRC.SDKBase;
    using VRC.SDK3.Dynamics.PhysBone.Components;
    using VRC.Udon.Common.Interfaces;

    public enum ActiveRelayByPhysboneType
    {
        PhysboneGrabAndRelease,
        PhysboneGrab,
        PhysboneRelease,
        PhysbonePoseAndUnpose,
        PhysbonePose,
        PhysboneUnpose
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-physbone")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay by/ActiveRelay by Physbone")]
    [RequireComponent(typeof(VRCPhysBone))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByPhysbone : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayByPhysboneType _eventType = default;
        [SerializeField]
        private NetworkEventTarget _acceptPlayerType = NetworkEventTarget.All;

        public override void OnPhysBoneGrabbed(PhysBoneGrabbedInfo physBoneInfo)
        {
            switch (_eventType)
            {
                case ActiveRelayByPhysboneType.PhysboneGrabAndRelease:
                case ActiveRelayByPhysboneType.PhysboneGrab:
                    var player = physBoneInfo.player;
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        public override void OnPhysBoneReleased(PhysBoneReleasedInfo physBoneInfo)
        {
            switch (_eventType)
            {
                case ActiveRelayByPhysboneType.PhysboneGrabAndRelease:
                case ActiveRelayByPhysboneType.PhysboneRelease:
                    var player = physBoneInfo.player;
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        public override void OnPhysBonePosed(PhysBonePosedInfo physBoneInfo)
        {
            switch (_eventType)
            {
                case ActiveRelayByPhysboneType.PhysbonePoseAndUnpose:
                case ActiveRelayByPhysboneType.PhysbonePose:
                    var player = physBoneInfo.player;
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
        }

        public override void OnPhysBoneUnPosed(PhysBoneUnPosedInfo physBoneInfo)
        {
            switch (_eventType)
            {
                case ActiveRelayByPhysboneType.PhysbonePoseAndUnpose:
                case ActiveRelayByPhysboneType.PhysboneUnpose:
                    var player = physBoneInfo.player;
                    if (CheckAccept(player)) { DoAction(player); }
                    break;
            }
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
