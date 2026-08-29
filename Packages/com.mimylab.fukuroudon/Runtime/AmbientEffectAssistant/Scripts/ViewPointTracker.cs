/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Rendering;
    using VRC.Udon;

    [System.Obsolete("このコンポーネントは不要となりました。今後メンテされません。")]
    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#viewpoint-tracker")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ViewPointTracker : UdonSharpBehaviour
    {
        private const string ValNameViewPointPosition = "viewPointPosition";
        private const string ValNameViewPointRotation = "viewPointRotation";

        [Header("For IViewPointReceivers")]
        [SerializeField]
        private IViewPointReceiver[] _viewPointReceiver = System.Array.Empty<IViewPointReceiver>();

        [Header("For UdonBehaviours")]
        [SerializeField]
        private UdonBehaviour[] _positionReceiver = System.Array.Empty<UdonBehaviour>();

        [SerializeField]
        private UdonBehaviour[] _rotationReceiver = System.Array.Empty<UdonBehaviour>();

        private VRCCameraSettings _screenCamera;
        private Vector3 _prevViewPointPosition;
        private Quaternion _prevViewPointRotation;

        private void OnEnable()
        {
            _screenCamera = VRCCameraSettings.ScreenCamera;
            _prevViewPointPosition = _screenCamera.Position;
            _prevViewPointRotation = _screenCamera.Rotation;
            this.transform.SetPositionAndRotation(_prevViewPointPosition, _prevViewPointRotation);

            foreach (IViewPointReceiver target in _viewPointReceiver)
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
            Vector3 viewPointPosition = _screenCamera.Position;
            Quaternion viewPointRotation = _screenCamera.Rotation;

            bool isMoved = viewPointPosition != _prevViewPointPosition;
            bool isTurned = viewPointRotation != _prevViewPointRotation;

            if (isMoved | isTurned)
            {
                this.transform.SetPositionAndRotation(viewPointPosition, viewPointRotation);

                foreach (IViewPointReceiver target in _viewPointReceiver)
                {
                    if (target && target.isActiveAndEnabled)
                    {
                        target.OnViewPointChanged();
                    }
                }

                if (isMoved)
                {
                    foreach (UdonBehaviour target in _positionReceiver)
                    {
                        if (target && target.isActiveAndEnabled)
                        {
                            target.SetProgramVariable(ValNameViewPointPosition, viewPointPosition);
                        }
                    }
                }

                if (isTurned)
                {
                    foreach (UdonBehaviour target in _rotationReceiver)
                    {
                        if (target && target.isActiveAndEnabled)
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