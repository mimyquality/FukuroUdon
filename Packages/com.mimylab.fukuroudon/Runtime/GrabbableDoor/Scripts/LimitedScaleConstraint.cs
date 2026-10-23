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

        [SerializeField, Range(0.0f, 1.0f)]
        private float _smoothingFactor = 0.0f;

        [Header("Limit Settings")]
        [SerializeField, Min(0.0f)]
        private Vector3 _minScale = Vector3.zero;

        [SerializeField, Min(0.0f)]
        private Vector3 _maxScale = Vector3.positiveInfinity;

        private Vector3 _scaleAtRest;
        private Vector3 _differential = Vector3.zero;

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
            
            Vector3 targetScale = _targetTransform.localScale;
            
            // 追従処理
            Vector3 scale = _sourceTransform ? ConstrainScale() : targetScale;
            
            // 遅延処理
            scale = FollowScale(targetScale, scale);

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

        private Vector3 ConstrainScale()
        {
            return Vector3.Lerp(_scaleAtRest, _sourceTransform.localScale, _weight);
        }

        private Vector3 FollowScale(Vector3 targetScale, Vector3 goalScale)
        {
            if (targetScale == goalScale)
            {
                return targetScale;
            }
            
            if (_smoothingFactor > 0.0f)
            {
                return Vector3.SmoothDamp(targetScale, goalScale, ref _differential, _smoothingFactor);
            }

            return goalScale;
        }
    }
}