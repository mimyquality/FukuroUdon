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
        private Transform targetTransform;

        [Header("Follow Settings")]
        [SerializeField]
        private Transform sourceTransform;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float weight = 1.0f;

        [SerializeField]
        private bool solveInLocalSpace = false;

        [Header("Limit Settings")]
        [SerializeField]
        private Vector3 minPosition = Vector3.negativeInfinity;

        [SerializeField]
        private Vector3 maxPosition = Vector3.positiveInfinity;

        [SerializeField]
        private Space relativeTo = Space.Self;

        private Transform _parent;
        private Vector3 _positionAtRest;
        
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

            if (!targetTransform)
            {
                targetTransform = transform;
            }

            _parent = targetTransform.parent;
            _positionAtRest = targetTransform.localPosition;

            _eventReceivers = transform.GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            Vector3 position = sourceTransform ? FollowPosition() : targetTransform.localPosition;

            // 範囲制限処理
            if (relativeTo == Space.World)
            {
                if (_parent)
                {
                    position = _parent.TransformPoint(position);
                }
            }

            position.x = Mathf.Clamp(position.x, minPosition.x, maxPosition.x);
            position.y = Mathf.Clamp(position.y, minPosition.y, maxPosition.y);
            position.z = Mathf.Clamp(position.z, minPosition.z, maxPosition.z);

            // 結果を Transform へ反映
            if (relativeTo == Space.World)
            {
                targetTransform.position = position;
            }
            else
            {
                targetTransform.localPosition = position;
            }

            // 制限イベント
            SetIsReachMinX(position.x <= minPosition.x);
            SetIsReachMaxX(position.x >= maxPosition.x);
            SetIsReachMinY(position.y <= minPosition.y);
            SetIsReachMaxY(position.y >= maxPosition.y);
            SetIsReachMinZ(position.z <= minPosition.z);
            SetIsReachMaxZ(position.z >= maxPosition.z);
        }

        private Vector3 FollowPosition()
        {
            Vector3 sourcePosition = solveInLocalSpace 
                ? sourceTransform.localPosition 
                : _parent
                    ? _parent.InverseTransformPoint(sourceTransform.position)
                    : sourceTransform.position;

            return Vector3.Lerp(_positionAtRest, sourcePosition, weight);
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