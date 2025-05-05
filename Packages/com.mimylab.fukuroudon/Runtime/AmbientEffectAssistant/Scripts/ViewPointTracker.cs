/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.Udon;
    using VRC.SDK3.Rendering;

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

        private VRCCameraSettings _screenCamera;
        private Vector3 _prevViewPointPosition;
        private Quaternion _prevViewPointRotation;

        private void OnEnable()
        {
            _screenCamera = VRCCameraSettings.ScreenCamera;
            _prevViewPointPosition = _screenCamera.Position;
            _prevViewPointRotation = _screenCamera.Rotation;
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
            var viewPointPosition = _screenCamera.Position;
            var viewPointRotation = _screenCamera.Rotation;

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
            }

            _prevViewPointPosition = viewPointPosition;
            _prevViewPointRotation = viewPointRotation;
        }
    }
}
