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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-twist-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Twist Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedTwistConstraint : LimitedConstraint
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
        private Vector3 _twistAxis = Vector3.up;

        [SerializeField]
        private SourceAimType _sourceAimType = SourceAimType.ObjectAim;

        [SerializeField]
        private Vector3 _sourceAimVector = Vector3.forward;

        [Header("Limit Settings")]
        [SerializeField, MinMaxRange(-180f, 180f)]
        private Vector2 _angleRange = new(-180f, 180f);

        private Vector3 _upVector = Vector3.forward;

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
                        _upVector = _sourceTransform.position - _targetTransform.position;
                        break;
                    case SourceAimType.ObjectRotationAim:
                        _upVector = _sourceTransform.rotation * _sourceAimVector;
                        break;
                }

                _upVector = _targetTransform.InverseTransformDirection(_upVector.normalized);
            }

            _parent = _targetTransform.parent;
            _rotationAtRest = _targetTransform.localRotation;
            _axisOffset = Quaternion.LookRotation(_twistAxis, _upVector);

            _eventReceivers = transform.GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            Quaternion targetRotation = _targetTransform.rotation * _axisOffset;
            Quaternion parentRotation = _parent ? _parent.rotation : Quaternion.identity;
            Quaternion baseRotation = parentRotation * _rotationAtRest * _axisOffset;

            // 追従処理
            Quaternion rotation = _sourceTransform ? ConstrainRotation(baseRotation) : targetRotation;

            // 遅延処理
            rotation = FollowRotation(targetRotation, rotation);

            // 範囲制限処理
            rotation = LimitRoll(rotation, out float angle, baseRotation);

            // 結果を Transform へ反映
            _targetTransform.rotation = rotation * Quaternion.Inverse(_axisOffset);

            // 制限イベント
            SetIsReachMinAngle(angle <= _angleRange.x);
            SetIsReachMaxAngle(angle >= _angleRange.y);
        }

        private Quaternion ConstrainRotation(Quaternion baseRotation)
        {
            // ワールド空間で計算
            Vector3 forward = baseRotation * Vector3.forward;
            Vector3 up = _targetTransform.TransformDirection(_upVector);
            switch (_sourceAimType)
            {
                case SourceAimType.ObjectAim:
                    up = (_sourceTransform.position - _targetTransform.position);
                    break;
                case SourceAimType.ObjectRotationAim:
                    up = _sourceTransform.rotation * _sourceAimVector;
                    break;
            }

            Quaternion sourceRotation = Quaternion.LookRotation(forward, up);

            return Quaternion.Slerp(baseRotation, sourceRotation, _weight);
        }

        private Quaternion FollowRotation(Quaternion targetRotation, Quaternion goalRotation)
        {
            if (targetRotation == goalRotation)
            {
                return targetRotation;
            }

            if (_smoothingFactor > 0.0f)
            {
                float angle = Quaternion.Angle(targetRotation, goalRotation);
                angle = Mathf.SmoothDampAngle(0.0f, angle, ref _differential, _smoothingFactor);

                return Quaternion.RotateTowards(targetRotation, goalRotation, angle);
            }

            return goalRotation;
        }

        private Quaternion LimitRoll(Quaternion rotation, out float angle, Quaternion baseRotation)
        {
            angle = 0f;

            if (_angleRange.y - _angleRange.x >= 360f) return rotation;

            angle =
                Vector3.SignedAngle(baseRotation * Vector3.up, rotation * Vector3.up, baseRotation * Vector3.forward);

            if (_angleRange.x <= angle && angle <= _angleRange.y) return rotation;

            angle = Mathf.Clamp(angle, _angleRange.x, _angleRange.y);

            return baseRotation * Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}