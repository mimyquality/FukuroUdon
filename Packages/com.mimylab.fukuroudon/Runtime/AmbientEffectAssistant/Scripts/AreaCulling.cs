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
        [Header("Targets")]
        [SerializeField]
        private Renderer[] _renderers = new Renderer[0];
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];

        [Space]
        [SerializeField]
        private bool _invert = false;

        [Header("Bound Settings")]
        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];

        private VRCCameraSettings _camera;
        private bool _wasIn = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _camera = VRCCameraSettings.ScreenCamera;

            ToggleTargetsEnabled(_wasIn ^ _invert);

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

            var isIn = false;
            //var nearest = Vector3.positiveInfinity;
            foreach (var collider in _area)
            {
                if (!collider) { continue; }

                var point = collider.ClosestPoint(position);
                //nearest = (point - vpPosition).sqrMagnitude < (nearest - vpPosition).sqrMagnitude ? point : nearest;

                if (point == position)
                {
                    isIn = true;
                    break;
                }
            }

            if (_wasIn != isIn)
            {
                ToggleTargetsEnabled(isIn ^ _invert);
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
