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
        private Transform _targetTransform;

        [Header("Follow Settings")]
        [SerializeField]
        private Transform _sourceTransform;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _weight = 1.0f;

        [SerializeField]
        private Vector3 _aimVector = Vector3.forward;

        [SerializeField]
        private Vector3 _upVector = Vector3.up;

        [SerializeField]
        private AimConstraint.WorldUpType _worldUpType = AimConstraint.WorldUpType.SceneUp;

        [SerializeField]
        private Vector3 _worldUpVector = Vector3.up;

        [SerializeField]
        private Transform _worldUpObject;

        [Header("Limit Settings")]
        [SerializeField]
        private AimLimitType _limitType = AimLimitType.Angle;

        [SerializeField, Range(0.0f, 180.0f)]
        private float _maxAngle = 180.0f;

        private Transform _parent;
        private Quaternion _rotationAtRest;

        private bool _isReachMaxAngle;

        private UdonBehaviour[] _eventReceivers;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            if (!_targetTransform)
            {
                _targetTransform = transform;
            }

            _parent = _targetTransform.parent;
            _rotationAtRest = _targetTransform.localRotation;

            _eventReceivers = transform.GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            // 範囲制限しつつ追従処理(ワールド空間)
            Quaternion rotation = _sourceTransform ? FollowRotation() : _targetTransform.rotation;

            Vector3 angles = Vector3.zero;
            switch (_limitType)
            {
                case AimLimitType.Angle:
                    rotation = AimAndLimitByAngle(rotation, out angles);
                    break;
                case AimLimitType.Polar:
                    rotation = AimAndLimitByPolar(rotation, out angles);
                    break;
            }

            // 結果を Transform へ反映
            _targetTransform.rotation = rotation;

            // 制限イベント
            SetIsReachMaxAngle(angles.x >= _maxAngle);
        }

        private Quaternion FollowRotation()
        {
            Quaternion targetRotation = _parent
                ? _parent.rotation * _rotationAtRest
                : _rotationAtRest;

            Quaternion sourceRotation;
            Vector3 forward = _sourceTransform.position - _targetTransform.position;
            switch (_worldUpType)
            {
                case AimConstraint.WorldUpType.SceneUp:
                    sourceRotation = Quaternion.LookRotation(forward, Vector3.up);
                    break;
                case AimConstraint.WorldUpType.ObjectUp:
                    sourceRotation = _worldUpObject
                        ? Quaternion.LookRotation(forward, _worldUpObject.position - _targetTransform.position)
                        : Quaternion.LookRotation(forward);
                    break;
                case AimConstraint.WorldUpType.ObjectRotationUp:
                    sourceRotation = _worldUpObject
                        ? Quaternion.LookRotation(forward, _worldUpObject.TransformDirection(_worldUpVector))
                        : Quaternion.LookRotation(forward);
                    break;
                case AimConstraint.WorldUpType.Vector:
                    sourceRotation = Quaternion.LookRotation(forward, _worldUpVector);
                    break;
                default:
                    sourceRotation = Quaternion.LookRotation(forward);
                    break;
            }

            return Quaternion.Slerp(targetRotation, sourceRotation, _weight);
        }

        private Quaternion AimAndLimitByAngle(Quaternion rotation, out Vector3 angles)
        {
            
            
            angles = Vector3.zero;
            return rotation;
        }

        private Quaternion AimAndLimitByPolar(Quaternion rotation, out Vector3 angles)
        {
            angles = Vector3.zero;
            return rotation;
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