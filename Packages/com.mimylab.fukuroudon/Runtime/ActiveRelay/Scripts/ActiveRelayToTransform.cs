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

    [System.Flags]
    public enum ActiveRelayToTransformChangeProperties
    {
        //None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-transform")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to Transform")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToTransform : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private Transform[] _transforms = new Transform[0];
        [SerializeField, EnumFlag]
        private ActiveRelayToTransformChangeProperties _changeProperty =
            ActiveRelayToTransformChangeProperties.Position |
            ActiveRelayToTransformChangeProperties.Rotation;
        [SerializeField]
        private Vector3 _position = Vector3.zero;
        [SerializeField]
        private Quaternion _rotation = Quaternion.identity;
        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [SerializeField]
        private Space _relativeTo;

        [Header("Advanced Settings")]
        [SerializeField]
        private Transform _referenceTransform = null;

        private VRCPickup[] _pickups;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _pickups = new VRCPickup[_transforms.Length];
            var positionOrRotation =
                (int)ActiveRelayToTransformChangeProperties.Position |
                (int)ActiveRelayToTransformChangeProperties.Rotation;
            var dropFlag = ((int)_changeProperty & positionOrRotation) > 0;
            if (dropFlag)
            {
                for (int i = 0; i < _transforms.Length; i++)
                {
                    _pickups[i] = _transforms[i].GetComponent<VRCPickup>();
                }
            }

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            if (_eventType == ActiveRelayEventType.ActiveAndInactive ||
                _eventType == ActiveRelayEventType.Active)
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
            if (_eventType == ActiveRelayEventType.ActiveAndInactive ||
                _eventType == ActiveRelayEventType.Inactive)
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
            Vector3 position = _referenceTransform ? _referenceTransform.position : _position;
            Quaternion rotation = _referenceTransform ? _referenceTransform.rotation : _rotation;
            Vector3 scale = _referenceTransform ? _referenceTransform.lossyScale : _scale;

            for (int i = 0; i < _transforms.Length; i++)
            {
                if (_pickups[i])
                {
                    _pickups[i].Drop();
                }

                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Position) > 0)
                {
                    _transforms[i].position = position;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Rotation) > 0)
                {
                    _transforms[i].rotation = rotation;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Scale) > 0)
                {
                    // ワールド空間のスケールを適用
                    Transform parent = _transforms[i].parent;
                    Vector3 parentScale = parent ? parent.lossyScale : Vector3.one;
                    _transforms[i].localScale = new Vector3
                    (
                        Mathf.Approximately(parentScale.x, 0.0f) ? scale.x : scale.x / parentScale.x,
                        Mathf.Approximately(parentScale.y, 0.0f) ? scale.y : scale.y / parentScale.y,
                        Mathf.Approximately(parentScale.z, 0.0f) ? scale.z : scale.z / parentScale.z
                    );
                }
            }
        }

        private void RelativeTransforming()
        {
            Vector3 position = _referenceTransform ? _referenceTransform.localPosition : _position;
            Quaternion rotation = _referenceTransform ? _referenceTransform.localRotation : _rotation;
            Vector3 scale = _referenceTransform ? _referenceTransform.localScale : _scale;

            for (int i = 0; i < _transforms.Length; i++)
            {
                if (_pickups[i])
                {
                    _pickups[i].Drop();
                }

                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Position) > 0)
                {
                    _transforms[i].localPosition = position;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Rotation) > 0)
                {
                    _transforms[i].localRotation = rotation;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperties.Scale) > 0)
                {
                    _transforms[i].localScale = scale;
                }
            }
        }
    }
}
