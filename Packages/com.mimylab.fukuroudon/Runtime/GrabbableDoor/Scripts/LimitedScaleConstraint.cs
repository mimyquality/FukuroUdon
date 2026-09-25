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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-scale-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Scale Constraint")]
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedScaleConstraint : LimitedConstraint
    {
        [SerializeField]
        private Transform _targetTransform;

        [Header("Constraint Settings")]
        [SerializeField]
        private Transform _sourceTransform;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _weight = 1.0f;

        [Header("Limit Settings")]
        [SerializeField, Min(0.0f)]
        private Vector3 _minScale = Vector3.zero;

        [SerializeField, Min(0.0f)]
        private Vector3 _maxScale = Vector3.positiveInfinity;

        private Vector3 _scaleAtRest;

        private bool _isReachMinX, _isReachMaxX;
        private bool _isReachMinY, _isReachMaxY;
        private bool _isReachMinZ, _isReachMaxZ;

        private UdonBehaviour[] _eventReceivers;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            if (!_targetTransform)
            {
                _targetTransform = transform;
            }

            _scaleAtRest = _targetTransform.localScale;

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
            Vector3 scale = _sourceTransform
                ? Vector3.Lerp(_scaleAtRest, _sourceTransform.localScale, _weight)
                : _targetTransform.localScale;

            // 範囲制限処理
            scale.x = Mathf.Clamp(scale.x, _minScale.x, _maxScale.x);
            scale.y = Mathf.Clamp(scale.y, _minScale.y, _maxScale.y);
            scale.z = Mathf.Clamp(scale.z, _minScale.z, _maxScale.z);

            // 結果を Transform へ反映
            _targetTransform.localScale = scale;

            // 制限イベント
            SetIsReachMinX(scale.x <= _minScale.x);
            SetIsReachMaxX(scale.x >= _maxScale.x);
            SetIsReachMinY(scale.y <= _minScale.y);
            SetIsReachMaxY(scale.y >= _maxScale.y);
            SetIsReachMinZ(scale.z <= _minScale.z);
            SetIsReachMaxZ(scale.z >= _maxScale.z);
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