/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Boundary Culling")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BoundaryCulling : IViewPointReceiver
    {
        [Header("Targets")]
        [SerializeField]
        private Renderer[] _renderers = new Renderer[0];

        [Header("Bound Settings")]
        [SerializeField]
        private Transform _point;
        [SerializeField]
        private Vector3 _normal = Vector3.up;

        private bool _prevEnabled = true;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_point) _point = transform;
            ToggleTargetsEnabled(!_prevEnabled);

            _initialized = true;
        }

        public override void ReceiveViewPoint(Vector3 position, Quaternion rotation)
        {
            Initialize();

            var direction = position - _point.position;
            var borderNormal = _point.rotation * _normal;
            ToggleTargetsEnabled(Vector3.Dot(borderNormal, direction) >= 0);
        }

        private void ToggleTargetsEnabled(bool value)
        {
            if (value != _prevEnabled)
            {
                foreach (var target in _renderers)
                {
                    if (target) { target.enabled = value; }
                }
                _prevEnabled = value;
            }
        }
    }
}
