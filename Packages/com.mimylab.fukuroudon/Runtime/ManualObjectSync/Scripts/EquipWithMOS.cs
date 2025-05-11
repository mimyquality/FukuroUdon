﻿/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Equip with MOS")]
    [RequireComponent(typeof(VRCPickup))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class EquipWithMOS : UdonSharpBehaviour
    {
        [SerializeField]
        private Transform snapPoint = null;
        [SerializeField]
        private ManualObjectSync target = null;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            if (!snapPoint) { snapPoint = this.transform; }
            if (!target) { target = GetComponent<ManualObjectSync>(); }
        }
#endif

        private void Start()
        {
            if (!snapPoint) { snapPoint = this.transform; }
            if (!target) { target = GetComponent<ManualObjectSync>(); }
        }

        public override void OnPickup()
        {
            if (!target) { return; }

            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ManualObjectSync.CallUnequip));
        }

        public override void OnPickupUseDown()
        {
            if (!target) { return; }
            if (target.IsEquiped) { return; }

            Networking.SetOwner(Networking.LocalPlayer, target.gameObject);
            var mostNearBone = MostNearBone(Networking.LocalPlayer);
            if (mostNearBone == HumanBodyBones.LastBone) { return; }

            target.Equip(mostNearBone);
        }

        private HumanBodyBones MostNearBone(VRCPlayerApi pl)
        {
            var result = HumanBodyBones.LastBone;
            var snapPosition = snapPoint.position;
            var mostNearSqrtDistance = float.PositiveInfinity;
            var lastBone = (int)HumanBodyBones.LastBone;
            for (int i = 0; i < lastBone; i++)
            {
                var bonePosition = pl.GetBonePosition((HumanBodyBones)i);
                if (bonePosition.Equals(Vector3.zero)) { continue; }

                var sqrtDistance = (snapPosition - bonePosition).sqrMagnitude;
                if (sqrtDistance < mostNearSqrtDistance)
                {
                    mostNearSqrtDistance = sqrtDistance;
                    result = (HumanBodyBones)i;
                }
            }

            return result;
        }
    }
}
