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
        private const float PickupSmoothingFactor = 0.02f;

        internal Vector3 syncPosition;
        internal Quaternion syncRotation;
        internal HumanBodyBones equipBone;

        private ManualObjectSync _mos;
        private Rigidbody _rigidbody;
        private Transform _attachPoint;
        private bool _isHeld;
        private bool _isEquipped;
        private bool _isAttached;
        private Vector3 _smoothSyncPosition;
        private Quaternion _smoothSyncRotation;
        private Vector3 _syncVelocity;
        private float _angleDifferential;

        public bool IsHeld
        {
            set
            {
                if (!_isHeld && value)
                {
                    _smoothSyncPosition = syncPosition;
                    _smoothSyncRotation = syncRotation;
                }

                _isHeld = value;
            }
        }

        public bool IsEquipped
        {
            set => _isEquipped = value;
        }

        public bool IsAttached
        {
            set => _isAttached = value;
        }

        private void Reset()
        {
            enabled = false;
        }

        private void Start()
        {
            _mos = GetComponent<ManualObjectSync>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void PostLateUpdate()
        {
            if (_isHeld)
            {
                Holding();
            }
            else if (_isEquipped)
            {
                EquipingBone();
            }
            else if (_isAttached)
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

            if (_smoothSyncPosition != syncPosition)
            {
                _smoothSyncPosition =
                    Vector3.SmoothDamp(_smoothSyncPosition, syncPosition, ref _syncVelocity, PickupSmoothingFactor);
            }

            if (_smoothSyncRotation != syncRotation)
            {
                float angle = Quaternion.Angle(syncRotation, _smoothSyncRotation);
                angle = Mathf.SmoothDampAngle(angle, 0.0f, ref _angleDifferential, PickupSmoothingFactor);
                _smoothSyncRotation = Quaternion.RotateTowards(syncRotation, _smoothSyncRotation, angle);
            }

            Vector3 pickupPosition = handPosition + handRotation * _smoothSyncPosition;
            Quaternion pickupRotation = handRotation * _smoothSyncRotation;

            _rigidbody.Move(pickupPosition, pickupRotation);
        }

        private void EquipingBone()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            if (!Utilities.IsValid(owner)) return;

            Vector3 bonePosition = owner.GetBonePosition(equipBone);
            Quaternion boneRotation = owner.GetBoneRotation(equipBone);
            if (bonePosition.Equals(Vector3.zero) && boneRotation.Equals(Quaternion.identity)) return;

            Vector3 equipPosition = bonePosition + boneRotation * syncPosition;
            Quaternion equipRotation = boneRotation * syncRotation;

            transform.SetPositionAndRotation(equipPosition, equipRotation);
        }

        private void Attaching()
        {
            if (Utilities.IsValid(_attachPoint = _mos.attachPoint))
            {
                Vector3 attachPosition = _attachPoint.position;
                Quaternion attachRotation = _attachPoint.rotation;

                Vector3 offsetPosition = attachPosition + attachRotation * syncPosition;
                Quaternion offsetRotation = attachRotation * syncRotation;

                transform.SetPositionAndRotation(offsetPosition, offsetRotation);
            }
        }
    }
}