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

        [SerializeField, Tooltip("meter")]
        private float _effectiveRangeOffset = 1.0f;

        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];

        private Transform _decayTransform;
        private Transform _innerTransform;
        private VRCCameraSettings _camera;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (_decaySound) { _decayTransform = _decaySound.transform; }
            if (_innerSound) { _innerTransform = _innerSound.transform; }
            _camera = VRCCameraSettings.ScreenCamera;

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
            var isIn = false;
            foreach (var collider in _area)
            {
                if (!collider) { continue; }

                var point = collider.ClosestPoint(position);
                nearest = (point - position).sqrMagnitude < (nearest - position).sqrMagnitude ? point : nearest;

                if (isIn = point == position) { break; }
            }
            // positiveInfinityならコライダーが無かったと見なす。
            if (nearest.Equals(Vector3.positiveInfinity)) { Debug.LogWarning($"Flexible Spatial Audio in {this.gameObject.name} haven't Area Collider."); return; }

            if (_decaySound)
            {
                // マージンを足して、接近に対して早めに有効にする
                var effectiveRange = _decaySound.maxDistance + _effectiveRangeOffset;

                _decayTransform.position = nearest;
                //_decaySound.enabled = !(_innerSound && isIn) && (vpPosition - nearest).sqrMagnitude <= (effectiveRange * effectiveRange);
                if (!(_innerSound && isIn) && (position - nearest).sqrMagnitude <= (effectiveRange * effectiveRange))
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
                //_innerSound.enabled = isIn;
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
    }
}
