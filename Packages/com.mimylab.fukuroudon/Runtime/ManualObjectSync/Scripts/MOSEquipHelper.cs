/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Components;
    using VRC.SDKBase;
    using VRC.Udon.Common.Interfaces;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#mos-equip-helper")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/MOS Equip Helper")]
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class MOSEquipHelper : UdonSharpBehaviour
    {
        [SerializeField]
        private Transform snapPoint = null;

        [SerializeField]
        private ManualObjectSync target = null;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            if (!snapPoint)
            {
                snapPoint = transform;
            }

            if (!target)
            {
                target = GetComponent<ManualObjectSync>();
            }
        }
#endif

        private void Start()
        {
            if (!snapPoint)
            {
                snapPoint = transform;
            }

            if (!target)
            {
                target = GetComponent<ManualObjectSync>();
            }
        }

        public override void OnPickup()
        {
            if (!target) return;

            target.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ManualObjectSync.Unequip));
        }

        public override void OnPickupUseDown()
        {
            if (!target) return;
            if (target.IsEquipped) return;

            Networking.SetOwner(Networking.LocalPlayer, target.gameObject);
            HumanBodyBones nearestBone = GetNearestBone(Networking.LocalPlayer);
            if (nearestBone == HumanBodyBones.LastBone) return;

            Vector3 bonePosition = Networking.LocalPlayer.GetBonePosition(nearestBone);
            Quaternion boneRotation = Networking.LocalPlayer.GetBoneRotation(nearestBone);
            if (bonePosition.Equals(Vector3.zero) && boneRotation.Equals(Quaternion.identity)) return;

            target.Equip
            (
                nearestBone, 
                Quaternion.Inverse(boneRotation) * (target.transform.position - bonePosition),
                Quaternion.Inverse(boneRotation) * target.transform.rotation
            );
        }

        private HumanBodyBones GetNearestBone(VRCPlayerApi player)
        {
            var result = HumanBodyBones.LastBone;

            Vector3 snapPosition = snapPoint.position;
            var nearestSqrtDistance = float.PositiveInfinity;
            var lastBone = (int)HumanBodyBones.LastBone;
            for (int currentBone = 0; currentBone < lastBone; currentBone++)
            {
                Vector3 bonePosition = player.GetBonePosition((HumanBodyBones)currentBone);
                if (bonePosition.Equals(Vector3.zero)) continue;

                float sqrtDistance = (snapPosition - bonePosition).sqrMagnitude;
                if (sqrtDistance < nearestSqrtDistance)
                {
                    nearestSqrtDistance = sqrtDistance;
                    result = (HumanBodyBones)currentBone;
                }
            }

            return result;
        }
    }
}