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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#area-culling")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Area Culling")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AreaCulling : UdonSharpBehaviour
    {
        [SerializeField]
        private Renderer[] _renderers = new Renderer[0];
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];

        [Header("Bounds Settings")]
        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];
        [SerializeField]
        private bool _areaIsStatic = true;
        [SerializeField]
        private bool _invert = false;
        [SerializeField, Tooltip("Include the VRC Camera and Drone for culling checks")]
        private bool _includeVRCCamera = false;

        private Bounds _areaBounds;
        private VRCCameraSettings _screenCamera;
        private VRCCameraSettings _photoCamera;
        private bool _wasIn = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _screenCamera = VRCCameraSettings.ScreenCamera;
            _photoCamera = VRCCameraSettings.PhotoCamera;
            // ClientSim 対策
            if (_photoCamera == null) { _includeVRCCamera = false; }

            ToggleTargetsEnabled(_wasIn ^ _invert);

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();
            RecalculateAreaBounds();
        }

        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(_screenCamera)) { return; }

            var position = _screenCamera.Position;
            var isIn = _areaIsStatic ?
                       _areaBounds.Contains(position) && CheckInArea(position) :
                       CheckInArea(position);

            if (_includeVRCCamera && _photoCamera.Active && !isIn)
            {
                var photoPosition = _photoCamera.Position;
                isIn = _areaIsStatic ?
                       _areaBounds.Contains(photoPosition) && CheckInArea(photoPosition) :
                       CheckInArea(photoPosition);
            }

            if (isIn != _wasIn)
            {
                ToggleTargetsEnabled(isIn ^ _invert);
                _wasIn = isIn;
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

            var center = (compoundMin + compoundMax) / 2f;
            var size = compoundMax - compoundMin;
            _areaBounds = new Bounds(center, size);
        }

        private bool CheckInArea(Vector3 position)
        {
            foreach (var collider in _area)
            {
                if (!collider) { continue; }
                if (!collider.enabled) { continue; }
                if (!collider.gameObject.activeInHierarchy) { continue; }

                var point = collider.ClosestPoint(position);
                if (point == position)
                {
                    return true;
                }
            }

            return false;
        }

        private void ToggleTargetsEnabled(bool value)
        {
            foreach (var target in _renderers)
            {
                if (target) { target.enabled = value; }
            }
            foreach (var target in _gameObjects)
            {
                if (target) { target.SetActive(value); }
            }
        }
    }
}
