/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Flexible Transform")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlexibleTransform : IViewPointReceiver
    {
        [SerializeField]
        private Transform _target;

        [SerializeField]
        private bool _positionOnly = false;
        [SerializeField]
        private bool _inactiveOutOfRange = false;
        [SerializeField, Tooltip("meter")]
        private float _activeRange = 10.0f;

        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];

        private Vector3 _viewPointPosition;
        private Quaternion _viewPointRotation;

        public override void ReceiveViewPoint(Vector3 position, Quaternion rotation)
        {
            if (position == _viewPointPosition
             && rotation == _viewPointRotation) { return; }

            SnapViewPoint(position, rotation);
            _viewPointPosition = position;
            _viewPointRotation = rotation;
        }

        private void SnapViewPoint(Vector3 vpPosition, Quaternion vpRotation)
        {
            if (!_target) { return; }

            var nearest = Vector3.positiveInfinity;
            foreach (var col in _area)
            {
                if (!col) { continue; }

                var point = col.ClosestPoint(vpPosition);
                nearest = (point - vpPosition).sqrMagnitude < (nearest - vpPosition).sqrMagnitude ? point : nearest;

                if (point == vpPosition) { break; }
            }
            // positiveInfinityならコライダーが無かったと見なす。
            if (nearest.Equals(Vector3.positiveInfinity)) { Debug.LogWarning($"Flexible Spatial Audio in {this.gameObject.name} haven't Area Collider."); return; }

            if (_positionOnly)
            {
                _target.position = nearest;
            }
            else
            {
                _target.SetPositionAndRotation(nearest, vpRotation);
            }

            if (_inactiveOutOfRange)
            {
                var isInRange = (vpPosition - nearest).sqrMagnitude <= (_activeRange * _activeRange);
                if (_target.gameObject.activeSelf != isInRange) { _target.gameObject.SetActive(isInRange); }
            }
        }
    }
}
