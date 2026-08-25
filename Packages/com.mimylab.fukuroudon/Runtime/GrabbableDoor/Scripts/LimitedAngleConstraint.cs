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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-angle-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limit Constraint/Limited Angle Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedAngleConstraint : LimitedConstraint
    {
        [SerializeField]
        private Transform sourceTransform;

        [SerializeField]
        private Space relativeTo = Space.Self;

        [SerializeField]
        [Range(0.0f, 180.0f)]
        private float maxAngle = 180.0f;

        [Header("Advanced Settings")]
        [SerializeField]
        private Transform targetTransform;

        private bool _isReachMaxAngle;

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
            Quaternion rotation = relativeTo == Space.Self ? sourceTransform.localRotation : sourceTransform.rotation;
            float angle = 0; // ToDo:角度計算

            angle = Mathf.Min(angle, maxAngle);

            if (relativeTo == Space.Self)
            {
                targetTransform.localRotation = rotation;
            }
            else
            {
                targetTransform.rotation = rotation;
            }

            SetIsReachMaxAngle(angle >= maxAngle);
        }

        private void SetIsReachMaxAngle(bool value)
        {
            if (_isReachMaxAngle != value)
            {
                SendLimitEndEvent(value ? "OnReachedMaxAngle" : "OnDepartedMaxAngle");

                _isReachMaxAngle = value;
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