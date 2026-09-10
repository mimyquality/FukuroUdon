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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-position-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Position Constraint")]
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedPositionConstraint : LimitedConstraint
    {
        [SerializeField]
        private Transform _targetTransform;

        [Header("Follow Settings")]
        [SerializeField]
        private Transform _sourceTransform;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _weight = 1.0f;

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
            _positionAtRest = _targetTransform.localPosition;

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
            Vector3 position = _sourceTransform ? FollowPosition() : _targetTransform.localPosition;

            // 範囲制限処理
            if (_relativeTo == Space.World)
            {
                if (_parent)
                {
                    position = _parent.TransformPoint(position);
                }
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

        private Vector3 FollowPosition()
        {
            Vector3 sourcePosition = _solveInLocalSpace 
                ? _sourceTransform.localPosition 
                : _parent
                    ? _parent.InverseTransformPoint(_sourceTransform.position)
                    : _sourceTransform.position;

            return Vector3.Lerp(_positionAtRest, sourcePosition, _weight);
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