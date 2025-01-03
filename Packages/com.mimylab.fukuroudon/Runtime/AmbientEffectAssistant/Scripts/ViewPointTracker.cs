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
    using VRC.Udon;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/ViewPoint Tracker")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ViewPointTracker : UdonSharpBehaviour
    {
        private const string ValNameViewPointPosition = "viewPointPosition";
        private const string ValNameViewPointRotation = "viewPointRotation";

        [Header("For IViewPointReceivers")]
        [SerializeField]
        private IViewPointReceiver[] _viewPointReceiver = new IViewPointReceiver[0];

        [Header("For UdonBehaviours")]
        [SerializeField]
        private UdonBehaviour[] _positionReceiver = new UdonBehaviour[0];
        [SerializeField]
        private UdonBehaviour[] _rotationReceiver = new UdonBehaviour[0];

        private VRCPlayerApi _localPlayer;
        private Vector3 _prevViewPointPosition;
        private Quaternion _prevViewPointRotation;

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            var viewPoint = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            _prevViewPointPosition = viewPoint.position;
            _prevViewPointRotation = viewPoint.rotation;
            this.transform.SetPositionAndRotation(_prevViewPointPosition, _prevViewPointRotation);

            foreach (var target in _viewPointReceiver)
            {
                if (target)
                {
                    target.viewPointTracker = this.transform;
                    target.OnViewPointChanged();
                }
            }
        }

        public override void PostLateUpdate()
        {
            var viewPoint = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            var viewPointPosition = viewPoint.position;
            var viewPointRotation = viewPoint.rotation;

            var isMoved = viewPointPosition != _prevViewPointPosition;
            var isTurned = viewPointRotation != _prevViewPointRotation;

            if (isMoved | isTurned)
            {
                this.transform.SetPositionAndRotation(viewPointPosition, viewPointRotation);

                foreach (var target in _viewPointReceiver)
                {
                    if (target && target.gameObject.activeInHierarchy && target.enabled)
                    {
                        target.OnViewPointChanged();
                    }
                }
            }

            if (isMoved)
            {
                foreach (var target in _positionReceiver)
                {
                    if (target && target.gameObject.activeInHierarchy && target.enabled)
                    {
                        target.SetProgramVariable(ValNameViewPointPosition, viewPointPosition);
                    }
                }
            }

            if (isTurned)
            {
                foreach (var target in _rotationReceiver)
                {
                    if (target && target.gameObject.activeInHierarchy && target.enabled)
                    {
                        target.SetProgramVariable(ValNameViewPointRotation, viewPointRotation);
                    }
                }
            }

            _prevViewPointPosition = viewPointPosition;
            _prevViewPointRotation = viewPointRotation;
        }
    }
}
