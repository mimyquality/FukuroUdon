/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace MimyLab
{
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
            target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(target.Unequip));
        }

        public override void OnPickupUseDown()
        {
            if (target.IsEquiped) { return; }

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(EquipWithTarget));
        }

        public void EquipWithTarget()
        {
            if (!Networking.IsOwner(target.gameObject)) { return; }

            var snapPosition = snapPoint.position;
            var localPlayer = Networking.LocalPlayer;
            var bonePosition = Vector3.zero;
            var sqrtDistance = 0.0f;
            var mostNearSqrtDistance = float.PositiveInfinity;
            var mostNearBone = HumanBodyBones.LastBone;
            var lastBone = (int)HumanBodyBones.LastBone;
            for (int i = 0; i < lastBone; i++)
            {
                bonePosition = localPlayer.GetBonePosition((HumanBodyBones)i);
                if (bonePosition.Equals(Vector3.zero)) { continue; }

                sqrtDistance = (snapPosition - bonePosition).sqrMagnitude;
                if (sqrtDistance < mostNearSqrtDistance)
                {
                    mostNearSqrtDistance = sqrtDistance;
                    mostNearBone = (HumanBodyBones)i;
                }
            }
            if (mostNearBone == HumanBodyBones.LastBone) { return; }

            target.Equip(mostNearBone);
        }
    }
}
