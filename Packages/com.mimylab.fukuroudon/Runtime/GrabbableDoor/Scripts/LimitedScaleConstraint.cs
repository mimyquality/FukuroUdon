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
    [AddComponentMenu("Fukuro Udon/Limit Constraint/Limited Scale Constraint")]
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedScaleConstraint : LimitedConstraint
    {
        [SerializeField]
        private Transform sourceTransform;

        [SerializeField]
        private Vector3 minScale = Vector3.zero;

        [SerializeField]
        private Vector3 maxScale = Vector3.positiveInfinity;

        [Header("Advanced Settings")]
        [SerializeField]
        private Transform targetTransform;

        private bool _isReachMinX;
        private bool _isReachMaxX;
        private bool _isReachMinY;
        private bool _isReachMaxY;
        private bool _isReachMinZ;
        private bool _isReachMaxZ;

        private UdonBehaviour[] _eventReceivers;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            if (!sourceTransform)
            {
                sourceTransform = transform;
            }

            if (!targetTransform)
            {
                targetTransform = transform;
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
            Vector3 scale = sourceTransform.localScale;

            scale.x = Mathf.Clamp(scale.x, minScale.x, maxScale.x);
            scale.y = Mathf.Clamp(scale.y, minScale.y, maxScale.y);
            scale.z = Mathf.Clamp(scale.z, minScale.z, maxScale.z);

            targetTransform.localScale = scale;

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