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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#flexible-reverb-zone")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Flexible Reverb Zone")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlexibleReverbZone : UdonSharpBehaviour
    {
        [SerializeField]
        AudioReverbZone _reverbZone;

        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];

        private Transform _reverbZoneTransform;
        private VRCCameraSettings _camera;

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
            _camera = VRCCameraSettings.ScreenCamera;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void PostLateUpdate()
        {
            if (!_reverbZone) { return; }
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

            _reverbZoneTransform.position = nearest;
        }
    }
}
