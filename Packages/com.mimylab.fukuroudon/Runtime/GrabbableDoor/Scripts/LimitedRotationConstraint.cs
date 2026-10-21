/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.Udon;

    public enum RotationLimitType
    {
        Rotate,
        EulerAngles
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-rotation-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Rotation Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedRotationConstraint : LimitedConstraint
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
        private bool _solveInLocalSpace = false;

        [Header("Limit Settings")]
        [SerializeField]
        private RotationLimitType _limitType = RotationLimitType.Rotate;

        [SerializeField, Range(0f, 180f)]
        private float _maxAngle = 180f;

        [Tooltip("下限から上限までの範囲が360°以上なら無制限扱いになります。")]
        [SerializeField, MinMaxRange(-360f, 360f)]
        private Vector2 _xAxisRange = new(-180f, 180f);

        [Tooltip("下限から上限までの範囲が360°以上なら無制限扱いになります。")]
        [SerializeField, MinMaxRange(-360f, 360f)]
        private Vector2 _yAxisRange = new(-180f, 180f);

        [Tooltip("下限から上限までの範囲が360°以上なら無制限扱いになります。")]
        [SerializeField, MinMaxRange(-360f, 360f)]
        private Vector2 _zAxisRange = new(-180f, 180f);

        [SerializeField]
        private Space _relativeTo = Space.Self;

        private Transform _parent;
        private Quaternion _rotationAtRest;
        private Quaternion _rotationOffset;
        private Quaternion _startRotation;
        private float _differential = 0.0f;

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
            _startRotation = _targetTransform.rotation;
            if (_sourceTransform)
            {
                Quaternion sourceRotation = _solveInLocalSpace
                    ? _sourceTransform.localRotation
                    : _parent
                        ? Quaternion.Inverse(_parent.rotation) * _sourceTransform.rotation
                        : _sourceTransform.rotation;
                _rotationOffset = Quaternion.Inverse(sourceRotation) * _rotationAtRest;
            }

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

            // 範囲制限処理
            if (_relativeTo == Space.World && _parent)
            {
                rotation = _parent.rotation * rotation;
            }

            Vector3 angles = Vector3.zero;
            switch (_limitType)
            {
                case RotationLimitType.Rotate:
                    rotation = LimitRotationByAngle(rotation, ref angles.x);
                    break;
                case RotationLimitType.EulerAngles:
                    rotation = LimitRotationByAxes(rotation, out angles);
                    break;
            }

            // 結果を Transform へ反映
            if (_relativeTo == Space.World)
            {
                _targetTransform.rotation = rotation;
            }
            else
            {
                _targetTransform.localRotation = rotation;
            }

            // 制限イベント
            switch (_limitType)
            {
                case RotationLimitType.Rotate:
                    SetIsReachMaxAngle(angles.x >= _maxAngle);
                    break;
                case RotationLimitType.EulerAngles:
                    SetIsReachMinX(angles.x <= _xAxisRange.x);
                    SetIsReachMaxX(angles.x >= _xAxisRange.y);
                    SetIsReachMinY(angles.y <= _yAxisRange.x);
                    SetIsReachMaxY(angles.y >= _yAxisRange.y);
                    SetIsReachMinZ(angles.z <= _zAxisRange.x);
                    SetIsReachMaxZ(angles.z >= _zAxisRange.y);
                    break;
            }
        }

        private Quaternion FollowRotation()
        {
            Quaternion sourceRotation = _solveInLocalSpace
                ? _sourceTransform.localRotation
                : _parent
                    ? Quaternion.Inverse(_parent.rotation) * _sourceTransform.rotation
                    : _sourceTransform.rotation;
            sourceRotation *= _rotationOffset;

            Quaternion result = Quaternion.Slerp(_rotationAtRest, sourceRotation, _weight);
            
            Quaternion targetRotation = _targetTransform.localRotation;

            if (targetRotation == result)
            {
                return targetRotation;
            }

            if (_smoothingFactor > 0.0f)
            {
                float angle = Quaternion.Angle(targetRotation, result);
                angle = Mathf.SmoothDampAngle(0.0f, angle, ref _differential, _smoothingFactor);

                return Quaternion.RotateTowards(targetRotation, result, angle);
            }

            return result;
        }

        private Quaternion LimitRotationByAngle(Quaternion rotation, ref float angle)
        {
            if (_maxAngle >= 180f) return rotation;

            Quaternion rotationAtRest = _relativeTo == Space.World ? _startRotation : _rotationAtRest;
            angle = Mathf.Clamp(Quaternion.Angle(rotationAtRest, rotation), 0f, _maxAngle);

            return Quaternion.RotateTowards(rotationAtRest, rotation, angle);
        }

        private Quaternion LimitRotationByAxes(Quaternion rotation, out Vector3 angles)
        {
            angles = rotation.eulerAngles;

            if (_xAxisRange.y - _xAxisRange.x < 360f)
            {
                angles.x = ClampAngle(angles.x, _xAxisRange.x, _xAxisRange.y);
            }

            if (_yAxisRange.y - _yAxisRange.x < 360f)
            {
                angles.y = ClampAngle(angles.y, _yAxisRange.x, _yAxisRange.y);
            }

            if (_zAxisRange.y - _zAxisRange.x < 360f)
            {
                angles.z = ClampAngle(angles.z, _zAxisRange.x, _zAxisRange.y);
            }

            return Quaternion.Euler(angles);
        }

        private float ClampAngle(float angle, float min, float max)
        {
            float offset = 180f - 0.5f * (min + max);
            min += offset;
            max += offset;
            angle = Mathf.Repeat(angle + offset, 360f);

            return Mathf.Clamp(angle, min, max) - offset;
        }
    }
}