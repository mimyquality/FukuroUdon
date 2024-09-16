/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/SC2 Caster")]
    [RequireComponent(typeof(VRCObjectSync))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class SC2Caster : UdonSharpBehaviour
    {
        public bool immobile = false;
        [Tooltip("meter/sec (If physics Rigidbody is attached, the units for this parameter are [N].)")]
        public float moveSpeed = 2.0f;
        [Tooltip("degree/sec (If physics Rigidbody is attached, the units for this parameter are [Nm].)")]
        public float turnSpeed = 60.0f;

        private Transform _transform;
        private Rigidbody _rigidbody;
        private VRCObjectSync _objectSync;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
            _objectSync = GetComponent<VRCObjectSync>();

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public void Move(Vector3 inputValue)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }
            if (immobile) { return; }

            var shift = Time.deltaTime * moveSpeed * inputValue;
            if (_rigidbody)
            {
                if (_rigidbody.isKinematic)
                {
                    _rigidbody.MovePosition(_rigidbody.position + _rigidbody.rotation * shift);
                }
                else
                {
                    _rigidbody.AddRelativeForce(shift, ForceMode.Impulse);
                }
            }
            else
            {
                _transform.Translate(shift, Space.Self);
            }
        }

        public void Turn(float inputValue)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }
            if (immobile) { return; }

            var angle = Time.deltaTime * turnSpeed * inputValue;
            if (_rigidbody)
            {
                if (_rigidbody.isKinematic)
                {
                    _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.AngleAxis(angle, Vector3.up));
                }
                else
                {
                    _rigidbody.AddRelativeTorque(0.0f, angle, 0.0f, ForceMode.Impulse);
                }
            }
            else
            {
                _transform.Rotate(Vector3.up, angle, Space.Self);
            }
        }
    }
}
