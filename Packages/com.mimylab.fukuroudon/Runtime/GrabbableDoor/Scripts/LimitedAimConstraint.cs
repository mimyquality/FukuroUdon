/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.Animations;
    using VRC.SDKBase;
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-aim-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Aim Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedAimConstraint : LimitedConstraint
    {
        [SerializeField]
        private Transform targetTransform;

        [Header("Follow Settings")]
        [SerializeField]
        private Transform sourceTransform;

        [SerializeField, Range(0.0f, 1.0f)]
        private float weight = 1.0f;

        [SerializeField]
        private Vector3 aimVector = Vector3.forward;

        [SerializeField]
        private Vector3 upVector = Vector3.up;

        [SerializeField]
        private AimConstraint.WorldUpType worldUpType = AimConstraint.WorldUpType.SceneUp;

        [SerializeField]
        private Vector3 worldUpVector = Vector3.up;

        [SerializeField]
        private Transform worldUpObject;

        [Header("Limit Settings")]
        [SerializeField]
        private RotationLimitType limitType = RotationLimitType.Angle;

        [SerializeField]
        private Vector3 limitBaseVector = Vector3.forward;

        [SerializeField, Range(0.0f, 180.0f)]
        private float maxAngle = 180.0f;

        private Transform _parent;
        private Quaternion _rotationAtRest;

        private bool _isReachMaxAngle;

        private UdonBehaviour[] _eventReceivers;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            if (!targetTransform)
            {
                targetTransform = transform;
            }

            _parent = targetTransform.parent;
            _rotationAtRest = targetTransform.localRotation;

            _eventReceivers = transform.GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            // ワールド空間で計算
            Quaternion rotation = sourceTransform ? FollowRotation() : targetTransform.rotation;

            // ToDo:角度計算

            // 結果を Transform へ反映
            targetTransform.rotation = rotation;

            // 制限イベント
            SetIsReachMaxAngle(false);
        }

        private Quaternion FollowRotation()
        {
            Quaternion targetRotation = _parent
                ? _parent.rotation * _rotationAtRest
                : _rotationAtRest;

            Quaternion sourceRotation;
            Vector3 forward = sourceTransform.position - targetTransform.position;
            switch (worldUpType)
            {
                case AimConstraint.WorldUpType.SceneUp:
                    sourceRotation = Quaternion.LookRotation(forward, Vector3.up);
                    break;
                case AimConstraint.WorldUpType.ObjectUp:
                    sourceRotation = worldUpObject
                        ? Quaternion.LookRotation(forward, worldUpObject.position - targetTransform.position)
                        : Quaternion.LookRotation(forward);
                    break;
                case AimConstraint.WorldUpType.ObjectRotationUp:
                    sourceRotation = worldUpObject
                        ? Quaternion.LookRotation(forward, worldUpObject.TransformDirection(worldUpVector))
                        : Quaternion.LookRotation(forward);
                    break;
                case AimConstraint.WorldUpType.Vector:
                    sourceRotation = Quaternion.LookRotation(forward, worldUpVector);
                    break;
                default:
                    sourceRotation = Quaternion.LookRotation(forward);
                    break;
            }

            return Quaternion.Slerp(targetRotation, sourceRotation, weight);
        }

        private void SetIsReachMaxAngle(bool value)
        {
            if (_isReachMaxAngle != value)
            {
                SendLimitEndEvent(value ? "OnReachedMaxAngle" : "OnDepartedMaxAngle");

                _isReachMaxAngle = value;
            }
        }

        private void SendLimitEndEvent(string eventName)
        {
            for (int i = 0; i < _eventReceivers.Length; i++)
            {
                if (!Utilities.IsValid(_eventReceivers[i])) continue;
                if (_eventReceivers[i] == this) continue;

                _eventReceivers[i].SendCustomEvent(eventName);
            }
        }
    }
}