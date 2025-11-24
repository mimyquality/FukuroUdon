/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Rendering;

    public enum LocalPlayerCameraTrackerCameraType
    {
        ScreenCamera,
        PhotoCamera
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/VR-Follow-HUD")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/LocalPlayer Camera Tracker")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1000)]
    public class LocalPlayerCameraTracker : UdonSharpBehaviour
    {
        [Header("General Settings")]
        public LocalPlayerCameraTrackerCameraType trackingPoint = LocalPlayerCameraTrackerCameraType.ScreenCamera;    // 追跡対象
        public bool enablePosition = true;
        public bool enableRotation = true;

        protected void OnEnable()
        {
            var camera = GetCamera(trackingPoint);
            if (Utilities.IsValid(camera))
            {
                // 初期位置にリセット
                var pos = enablePosition ? camera.Position : transform.position;
                var rot = enableRotation ? camera.Rotation : transform.rotation;
                transform.SetPositionAndRotation(pos, rot);
            }
        }

        public override void PostLateUpdate()
        {
            if (!enablePosition & !enableRotation) { return; }

            var camera = GetCamera(trackingPoint);
            if (Utilities.IsValid(camera))
            {
                // 初期位置にリセット
                var pos = enablePosition ? GetTrackingPosition(camera) : transform.position;
                var rot = enableRotation ? GetTrackingRotation(camera) : transform.rotation;
                transform.SetPositionAndRotation(pos, rot);
            }
        }

        public void TrackingScreenCamera() { trackingPoint = LocalPlayerCameraTrackerCameraType.ScreenCamera; }
        public void TrackingPhotoCamera() { trackingPoint = LocalPlayerCameraTrackerCameraType.PhotoCamera; }

        protected virtual Vector3 GetTrackingPosition(VRCCameraSettings trackingTarget)
        {
            return trackingTarget.Position;
        }

        protected virtual Quaternion GetTrackingRotation(VRCCameraSettings trackingTarget)
        {
            return trackingTarget.Rotation;
        }

        protected VRCCameraSettings GetCamera(LocalPlayerCameraTrackerCameraType cameraType)
        {
            VRCCameraSettings camera = null;
            switch (cameraType)
            {
                case LocalPlayerCameraTrackerCameraType.ScreenCamera:
                    camera = VRCCameraSettings.ScreenCamera;
                    break;
                case LocalPlayerCameraTrackerCameraType.PhotoCamera:
                    camera = VRCCameraSettings.PhotoCamera;
                    break;
            }

            return camera;
        }
    }
}
