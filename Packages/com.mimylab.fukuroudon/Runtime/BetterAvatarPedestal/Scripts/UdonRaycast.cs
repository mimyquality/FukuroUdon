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
        private Vector3 _alignmentAxis = Vector3.up;
        [SerializeField]
        private UdonRaycastBehaviorOnMiss _behaviorOnMiss = UdonRaycastBehaviorOnMiss.DoNothing;

        private RaycastHit _hitInfo;
        private bool _hit = false;
        private float _distanceToHit = 0.0f;
        private float _ratio = 0.0f;

        public bool Hit { get => _hit; }
        public float Distance { get => _distanceToHit; }
        public float Ratio { get => _ratio; }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private Color _gizmoColor = new Color(1.0f, 0.5f, 0.0f);

        private void OnDrawGizmosSelected()
        {
            Vector3 startPoint = transform.position;
            Vector3 direction = transform.rotation * _raycastDirection.normalized;
            float maxDistance = _applyTransformScale ? transform.lossyScale.z * _distance : _distance;
            Vector3 endPoint = maxDistance * direction + startPoint;

            Gizmos.color = _gizmoColor;
            Gizmos.DrawLine(startPoint, endPoint);
        }
#endif

        private void FixedUpdate()
        {
            if (!_resultTransform) { return; }

            Vector3 startPoint = transform.position;
            Vector3 direction = transform.rotation * _raycastDirection.normalized;
            float maxDistance = _applyTransformScale ? transform.lossyScale.z * _distance : _distance;

            if (_hit = Physics.Raycast(startPoint, direction, out _hitInfo, maxDistance, _collisionLayers, QueryTriggerInteraction.Ignore))
            {
                _resultTransform.position = _hitInfo.point;
                if (_applyRotation)
                {
                    _resultTransform.rotation = Quaternion.LookRotation(_hitInfo.normal) * Quaternion.FromToRotation(_alignmentAxis, Vector3.forward);
                }
                _distanceToHit = _hitInfo.distance;
                _ratio = Mathf.Clamp01(maxDistance > 0.0f ? _distanceToHit / maxDistance : 0.0f);
            }
            else
            {
                switch (_behaviorOnMiss)
                {
                    case UdonRaycastBehaviorOnMiss.SnapToStart:
                        _resultTransform.position = startPoint;
                        if (_applyRotation)
                        {
                            _resultTransform.rotation = Quaternion.LookRotation(-direction) * Quaternion.FromToRotation(_alignmentAxis, Vector3.forward);
                        }
                        _distanceToHit = 0.0f;
                        _ratio = 0.0f;
                        break;
                    case UdonRaycastBehaviorOnMiss.SnapToEnd:
                        _resultTransform.position = maxDistance * direction + startPoint;
                        if (_applyRotation)
                        {
                            _resultTransform.rotation = Quaternion.LookRotation(-direction) * Quaternion.FromToRotation(_alignmentAxis, Vector3.forward);
                        }
                        _distanceToHit = maxDistance;
                        _ratio = 1.0f;
                        break;
                    case UdonRaycastBehaviorOnMiss.DoNothing:
                    default:
                        // Do nothing
                        break;
                }
            }
        }
    }
}
