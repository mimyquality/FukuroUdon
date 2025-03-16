/*
Copyright (c) 2025 Mimy Quality
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
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to Transform")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToTransform : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private Transform[] _transforms = new Transform[0];
        [SerializeField]
        private Vector3 _position = Vector3.zero;
        [SerializeField]
        private Quaternion _rotation = Quaternion.identity;
        [SerializeField]
        private Space _relativeTo;

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                TranslateAndRotate();
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                TranslateAndRotate();
            }
        }

        private void TranslateAndRotate()
        {
            for (int i = 0; i < _transforms.Length; i++)
            {
                switch (_relativeTo)
                {
                    case Space.World: _transforms[i].SetPositionAndRotation(_position, _rotation); break;
                    case Space.Self: _transforms[i].SetLocalPositionAndRotation(_position, _rotation); break;
                }
            }
        }
    }
}
