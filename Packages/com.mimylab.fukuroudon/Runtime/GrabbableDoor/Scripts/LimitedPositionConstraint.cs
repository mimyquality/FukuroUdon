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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-position-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Position Constraint")]
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedPositionConstraint : LimitedConstraint
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
        private Vector3 _minPosition = Vector3.negativeInfinity;

        [SerializeField]
        private Vector3 _maxPosition = Vector3.positiveInfinity;

        [SerializeField]
        private Space _relativeTo = Space.Self;

        private Transform _parent;
        private Vector3 _positionAtRest;
        private Vector3 _positionOffset;
        private Vector3 _differential = Vector3.zero;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            if (!_targetTransform)
            {
                _targetTransform = transform;
            }

            _parent = _targetTransform.parent;
            _positionAtRest = _targetTransform.localPosition;
            if (_sourceTransform)
            {
                Vector3 sourcePosition = _solveInLocalSpace
                    ? _sourceTransform.localPosition
                    : _parent
                        ? _parent.InverseTransformPoint(_sourceTransform.position)
                        : _sourceTransform.position;
                _positionOffset = _positionAtRest - sourcePosition;
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
            Vector3 targetPosition = _targetTransform.localPosition;

            // 追従処理
            Vector3 position = _sourceTransform ? ConstrainPosition() : targetPosition;

            // 遅延処理
            position = FollowPosition(targetPosition, position);

            // 範囲制限処理
            if (_relativeTo == Space.World && _parent)
            {
                position = _parent.TransformPoint(position);
            }

            position.x = Mathf.Clamp(position.x, _minPosition.x, _maxPosition.x);
            position.y = Mathf.Clamp(position.y, _minPosition.y, _maxPosition.y);
            position.z = Mathf.Clamp(position.z, _minPosition.z, _maxPosition.z);

            // 結果を Transform へ反映
            if (_relativeTo == Space.World)
            {
                _targetTransform.position = position;
            }
            else
            {
                _targetTransform.localPosition = position;
            }

            // 制限イベント
            SetIsReachMinX(position.x <= _minPosition.x);
            SetIsReachMaxX(position.x >= _maxPosition.x);
            SetIsReachMinY(position.y <= _minPosition.y);
            SetIsReachMaxY(position.y >= _maxPosition.y);
            SetIsReachMinZ(position.z <= _minPosition.z);
            SetIsReachMaxZ(position.z >= _maxPosition.z);
        }

        private Vector3 ConstrainPosition()
        {
            Vector3 sourcePosition = _solveInLocalSpace
                ? _sourceTransform.localPosition
                : _parent
                    ? _parent.InverseTransformPoint(_sourceTransform.position)
                    : _sourceTransform.position;
            sourcePosition += _positionOffset;

            return Vector3.Lerp(_positionAtRest, sourcePosition, _weight);
        }

        private Vector3 FollowPosition(Vector3 targetPosition, Vector3 goalPosition)
        {
            if (targetPosition == goalPosition)
            {
                return targetPosition;
            }

            if (_smoothingFactor > 0.0f)
            {
                return Vector3.SmoothDamp(targetPosition, goalPosition, ref _differential, _smoothingFactor);
            }

            return goalPosition;
        }
    }
}