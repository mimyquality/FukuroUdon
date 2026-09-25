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

        private bool _isReachMaxAngle;
        private bool _isReachMinX, _isReachMaxX;
        private bool _isReachMinY, _isReachMaxY;
        private bool _isReachMaxZ, _isReachMinZ;

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
            // 追従処理
            Quaternion rotation = _sourceTransform ? FollowRotation() : _targetTransform.localRotation;

            // 範囲制限処理
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

            return Quaternion.Slerp(_rotationAtRest, sourceRotation, _weight);
        }

        private Quaternion LimitRotationByAngle(Quaternion rotation, ref float angle)
        {
            if (_maxAngle >= 180f) return rotation;

            angle = Mathf.Clamp(Quaternion.Angle(_rotationAtRest, rotation), 0f, _maxAngle);

            return Quaternion.RotateTowards(_rotationAtRest, rotation, _maxAngle);
        }

        private Quaternion LimitRotationByAxes(Quaternion rotation, out Vector3 angles)
        {
            if (_relativeTo == Space.World && _parent)
            {
                rotation = _parent.rotation * rotation;
            }

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

        private void SetIsReachMaxAngle(bool value)
        {
            if (_isReachMaxAngle != value)
            {
                SendLimitEndEvent(value ? "OnReachedMaxAngle" : "OnDepartedMaxAngle");

                _isReachMaxAngle = value;
            }
        }

        private void SetIsReachMinX(bool value)
        {
            if (_isReachMinX != value)
            {
                SendLimitEndEvent(value ? "OnReachedMinX" : "OnDepartedMinX");

                _isReachMinX = value;
            }
        }

        private void SetIsReachMaxX(bool value)
        {
            if (_isReachMaxX != value)
            {
                SendLimitEndEvent(value ? "OnReachedMaxX" : "OnDepartedMaxX");

                _isReachMaxX = value;
            }
        }

        private void SetIsReachMinY(bool value)
        {
            if (_isReachMinY != value)
            {
                SendLimitEndEvent(value ? "OnReachedMinY" : "OnDepartedMinY");

                _isReachMinY = value;
            }
        }

        private void SetIsReachMaxY(bool value)
        {
            if (_isReachMaxY != value)
            {
                SendLimitEndEvent(value ? "OnReachedMaxY" : "OnDepartedMaxY");

                _isReachMaxY = value;
            }
        }

        private void SetIsReachMinZ(bool value)
        {
            if (_isReachMinZ != value)
            {
                SendLimitEndEvent(value ? "OnReachedMinZ" : "OnDepartedMinZ");

                _isReachMinZ = value;
            }
        }

        private void SetIsReachMaxZ(bool value)
        {
            if (_isReachMaxZ != value)
            {
                SendLimitEndEvent(value ? "OnReachedMaxZ" : "OnDepartedMaxZ");

                _isReachMaxZ = value;
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