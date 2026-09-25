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

    public enum SourceAimType
    {
        ObjectAim,
        ObjectRotationAim,
        None
    }

    public enum AimLimitType
    {
        Angle,
        Polar
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-aim-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Aim Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedAimConstraint : LimitedConstraint
    {
        [SerializeField]
        private Transform _targetTransform;

        [Header("Constraint Settings")]
        [SerializeField]
        private Transform _sourceTransform;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _weight = 1.0f;

        [SerializeField]
        private Vector3 _aimVector = Vector3.forward;

        [SerializeField]
        private SourceAimType _sourceAimType = SourceAimType.ObjectAim;
        
        [SerializeField]
        private Vector3 _sourceAimVector = Vector3.forward;

        [Space]
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

        [SerializeField, Range(0f, 180f)]
        private float _maxAngle = 180f;

        [SerializeField, MinMaxRange(-180f, 180f)]
        private Vector2 _yawRange = new(-180f, 180f);

        [SerializeField, MinMaxRange(-180f, 180f)]
        private Vector2 _pitchRange = new(-180f, 180f);

        [SerializeField, MinMaxRange(-180f, 180f)]
        private Vector2 _rollRange = new(-180f, 180f);

        private Transform _parent;
        private Quaternion _rotationAtRest;
        private Quaternion _axisOffset;

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
            _axisOffset = Quaternion.LookRotation(_aimVector, _upVector);

            _eventReceivers = transform.GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            // 追従処理
            Quaternion rotation = _sourceTransform ? FollowRotation() : _targetTransform.localRotation;

            // 範囲制限処理(Up軸)
            Vector3 angles = Vector3.zero;
            rotation = LimitRoll(rotation, ref angles.z);

            // 範囲制限処理(Aim軸)
            switch (_limitType)
            {
                case AimLimitType.Angle:
                    rotation = LimitAimByAngle(rotation, ref angles.x);
                    break;
                case AimLimitType.Polar:
                    rotation = LimitAimByPolar(rotation, ref angles);
                    break;
            }

            // 結果を Transform へ反映
            _targetTransform.localRotation = rotation * Quaternion.Inverse(_axisOffset);

            // 制限イベント
            SetIsReachMaxAngle(angles.x >= _maxAngle);
        }

        private Quaternion FollowRotation()
        {
            // ワールド空間で計算
            Quaternion parentRotation = _parent ? _parent.rotation : Quaternion.identity;
            Quaternion baseRotation = parentRotation * _rotationAtRest * _axisOffset;

            Vector3 forward = baseRotation * Vector3.forward;
            switch (_sourceAimType)
            {
                case SourceAimType.ObjectAim:
                    forward = _sourceTransform.position - _targetTransform.position;
                    break;
                case SourceAimType.ObjectRotationAim:
                    forward = _sourceTransform.rotation * _sourceAimVector;
                    break;
            }
            
            Vector3 up = _targetTransform.TransformDirection(_upVector);
            switch (_worldUpType)
            {
                case AimConstraint.WorldUpType.SceneUp:
                    up = Vector3.up;
                    break;
                case AimConstraint.WorldUpType.ObjectUp:
                    up = _worldUpObject
                        ? _worldUpObject.position - _targetTransform.position
                        : up;
                    break;
                case AimConstraint.WorldUpType.ObjectRotationUp:
                    up = _worldUpObject
                        ? _worldUpObject.TransformDirection(_worldUpVector)
                        : up;
                    break;
                case AimConstraint.WorldUpType.Vector:
                    up = _worldUpVector;
                    break;
            }

            Quaternion sourceRotation = Quaternion.LookRotation(forward, up);

            // ローカル空間で返す (AxisOffset 込み)
            return Quaternion.Inverse(parentRotation) * Quaternion.Slerp(baseRotation, sourceRotation, _weight);
        }

        private Quaternion LimitRoll(Quaternion rotation, ref float angle)
        {
            if (_rollRange.y - _rollRange.x >= 360f) return rotation;

            Vector3 aimVector = rotation * Vector3.forward;
            Vector3 upVector = _rotationAtRest * _upVector;
            Quaternion aimRotation = Quaternion.LookRotation(aimVector, upVector);
            angle = Vector3.SignedAngle(aimRotation * Vector3.up, rotation * Vector3.up, aimVector);

            if (_rollRange.x <= angle && angle <= _rollRange.y) return rotation;

            angle = Mathf.Clamp(angle, _rollRange.x, _rollRange.y);

            return aimRotation * Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private Quaternion LimitAimByAngle(Quaternion rotation, ref float angle)
        {
            if (_maxAngle >= 180f) return rotation;

            Vector3 baseDirection = _rotationAtRest * _aimVector;
            Vector3 followDirection = rotation * Vector3.forward;
            Vector3 limitDirection =
                Vector3.RotateTowards(baseDirection, followDirection, _maxAngle * Mathf.Deg2Rad, 0f);

            angle = Vector3.Angle(baseDirection, followDirection);

            if (angle <= _maxAngle) return rotation;

            angle = _maxAngle;

            return Quaternion.FromToRotation(followDirection, limitDirection) * rotation;
        }

        private Quaternion LimitAimByPolar(Quaternion rotation, ref Vector3 angles)
        {
            //ToDo:Polar角度制限

            return Quaternion.LookRotation(_aimVector, _upVector);
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