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
    using VRC.Udon;

    public enum SourceAimType
    {
        ObjectAim,
        ObjectRotationAim
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

        [SerializeField, Range(0.0f, 1.0f)]
        private float _smoothingFactor = 0.0f;

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
        private Transform _worldUpObject;

        [SerializeField]
        private Vector3 _worldUpVector = Vector3.up;

        [Header("Limit Settings")]
        [SerializeField]
        private AimLimitType _limitType = AimLimitType.Angle;

        [SerializeField, Range(0f, 180f)]
        private float _maxAngle = 180f;

        [SerializeField, MinMaxRange(-180f, 180f)]
        private Vector2 _yawRange = new(-180f, 180f);

        [SerializeField, MinMaxRange(-90f, 90f)]
        private Vector2 _pitchRange = new(-90f, 90f);

        private Vector3 _aimVector = Vector3.forward;

        private Transform _parent;
        private Quaternion _rotationAtRest;
        private Quaternion _axisOffset;
        private float _differential = 0.0f;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            if (!_targetTransform)
            {
                _targetTransform = transform;
            }

            if (_sourceTransform)
            {
                switch (_sourceAimType)
                {
                    case SourceAimType.ObjectAim:
                        _aimVector = _sourceTransform.position - _targetTransform.position;
                        break;
                    case SourceAimType.ObjectRotationAim:
                        _aimVector = _sourceTransform.rotation * _sourceAimVector;
                        break;
                }

                _aimVector = _targetTransform.InverseTransformDirection(_aimVector).normalized;
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
            Quaternion parentRotation = _parent ? _parent.rotation : Quaternion.identity;
            Quaternion baseRotation = parentRotation * _rotationAtRest * _axisOffset;

            // 追従処理 (Aim軸)
            Vector3 aimDirection =
                _sourceTransform ? FollowAimDirection(baseRotation) : _targetTransform.rotation * _aimVector;

            // 範囲制限処理
            Vector2 angles = Vector2.zero;
            switch (_limitType)
            {
                case AimLimitType.Angle:
                    aimDirection = LimitAimByAngle(baseRotation, aimDirection, ref angles.x);
                    break;
                case AimLimitType.Polar:
                    aimDirection = LimitAimByPolar(baseRotation, aimDirection, ref angles);
                    break;
            }

            // 追従処理 (up軸)
            Quaternion rotation = FollowUpDirection(aimDirection);

            // 結果を Transform へ反映
            _targetTransform.rotation = rotation * Quaternion.Inverse(_axisOffset);

            // 制限イベント
            switch (_limitType)
            {
                case AimLimitType.Angle:
                    SetIsReachMaxAngle(angles.x >= _maxAngle);
                    break;
                case AimLimitType.Polar:
                    SetIsReachMinPitch(angles.x <= _pitchRange.x);
                    SetIsReachMaxPitch(angles.x >= _pitchRange.y);
                    SetIsReachMinYaw(angles.y <= _yawRange.x);
                    SetIsReachMaxYaw(angles.y >= _yawRange.y);
                    break;
            }
        }

        private Vector3 FollowAimDirection(Quaternion baseRotation)
        {
            // ワールド空間で計算
            Vector3 baseDirection = baseRotation * Vector3.forward;

            Vector3 sourceDirection = baseDirection;
            switch (_sourceAimType)
            {
                case SourceAimType.ObjectAim:
                    sourceDirection = _sourceTransform.position - _targetTransform.position;
                    break;
                case SourceAimType.ObjectRotationAim:
                    sourceDirection = _sourceTransform.rotation * _sourceAimVector;
                    break;
            }

            Vector3 result = Vector3.Slerp(baseDirection, sourceDirection, _weight).normalized;
            
            Vector3 targetAim = _targetTransform.rotation * _aimVector;

            if (targetAim == result)
            {
                return targetAim;
            }

            if (_smoothingFactor > 0.0f)
            {
                float angle = Vector3.Angle(targetAim, result);
                angle = Mathf.SmoothDampAngle(0.0f, angle, ref _differential, _smoothingFactor);
                
                return Vector3.RotateTowards(targetAim, result, angle * Mathf.Deg2Rad, 1.0f);
            }

            return result;
        }

        private Quaternion FollowUpDirection(Vector3 aim)
        {
            // ワールド空間で計算
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

            return Quaternion.LookRotation(aim, up);
        }

        private Vector3 LimitAimByAngle(Quaternion baseRotation, Vector3 aimDirection, ref float angle)
        {
            if (_maxAngle >= 180f) return aimDirection;

            Vector3 baseDirection = baseRotation * Vector3.forward;
            Vector3 limitDirection = Vector3.RotateTowards(baseDirection, aimDirection, _maxAngle * Mathf.Deg2Rad, 0f);

            angle = Vector3.Angle(baseDirection, limitDirection);

            return limitDirection;
        }

        private Vector3 LimitAimByPolar(Quaternion baseRotation, Vector3 aimDirection, ref Vector2 angles)
        {
            Vector3 relativeDirection = Quaternion.Inverse(baseRotation) * aimDirection.normalized;
            float theta = Mathf.Acos(relativeDirection.y);
            float phi = Mathf.Atan2(relativeDirection.x, relativeDirection.z);

            float minPitch = (_pitchRange.x + 90f) * Mathf.Deg2Rad;
            float maxPitch = (_pitchRange.y + 90f) * Mathf.Deg2Rad;
            float minYaw = _yawRange.x * Mathf.Deg2Rad;
            float maxYaw = _yawRange.y * Mathf.Deg2Rad;

            if (minPitch <= theta && theta <= maxPitch && minYaw <= phi && phi <= maxYaw) return aimDirection;

            theta = Mathf.Clamp(theta, minPitch, maxPitch);
            phi = Mathf.Clamp(phi, minYaw, maxYaw);
            angles.x = theta * Mathf.Rad2Deg - 90f;
            angles.y = phi * Mathf.Rad2Deg;

            float sineTheta = Mathf.Sin(theta);
            relativeDirection.z = sineTheta * Mathf.Cos(phi);
            relativeDirection.x = sineTheta * Mathf.Sin(phi);
            relativeDirection.y = Mathf.Cos(theta);

            return baseRotation * relativeDirection;
        }
    }
}