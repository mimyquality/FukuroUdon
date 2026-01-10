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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#flexible-reverb-zone")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Flexible Reverb Zone")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlexibleReverbZone : UdonSharpBehaviour
    {
        [SerializeField]
        AudioReverbZone _reverbZone;

        [Header("Bounds Settings")]
        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];
        [SerializeField]
        private bool _areaIsStatic = true;

        private Transform _reverbZoneTransform;
        private Bounds _areaBounds;
        //private VRCCameraSettings _camera;
        private VRCPlayerApi _localPlayer;

        private void Reset()
        {
            _reverbZone = GetComponent<AudioReverbZone>();
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_reverbZone) { _reverbZone = GetComponent<AudioReverbZone>(); }
            if (_reverbZone) { _reverbZoneTransform = _reverbZone.transform; }
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
            if (!_reverbZone) { return; }
            //if (!Utilities.IsValid(_camera)) { return; }
            //var position = _camera.Position;
            Vector3 position = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

            if (_areaIsStatic && !_areaBounds.Contains(position))
            {
                // 絶対に音の届かない距離にいる
                return;
            }

            Vector3 nearest;
            CheckInArea(position, out nearest);
            // positiveInfinityならコライダーが無かったと見なす。
            if (nearest.Equals(Vector3.positiveInfinity)) { Debug.LogWarning($"Flexible Spatial Audio in {this.gameObject.name} haven't Area Collider."); return; }

            _reverbZoneTransform.position = nearest;
        }

        public void RecalculateAreaBounds()
        {
            Vector3 compoundMin = Vector3.positiveInfinity;
            Vector3 compoundMax = Vector3.negativeInfinity;
            foreach (Collider collider in _area)
            {
                if (!collider) { continue; }

                Bounds bounds = collider.bounds;
                if (bounds.extents.Equals(Vector3.zero)) { continue; }

                compoundMin = Vector3.Min(compoundMin, bounds.min);
                compoundMax = Vector3.Max(compoundMax, bounds.max);
            }

            if (compoundMin.Equals(Vector3.positiveInfinity))
            {
                _areaBounds = new Bounds();
                return;
            }

            // ReverbZone の範囲も Bounds に含める
            // マージンを足して、接近に対して早めに有効にする
            Vector3 effectiveRange = _reverbZone ? (_reverbZone.maxDistance + 1.0f) * Vector3.one : Vector3.one;
            compoundMin -= effectiveRange;
            compoundMax += effectiveRange;

            Vector3 center = (compoundMin + compoundMax) * 0.5f;
            Vector3 size = compoundMax - compoundMin;
            _areaBounds = new Bounds(center, size);
        }

        private bool CheckInArea(Vector3 position, out Vector3 nearest)
        {
            nearest = Vector3.positiveInfinity;
            foreach (Collider collider in _area)
            {
                if (!collider) { continue; }
                if (!collider.enabled) { continue; }
                if (!collider.gameObject.activeInHierarchy) { continue; }

                Vector3 point = collider.ClosestPoint(position);
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
