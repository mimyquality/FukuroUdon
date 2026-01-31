/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-with-return")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay with/ActiveRelay with Return")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayWithReturn : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayActiveEvent _eventType = ActiveRelayActiveEvent.Inactive;
        [SerializeField, EnumFlag]
        private ActiveRelayToTransformChangeProperties _changeProperty =
            ActiveRelayToTransformChangeProperties.Position |
            ActiveRelayToTransformChangeProperties.Rotation;
        [SerializeField]
        private Space _relativeTo = Space.World;

        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _startScale;
        private Vector3 _startLocalScale;

        private VRCPickup _pickup = null;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            switch (_relativeTo)
            {
                case Space.World:
                    _startPosition = this.transform.position;
                    _startRotation = this.transform.rotation;
                    _startScale = this.transform.lossyScale;
                    _startLocalScale = this.transform.localScale;
                    break;
                case Space.Self:
                    _startPosition = this.transform.localPosition;
                    _startRotation = this.transform.localRotation;
                    _startScale = this.transform.localScale;
                    _startLocalScale = this.transform.localScale;
                    break;
            }

            var positionOrRotation =
                (int)ActiveRelayToTransformChangeProperties.Position |
                (int)ActiveRelayToTransformChangeProperties.Rotation;
            if (((int)_changeProperty & positionOrRotation) > 0)
            {
                _pickup = GetComponent<VRCPickup>();
            }

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive ||
                _eventType == ActiveRelayActiveEvent.Active)
            {
                switch (_relativeTo)
                {
                    case Space.World: Transforming(); break;
                    case Space.Self: RelativeTransforming(); break;
                }
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive ||
                _eventType == ActiveRelayActiveEvent.Inactive)
            {
                switch (_relativeTo)
                {
                    case Space.World: Transforming(); break;
                    case Space.Self: RelativeTransforming(); break;
                }
            }
        }

        private void Transforming()
        {
            if (_pickup)
            {
                _pickup.Drop();
            }

            if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Position) > 0)
            {
                this.transform.position = _startPosition;
            }
            if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Rotation) > 0)
            {
                this.transform.rotation = _startRotation;
            }
            if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Scale) > 0)
            {
                // ワールド空間のスケールを適用
                Transform parent = this.transform.parent;
                Vector3 parentScale = parent ? parent.lossyScale : Vector3.one;
                this.transform.localScale = new Vector3
                    (
                        Mathf.Approximately(parentScale.x, 0.0f) ? _startLocalScale.x : _startScale.x / parentScale.x,
                        Mathf.Approximately(parentScale.y, 0.0f) ? _startLocalScale.y : _startScale.y / parentScale.y,
                        Mathf.Approximately(parentScale.z, 0.0f) ? _startLocalScale.z : _startScale.z / parentScale.z
                    );
            }
        }

        private void RelativeTransforming()
        {
            if (_pickup)
            {
                _pickup.Drop();
            }

            if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Position) > 0)
            {
                this.transform.localPosition = _startPosition;
            }
            if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Rotation) > 0)
            {
                this.transform.localRotation = _startRotation;
            }
            if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Scale) > 0)
            {
                this.transform.localScale = _startLocalScale;
            }
        }
    }
}
