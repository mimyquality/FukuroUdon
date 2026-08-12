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
        private Renderer[] _renderers = System.Array.Empty<Renderer>();
        [SerializeField]
        private GameObject[] _gameObjects = System.Array.Empty<GameObject>();

        [Header("Bounds Settings")]
        [SerializeField, Tooltip("Sphere, Capsule, Box, Mesh(Convex 有効) のコライダーが使えます。")]
        private Collider[] _area = System.Array.Empty<Collider>();
        [SerializeField]
        private bool _areaIsStatic = true;
        [SerializeField]
        private bool _invert = false;
        [SerializeField, Tooltip("有効にすると、VRCカメラ//ドローンも視界として評価に含めます。")]
        private bool _includeVRCCamera = false;

        private Bounds _areaBounds;
        private VRCCameraSettings _screenCamera;
        private VRCCameraSettings _photoCamera;
        private bool _wasIn = false;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            var col = GetComponent<Collider>();
            if (col)
            {
                if (System.Array.IndexOf(_area, col) < 0)
                {
                    Collider[] tmp_area = new Collider[_area.Length + 1];
                    _area.CopyTo(tmp_area, 0);
                    tmp_area[_area.Length] = col;
                    _area = tmp_area;
                }
            }
        }
#endif

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

            Vector3 position = _screenCamera.Position;
            bool isIn = _areaIsStatic ?
                       _areaBounds.Contains(position) && CheckInArea(position) :
                       CheckInArea(position);

            if (_includeVRCCamera && _photoCamera.Active && !isIn)
            {
                Vector3 photoPosition = _photoCamera.Position;
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
            foreach (Collider col in _area)
            {
                if (!col) { continue; }

                Bounds bounds = col.bounds;
                if (bounds.extents.Equals(Vector3.zero)) { continue; }

                compoundMin = Vector3.Min(compoundMin, bounds.min);
                compoundMax = Vector3.Max(compoundMax, bounds.max);
            }

            if (compoundMin.Equals(Vector3.positiveInfinity))
            {
                _areaBounds = new Bounds();
                return;
            }

            Vector3 center = (compoundMin + compoundMax) / 2f;
            Vector3 size = compoundMax - compoundMin;
            _areaBounds = new Bounds(center, size);
        }

        private bool CheckInArea(Vector3 position)
        {
            foreach (Collider col in _area)
            {
                if (!col) { continue; }
                if (!col.enabled) { continue; }
                if (!col.gameObject.activeInHierarchy) { continue; }

                Vector3 point = col.ClosestPoint(position);
                if (point == position)
                {
                    return true;
                }
            }

            return false;
        }

        private void ToggleTargetsEnabled(bool value)
        {
            foreach (Renderer target in _renderers)
            {
                if (target) { target.enabled = value; }
            }
            foreach (GameObject target in _gameObjects)
            {
                if (target) { target.SetActive(value); }
            }
        }
    }
}
