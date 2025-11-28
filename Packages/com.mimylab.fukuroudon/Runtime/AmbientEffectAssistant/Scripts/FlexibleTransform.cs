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
    //using VRC.SDK3.Rendering;

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
        [SerializeField, Min(0.0f), Tooltip("meter")]
        private float _activeRange = 10.0f;

        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];

        private Bounds _areaBounds;
        //private VRCCameraSettings _camera;
        private VRCPlayerApi _localPlayer;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_target) { _target = this.transform; }
            //_camera = VRCCameraSettings.ScreenCamera;
            _localPlayer = Networking.LocalPlayer;

            if (_target == this.transform) { _inactiveOutOfRange = false; }

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();
            RecalculateAreaBounds();
        }

        public override void PostLateUpdate()
        {
            //if (!Utilities.IsValid(_camera)) { return; }
            //var position = _camera.Position;
            var position = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

            if (_inactiveOutOfRange && !_areaBounds.Contains(position))
            {
                // 完全に _activeRange の範囲外にいる
                if (_target.gameObject.activeSelf) { _target.gameObject.SetActive(false); }
                return;
            }

            Vector3 nearest;
            CheckInArea(position, out nearest);
            // positiveInfinity ならコライダーが無かったと見なす。
            if (nearest.Equals(Vector3.positiveInfinity)) { Debug.LogWarning($"Flexible Spatial Audio in {this.gameObject.name} haven't Area Collider."); return; }

            if (_inactiveOutOfRange)
            {
                var isInRange = (position - nearest).sqrMagnitude <= (_activeRange * _activeRange);
                if (_target.gameObject.activeSelf != isInRange) { _target.gameObject.SetActive(isInRange); }
                if (!isInRange) { return; }
            }

            if (_positionOnly)
            {
                _target.position = nearest;
            }
            else
            {
                //var rotation = _camera.Rotation;
                var rotation = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                _target.SetPositionAndRotation(nearest, rotation);
            }
        }

        public void RecalculateAreaBounds()
        {
            var compoundMin = Vector3.positiveInfinity;
            var compoundMax = Vector3.negativeInfinity;
            foreach (var collider in _area)
            {
                if (!collider) { continue; }

                var bounds = collider.bounds;
                if (bounds.extents.Equals(Vector3.zero)) { continue; }

                compoundMin = Vector3.Min(compoundMin, bounds.min);
                compoundMax = Vector3.Max(compoundMax, bounds.max);
            }

            if (compoundMin.Equals(Vector3.positiveInfinity))
            {
                _areaBounds = new Bounds();
                return;
            }

            // _activeRange の範囲も Bounds に含める
            // マージンを足して、接近に対して早めに有効にする
            var effectiveRange = (_activeRange + 1.0f) * Vector3.one;
            compoundMin -= effectiveRange;
            compoundMax += effectiveRange;

            var center = (compoundMin + compoundMax) * 0.5f;
            var size = compoundMax - compoundMin;
            _areaBounds = new Bounds(center, size);
        }

        private bool CheckInArea(Vector3 position, out Vector3 nearest)
        {
            nearest = Vector3.positiveInfinity;
            foreach (var collider in _area)
            {
                if (!collider) { continue; }
                if (!collider.enabled) { continue; }
                if (!collider.gameObject.activeInHierarchy) { continue; }

                var point = collider.ClosestPoint(position);
                nearest = (point - position).sqrMagnitude < (nearest - position).sqrMagnitude ? point : nearest;

                if (point == position)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
