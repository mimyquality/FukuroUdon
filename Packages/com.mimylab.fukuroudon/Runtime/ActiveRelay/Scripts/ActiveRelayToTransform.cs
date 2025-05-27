/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;

    [System.Flags]
    public enum ActiveRelayToTransformChangeProperty
    {
        //None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to Transform")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToTransform : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private Transform[] _transforms = new Transform[0];
        [SerializeField, EnumFlag]
        private ActiveRelayToTransformChangeProperty _changeProperty =
            ActiveRelayToTransformChangeProperty.Position |
            ActiveRelayToTransformChangeProperty.Rotation;
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

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
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
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
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
            var position = _referenceTransform ? _referenceTransform.position : _position;
            var rotation = _referenceTransform ? _referenceTransform.rotation : _rotation;
            var scale = _referenceTransform ? _referenceTransform.lossyScale : _scale;

            for (int i = 0; i < _transforms.Length; i++)
            {
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperty.Position) > 0)
                {
                    _transforms[i].position = position;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperty.Rotation) > 0)
                {
                    _transforms[i].rotation = rotation;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperty.Scale) > 0)
                {
                    // ワールド空間のスケールを適用
                    var parent = _transforms[i].parent;
                    var parentScale = parent ? parent.lossyScale : Vector3.one;
                    _transforms[i].localScale = new Vector3
                    (
                        scale.x / parentScale.x,
                        scale.y / parentScale.y,
                        scale.z / parentScale.z
                    );
                }
            }
        }

        private void RelativeTransforming()
        {
            var position = _referenceTransform ? _referenceTransform.localPosition : _position;
            var rotation = _referenceTransform ? _referenceTransform.localRotation : _rotation;
            var scale = _referenceTransform ? _referenceTransform.localScale : _scale;

            for (int i = 0; i < _transforms.Length; i++)
            {
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperty.Position) > 0)
                {
                    _transforms[i].localPosition = position;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperty.Rotation) > 0)
                {
                    _transforms[i].localRotation = rotation;
                }
                if (((int)_changeProperty & (int)ActiveRelayToTransformChangeProperty.Scale) > 0)
                {
                    _transforms[i].localScale = scale;
                }
            }
        }
    }
}
