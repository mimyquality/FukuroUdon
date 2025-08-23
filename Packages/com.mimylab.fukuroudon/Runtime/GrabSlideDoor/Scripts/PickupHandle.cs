/*
Copyright (c) 2021 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    using VRC.SDK3.Components;
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grab-SlideDoor#%E4%BD%BF%E3%81%84%E6%96%B9")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/Pickup Handle")]
    [RequireComponent(typeof(VRCPickup))]
    public class PickupHandle : UdonSharpBehaviour
    {
        [SerializeField]
        private Transform returnPoint = null;
        [Tooltip("When set to this, its Udon Enable is linked to the pickup.")]
        [SerializeField]
        private UdonBehaviour workingTogether = null;

        private VRCPickup _pickup;
        private Vector3 _returnPosition;
        private Quaternion _returnRotation;
        private bool _isLinkedUdonEnabled = false;

        [FieldChangeCallback(nameof(Position))]
        private Vector3 _position = Vector3.zero;
        private Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value) { return; }

                _position = value;

                _EnableLinkedUdon();
            }
        }

        private void Start()
        {
            _pickup = GetComponent<VRCPickup>();

            _returnPosition = this.transform.position;
            _returnRotation = this.transform.rotation;
            if (returnPoint)
            {
                returnPoint.SetPositionAndRotation(_returnPosition, _returnRotation);
            }

            Position = transform.position;
        }

        public override void OnDeserialization()
        {
            Position = transform.position;
        }

        public override void OnPickup()
        {
            _EnableLinkedUdon();
        }

        public override void OnDrop()
        {
            if (returnPoint)
            {
                this.transform.SetPositionAndRotation(returnPoint.position, returnPoint.rotation);
            }
            else
            {
                this.transform.SetPositionAndRotation(_returnPosition, _returnRotation);
            }
        }

        public void _EnableLinkedUdon()
        {
            if (_isLinkedUdonEnabled) { return; }

            SetEnabledLinkUdon(true);
            SendCustomEventDelayedSeconds(nameof(_DisableLinkUdon), 1.0f);
        }

        public void _DisableLinkUdon()
        {
            if ((Position == transform.position) && !_pickup.IsHeld)
            {
                SetEnabledLinkUdon(false);
            }
            else
            {
                _position = transform.position;
                SendCustomEventDelayedSeconds(nameof(_DisableLinkUdon), 1.0f);
            }
        }

        private void SetEnabledLinkUdon(bool value)
        {
            if (workingTogether)
            {
                _isLinkedUdonEnabled = value;
                workingTogether.enabled = value;
            }
        }
    }
}
