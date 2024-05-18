/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Flexible Reverb Zone")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlexibleReverbZone : IViewPointReceiver
    {
        [SerializeField]
        AudioReverbZone _reverbZone;

        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];

        Transform _reverbZoneTransform;

        private Vector3 _viewPointPosition;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (_reverbZone) { _reverbZoneTransform = _reverbZone.transform; }

            _initialized = true;
        }

        public override void ReceiveViewPoint(Vector3 position, Quaternion rotation)
        {
            Initialize();

            if (position == _viewPointPosition) { return; }

            SnapViewPointPosition(position);
            _viewPointPosition = position;
        }

        private void SnapViewPointPosition(Vector3 vpPosition)
        {
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

            if (_reverbZone)
            {
                _reverbZoneTransform.position = nearest;
            }
        }
    }
}
