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

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    using UnityEditor;
#endif

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#boundary-culling")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Boundary Culling")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BoundaryCulling : UdonSharpBehaviour
    {
        [SerializeField]
        private Renderer[] _renderers = new Renderer[0];
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];

        [Header("Bound Settings")]
        [SerializeField]
        private Transform _point;
        [SerializeField]
        private Vector3 _normal = Vector3.up;
        [SerializeField, Tooltip("Include the VRC Camera and Drone for culling checks")]
        private bool _includeVRCCamera = false;

        private VRCCameraSettings _screenCamera;
        private VRCCameraSettings _photoCamera;
        private bool _wasIn = false;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var point = _point ? _point : this.transform;
            var pos = point.position;
            var normal = (_normal != Vector3.zero) ? _normal.normalized : Vector3.up;
            var normalRotation = point.rotation * Quaternion.LookRotation(normal);
            var size = HandleUtility.GetHandleSize(pos);
            var plane = new Vector3[]
            {
                normalRotation * new Vector3(size, size, 0) + pos,
                normalRotation * new Vector3(-size, size, 0) + pos,
                normalRotation * new Vector3(-size, -size, 0) + pos,
                normalRotation * new Vector3(size, -size, 0) + pos,
            };
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, normalRotation * Vector3.forward * size + pos);
            Gizmos.DrawLineStrip(plane, true);
        }
#endif

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_point) _point = transform;
            _screenCamera = VRCCameraSettings.ScreenCamera;
            _photoCamera = VRCCameraSettings.PhotoCamera;
            // ClientSim 対策
            if (_photoCamera == null) { _includeVRCCamera = false; }

            ToggleTargetsEnabled(_wasIn);

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(_screenCamera)) { return; }

            var borderNormal = (_normal != Vector3.zero) ? _point.rotation * _normal : Vector3.up;
            var direction = _screenCamera.Position - _point.position;
            var isIn = Vector3.Dot(borderNormal, direction) <= 0.0f;

            if (_includeVRCCamera && _photoCamera.Active && !isIn)
            {
                direction = _photoCamera.Position - _point.position;
                isIn = Vector3.Dot(borderNormal, direction) <= 0.0f;
            }

            if (_wasIn != isIn)
            {
                ToggleTargetsEnabled(isIn);
                _wasIn = isIn;
            }
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
