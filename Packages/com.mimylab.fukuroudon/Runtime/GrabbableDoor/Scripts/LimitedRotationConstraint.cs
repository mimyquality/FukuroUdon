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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limited-rotation-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Rotation Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedRotationConstraint : LimitedConstraint
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
        private Vector3 limitBaseVector = Vector3.forward;
        
        [SerializeField]
        [Range(0.0f, 180.0f)]
        private float maxAngle = 180.0f;

        private Transform _parent;
        private Quaternion _rotationAtRest;

        private bool _isReachMaxAngle;

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
            _rotationAtRest = targetTransform.localRotation;

            _eventReceivers = transform.GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            Quaternion rotation = sourceTransform ? FollowRotation() : targetTransform.localRotation;

            // 範囲制限処理
            float angle = 0;
            
            // ToDo:角度計算

            angle = Mathf.Min(angle, maxAngle);

            // 結果を Transform へ反映
            targetTransform.localRotation = rotation;

            // 制限イベント
            SetIsReachMaxAngle(angle >= maxAngle);
        }

        private Quaternion FollowRotation()
        {
            Quaternion sourceRotation = solveInLocalSpace
                ? sourceTransform.localRotation
                : _parent
                    ? Quaternion.Inverse(_parent.rotation) * sourceTransform.rotation
                    : sourceTransform.rotation;

            return Quaternion.Slerp(_rotationAtRest, sourceRotation, weight);
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