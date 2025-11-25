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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#boundary-culling")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Boundary Culling")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BoundaryCulling : UdonSharpBehaviour
    {
        [Header("Targets")]
        [SerializeField]
        private Renderer[] _renderers = new Renderer[0];
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];

        [Header("Bound Settings")]
        [SerializeField]
        private Transform _point;
        [SerializeField]
        private Vector3 _normal = Vector3.up;

        private VRCCameraSettings _camera;
        private bool _wasIn = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_point) _point = transform;
            _camera = VRCCameraSettings.ScreenCamera;

            ToggleTargetsEnabled(_wasIn);

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

            var direction = position - _point.position;
            var borderNormal = _point.rotation * _normal;
            var isIn = Vector3.Dot(borderNormal, direction) >= 0.0f;

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
