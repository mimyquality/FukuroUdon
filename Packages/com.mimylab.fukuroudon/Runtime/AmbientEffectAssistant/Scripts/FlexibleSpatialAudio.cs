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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#flexible-spatial-audio")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Flexible Spatial Audio")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlexibleSpatialAudio : UdonSharpBehaviour
    {
        [SerializeField]
        private AudioSource _decaySound;
        [SerializeField]
        private AudioSource _innerSound;

        [SerializeField, Min(0.0f), Tooltip("meter")]
        private float _effectiveRangeOffset = 1.0f;

        [Header("Bounds Settings")]
        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];
        [SerializeField]
        private bool _areaIsStatic = true;

        private Transform _decayTransform;
        private Transform _innerTransform;
        private Bounds _areaBounds;
        //private VRCCameraSettings _camera;
        private VRCPlayerApi _localPlayer;
        private float _effectiveRange;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (_decaySound) { _decayTransform = _decaySound.transform; }
            if (_innerSound) { _innerTransform = _innerSound.transform; }
            //_camera = VRCCameraSettings.ScreenCamera;
            _localPlayer = Networking.LocalPlayer;

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

            if (_areaIsStatic && !_areaBounds.Contains(position))
            {
                // 絶対に音の届かない距離にいる
                if (_decaySound && _decaySound.isPlaying) { _decaySound.Pause(); }
                if (_innerSound && _innerSound.isPlaying) { _innerSound.Pause(); }
                return;
            }

            Vector3 nearest;
            var isIn = CheckInArea(position, out nearest);
            // positiveInfinityならコライダーが無かったと見なす。
            if (nearest.Equals(Vector3.positiveInfinity)) { Debug.LogWarning($"Flexible Spatial Audio in {this.gameObject.name} haven't Area Collider."); return; }

            if (_decaySound)
            {
                _decayTransform.position = nearest;
                if (!(_innerSound && isIn) && (position - nearest).sqrMagnitude <= (_effectiveRange * _effectiveRange))
                {
                    if (!_decaySound.isPlaying) { _decaySound.Play(); }
                }
                else
                {
                    if (_decaySound.isPlaying) { _decaySound.Pause(); }
                }
            }

            if (_innerSound)
            {
                _innerTransform.position = nearest;
                if (isIn)
                {
                    if (!_innerSound.isPlaying) { _innerSound.Play(); }
                }
                else
                {
                    if (_innerSound.isPlaying) { _innerSound.Pause(); }
                }
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

            // 音の届く範囲も Bounds に含める
            // マージンを足して、接近に対して早めに有効にする
            _effectiveRange = _decaySound ? _decaySound.maxDistance + _effectiveRangeOffset : _effectiveRangeOffset;
            var effectiveRangeV3 = _effectiveRange * Vector3.one;
            compoundMin -= effectiveRangeV3;
            compoundMax += effectiveRangeV3;

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
