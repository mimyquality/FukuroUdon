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
        Angle,
        EulerAngles
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-rotation-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Rotation Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedRotationConstraint : LimitedConstraint
    {
        [SerializeField]
        private Transform targetTransform;

        [Header("Follow Settings")]
        [SerializeField]
        private Transform sourceTransform;

        [SerializeField, Range(0.0f, 1.0f)]
        private float weight = 1.0f;

        [SerializeField]
        private bool solveInLocalSpace = false;

        [Header("Limit Settings")]
        [SerializeField]
        private RotationLimitType limitType = RotationLimitType.Angle;

        [SerializeField, Range(0.0f, 180.0f)]
        private float maxAngle = 180.0f;

        [Tooltip("下限から上限までの角度が360°以上なら無制限扱いになります。")]
        [SerializeField, MinMaxRange(-360f, 360f)]
        private Vector2 xAxisRange = new Vector2(-180f, 180f);

        [Tooltip("下限から上限までの角度が360°以上なら無制限扱いになります。")]
        [SerializeField, MinMaxRange(-360f, 360f)]
        private Vector2 yAxisRange = new Vector2(-180f, 180f);

        [Tooltip("下限から上限までの角度が360°以上なら無制限扱いになります。")]
        [SerializeField, MinMaxRange(-360f, 360f)]
        private Vector2 zAxisRange = new Vector2(-180f, 180f);

        [SerializeField]
        private Space relativeTo = Space.Self;

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
            // 追従処理
            Quaternion rotation = sourceTransform ? FollowRotation() : targetTransform.localRotation;

            // 範囲制限処理
            if (relativeTo == Space.World)
            {
                if (_parent)
                {
                    rotation = _parent.rotation * rotation;
                }
            }

            var reachFlags = new bool[6];
            switch (limitType)
            {
                case RotationLimitType.Angle:
                    rotation = LimitRotationByAngle(rotation, out reachFlags[0]);
                    break;
                case RotationLimitType.EulerAngles:
                    rotation = LimitRotationByAxes(rotation, out reachFlags);
                    break;
            }

            // 結果を Transform へ反映
            if (relativeTo == Space.World)
            {
                targetTransform.rotation = rotation;
            }
            else
            {
                targetTransform.localRotation = rotation;
            }

            // 制限イベント
            switch (limitType)
            {
                case RotationLimitType.Angle:
                    SetIsReachMaxAngle(reachFlags[0]);
                    break;
                case RotationLimitType.EulerAngles:
                    SetIsReachMinX(reachFlags[0]);
                    SetIsReachMaxX(reachFlags[1]);
                    SetIsReachMinY(reachFlags[2]);
                    SetIsReachMaxY(reachFlags[3]);
                    SetIsReachMinZ(reachFlags[4]);
                    SetIsReachMaxZ(reachFlags[5]);
                    break;
            }
        }

        private Quaternion FollowRotation()
        {
            Quaternion sourceRotation = solveInLocalSpace
                ? sourceTransform.localRotation
                : _parent
                    ? Quaternion.Inverse(_parent.rotation) * sourceTransform.rotation
                    : sourceTransform.rotation;

            return Quaternion.Slerp(_rotationAtRest, sourceRotation, weight);
        }

        private Quaternion LimitRotationByAngle(Quaternion rotation, out bool reachFlag)
        {
            Quaternion rotate = Quaternion.Inverse(_rotationAtRest) * rotation;
            rotate.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f)
            {
                angle = 360f - angle;
                axis = -1f * axis;
            }

            reachFlag = angle >= maxAngle;
            angle = Mathf.Clamp(angle, 0f, maxAngle);

            return _rotationAtRest * Quaternion.AngleAxis(angle, axis);
        }

        private Quaternion LimitRotationByAxes(Quaternion rotation, out bool[] reachFlags)
        {
            Vector3 eulerAngles = rotation.eulerAngles;
            float offset, min, max, angle;
            reachFlags = new bool[6];

            if (xAxisRange.y - xAxisRange.x < 360f)
            {
                offset = 180f - 0.5f * (xAxisRange.x + xAxisRange.y);
                min = xAxisRange.x + offset;
                max = xAxisRange.y + offset;
                angle = Mathf.Repeat(eulerAngles.x + offset, 360f);

                reachFlags[0] = angle <= min;
                reachFlags[1] = angle >= max;
                eulerAngles.x = Mathf.Clamp(angle, min, max) - offset;
            }

            if (yAxisRange.y - yAxisRange.x < 360f)
            {
                offset = 180f - 0.5f * (yAxisRange.x + yAxisRange.y);
                min = yAxisRange.x + offset;
                max = yAxisRange.y + offset;
                angle = Mathf.Repeat(eulerAngles.y + offset, 360f);

                reachFlags[2] = angle <= min;
                reachFlags[3] = angle >= max;
                eulerAngles.y = Mathf.Clamp(angle, min, max) - offset;
            }

            if (zAxisRange.y - zAxisRange.x < 360f)
            {
                offset = 180f - 0.5f * (zAxisRange.x + zAxisRange.y);
                min = zAxisRange.x + offset;
                max = zAxisRange.y + offset;
                angle = Mathf.Repeat(eulerAngles.z + offset, 360f);

                reachFlags[4] = angle <= min;
                reachFlags[5] = angle >= max;
                eulerAngles.z = Mathf.Clamp(angle, min, max) - offset;
            }

            return Quaternion.Euler(eulerAngles);
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