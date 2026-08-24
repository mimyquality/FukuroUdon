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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limit-position-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limit Constraint/Limit Position Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitPositionConstraint : LimitConstraint
    {
        [SerializeField] private Transform target;
        [SerializeField] private Space relativeTo = Space.Self;
        [SerializeField] private Vector3 minPosition = Vector3.negativeInfinity;
        [SerializeField] private Vector3 maxPosition = Vector3.positiveInfinity;

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

            if (!target)
            {
                target = transform;
            }

            _eventReceivers = target.GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            Vector3 currentPosition = relativeTo == Space.Self ? target.localPosition : target.position;

            currentPosition.x = Mathf.Clamp(currentPosition.x, minPosition.x, maxPosition.x);
            currentPosition.y = Mathf.Clamp(currentPosition.y, minPosition.y, maxPosition.y);
            currentPosition.z = Mathf.Clamp(currentPosition.z, minPosition.z, maxPosition.z);

            if (relativeTo == Space.Self)
            {
                target.localPosition = currentPosition;
            }
            else
            {
                target.position = currentPosition;
            }

            SetIsReachMinX(currentPosition.x <= minPosition.x);
            SetIsReachMaxX(currentPosition.x >= maxPosition.x);
            SetIsReachMinY(currentPosition.y <= minPosition.y);
            SetIsReachMaxY(currentPosition.y >= maxPosition.y);
            SetIsReachMinZ(currentPosition.z <= minPosition.z);
            SetIsReachMaxZ(currentPosition.z >= maxPosition.z);
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
                if (!_eventReceivers[i]) continue;
                if (_eventReceivers[i] == this) continue;

                _eventReceivers[i].SendCustomEvent(eventName);
            }
        }
    }
}