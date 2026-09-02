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
        private Transform targetTransform;

        [Header("Follow Settings")]
        [SerializeField]
        private Transform sourceTransform;

        [SerializeField, Range(0.0f, 1.0f)]
        private float weight = 1.0f;

        [Header("Limit Settings")]
        [SerializeField, Min(0.0f)]
        private Vector3 minScale = Vector3.zero;

        [SerializeField, Min(0.0f)]
        private Vector3 maxScale = Vector3.positiveInfinity;

        private Vector3 _scaleAtRest;

        private bool _isReachMinX, _isReachMaxX;
        private bool _isReachMinY, _isReachMaxY;
        private bool _isReachMinZ, _isReachMaxZ;

        private UdonBehaviour[] _eventReceivers;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            if (!targetTransform)
            {
                targetTransform = transform;
            }

            _scaleAtRest = targetTransform.localScale;

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
            Vector3 scale = sourceTransform
                ? Vector3.Lerp(_scaleAtRest, sourceTransform.localScale, weight)
                : targetTransform.localScale;

            // 範囲制限処理
            scale.x = Mathf.Clamp(scale.x, minScale.x, maxScale.x);
            scale.y = Mathf.Clamp(scale.y, minScale.y, maxScale.y);
            scale.z = Mathf.Clamp(scale.z, minScale.z, maxScale.z);

            // 結果を Transform へ反映
            targetTransform.localScale = scale;

            // 制限イベント
            SetIsReachMinX(scale.x <= minScale.x);
            SetIsReachMaxX(scale.x >= maxScale.x);
            SetIsReachMinY(scale.y <= minScale.y);
            SetIsReachMaxY(scale.y >= maxScale.y);
            SetIsReachMinZ(scale.z <= minScale.z);
            SetIsReachMaxZ(scale.z >= maxScale.z);
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