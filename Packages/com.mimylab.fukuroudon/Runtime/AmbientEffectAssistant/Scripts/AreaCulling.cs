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

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Area Culling")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AreaCulling : IViewPointReceiver
    {
        [Header("Targets")]
        [SerializeField]
        private Renderer[] _renderers = new Renderer[0];
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];

        [Space]
        [SerializeField]
        private bool _invert;

        [Header("Bound Settings")]
        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];

        private bool _prevEnabled;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _prevEnabled = !_invert;
            ToggleTargetsEnabled(!_prevEnabled);

            _initialized = true;
        }

        public override void ReceiveViewPoint(Vector3 position, Quaternion rotation)
        {
            Initialize();

            CullingByViewPointPosition(position);
        }

        private void CullingByViewPointPosition(Vector3 vpPosition)
        {
            var isIn = false;
            //var nearest = Vector3.positiveInfinity;
            foreach (var col in _area)
            {
                if (!col) { continue; }

                var point = col.ClosestPoint(vpPosition);
                //nearest = (point - vpPosition).sqrMagnitude < (nearest - vpPosition).sqrMagnitude ? point : nearest;

                if (point == vpPosition)
                {
                    isIn = true;
                    break;
                }
            }

            ToggleTargetsEnabled(isIn ^ _invert);
        }

        private void ToggleTargetsEnabled(bool value)
        {
            if (value == _prevEnabled) { return; }

            foreach (var target in _renderers)
            {
                if (target) { target.enabled = value; }
            }
            foreach (var target in _gameObjects)
            {
                if (target) { target.SetActive(value); }
            }

            _prevEnabled = value;
        }
    }
}
