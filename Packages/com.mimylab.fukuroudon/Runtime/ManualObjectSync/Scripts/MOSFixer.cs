/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#mos-fixer")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MOSFixer : UdonSharpBehaviour
    {
        internal ManualObjectSync mos;
        internal Rigidbody rb;
        
        internal bool isHeld;
        internal bool isEquipped;
        internal bool isAttached;
        
        internal Vector3 syncPosition;
        internal Quaternion syncRotation;
        internal HumanBodyBones equipBone;
        
        internal Transform attachPoint;

        private void Reset()
        {
            enabled = false;
        }

        public override void PostLateUpdate()
        {
            if (isHeld)
            {
                Holding();
            }
            else if (isEquipped)
            {
                EquipingBone();
            }
            else if (isAttached)
            {
                Attaching();
            }
        }

        // _isHeld なら VRCPickup と Rigidbody が付いている
        private void Holding()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            if (!Utilities.IsValid(owner)) return;
            if (owner.isLocal) return;

            var pickupHandBone = owner.IsUserInVR()
                ? equipBone == HumanBodyBones.LeftHand
                    ? HumanBodyBones.LeftHand
                    : HumanBodyBones.RightHand
                : HumanBodyBones.Head;

            Vector3 handPosition = owner.GetBonePosition(pickupHandBone);
            Quaternion handRotation = owner.GetBoneRotation(pickupHandBone);
            if (handPosition.Equals(Vector3.zero) && handRotation.Equals(Quaternion.identity))
            {
                // ボーンが無い＝ジェネリックアバター
                handPosition = owner.GetPosition();
                handRotation = owner.GetRotation();
            }

            rb.Move(handPosition + handRotation * syncPosition, handRotation * syncRotation);
        }

        private void EquipingBone()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            if (!Utilities.IsValid(owner)) return;

            Vector3 bonePosition = owner.GetBonePosition(equipBone);
            Quaternion boneRotation = owner.GetBoneRotation(equipBone);
            if (bonePosition.Equals(Vector3.zero) && boneRotation.Equals(Quaternion.identity)) return;

            Vector3 equipPosition = bonePosition + (boneRotation * syncPosition);
            Quaternion equipRotation = boneRotation * syncRotation;

            transform.SetPositionAndRotation(equipPosition, equipRotation);
        }

        private void Attaching()
        {
            if(Utilities.IsValid(attachPoint = mos.attachPoint))
            {
                transform.SetPositionAndRotation(attachPoint.position, attachPoint.rotation);
            }
        }
    }
}