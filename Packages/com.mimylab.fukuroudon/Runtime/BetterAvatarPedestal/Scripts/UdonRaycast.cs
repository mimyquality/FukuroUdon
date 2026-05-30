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
    using VRC.SDK3.Components;
    using VRC.Udon;

    public enum UdonRaycastBehaviorOnMiss
    {
        DoNothing,
        SnapToStart,
        SnapToEnd,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Better-AvatarPedestal#udon-raycast")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Better AvatarPedestal/Udon Raycast")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UdonRaycast : UdonSharpBehaviour
    {
        [Header("Raycast Properties")]
        [SerializeField]
        private Vector3 _raycastDirection = Vector3.forward;
        [SerializeField, Range(0.0f, 10000.0f)]
        private float _distance = 10.0f;
        [SerializeField]
        private bool _applyTransformScale = false;
        [SerializeField]
        private LayerMask _collisionLayers = ~0;

        [Header("Results")]
        [SerializeField]
        private Transform _resultTransform = null;
        [SerializeField]
        private bool _applyRotation = false;
        [SerializeField]
        private UdonRaycastBehaviorOnMiss _behaviorOnMiss = UdonRaycastBehaviorOnMiss.DoNothing;

        private Vector3 _localDirection;
        private RaycastHit _hitInfo;

        private void Start()
        {
            _localDirection = _raycastDirection.normalized;
        }

        private void FixedUpdate()
        {
            if (!_resultTransform) { return; }

            Vector3 startPoint = transform.position;
            Vector3 direction = transform.rotation * _localDirection;
            float distance = _applyTransformScale ? transform.lossyScale.z * _distance : _distance;

            if (Physics.Raycast(startPoint, direction, out _hitInfo, _distance, _collisionLayers))
            {
                _resultTransform.position = _hitInfo.point;
                if (_applyRotation)
                {
                    // ToDo: 軸変換が未実装
                    _resultTransform.rotation = Quaternion.LookRotation(_hitInfo.normal);
                }
            }
            else
            {
                switch (_behaviorOnMiss)
                {
                    case UdonRaycastBehaviorOnMiss.SnapToStart:
                        _resultTransform.position = startPoint;
                        if (_applyRotation)
                        {
                            // ToDo: 軸変換が未実装
                            _resultTransform.rotation = transform.rotation;
                        }
                        break;
                    case UdonRaycastBehaviorOnMiss.SnapToEnd:
                        Vector3 endPoint = distance * direction + startPoint;
                        _resultTransform.position = endPoint;
                        if (_applyRotation)
                        {
                            // ToDo: 軸変換が未実装
                            _resultTransform.rotation = transform.rotation;
                        }
                        break;
                    case UdonRaycastBehaviorOnMiss.DoNothing:
                    default:
                        // Do nothing
                        break;
                }
            }
        }

        // ToDo: Animator Parameter 渡し用の変数が必要
    }
}
