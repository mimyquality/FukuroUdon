/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Rendering;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#flexible-transform")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Flexible Transform")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlexibleTransform : UdonSharpBehaviour
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

        private VRCCameraSettings _camera;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_target) { _target = this.transform; }
            _camera = VRCCameraSettings.ScreenCamera;

            if (_target == this.transform) { _inactiveOutOfRange = false; }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(_camera)) { return; }
            var position = _camera.Position;

            var nearest = Vector3.positiveInfinity;
            foreach (var collider in _area)
            {
                if (!collider) { continue; }

                var point = collider.ClosestPoint(position);
                nearest = (point - position).sqrMagnitude < (nearest - position).sqrMagnitude ? point : nearest;

                if (point == position) { break; }
            }
            // positiveInfinityならコライダーが無かったと見なす。
            if (nearest.Equals(Vector3.positiveInfinity)) { Debug.LogWarning($"Flexible Spatial Audio in {this.gameObject.name} haven't Area Collider."); return; }

            if (_positionOnly)
            {
                _target.position = nearest;
            }
            else
            {
                var rotation = _camera.Rotation;
                _target.SetPositionAndRotation(nearest, rotation);
            }

            if (_inactiveOutOfRange)
            {
                var isInRange = (position - nearest).sqrMagnitude <= (_activeRange * _activeRange);
                if (_target.gameObject.activeSelf != isInRange) { _target.gameObject.SetActive(isInRange); }
            }
        }
    }
}
