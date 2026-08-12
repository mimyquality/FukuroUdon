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
        // 追跡対象
        public LocalPlayerCameraTrackerCameraType trackingPoint = LocalPlayerCameraTrackerCameraType.ScreenCamera;

        public bool enablePosition = true;
        public bool enableRotation = true;

        private void OnEnable()
        {
            VRCCameraSettings targetCamera = GetCamera(trackingPoint);
            if (Utilities.IsValid(targetCamera))
            {
                // 初期位置にリセット
                Vector3 pos = enablePosition ? targetCamera.Position : transform.position;
                Quaternion rot = enableRotation ? targetCamera.Rotation : transform.rotation;
                transform.SetPositionAndRotation(pos, rot);
            }
        }

        public override void PostLateUpdate()
        {
            if (!enablePosition && !enableRotation) return;

            VRCCameraSettings camera = GetCamera(trackingPoint);
            if (Utilities.IsValid(camera))
            {
                // 初期位置にリセット
                Vector3 pos = enablePosition ? GetTrackingPosition(camera) : transform.position;
                Quaternion rot = enableRotation ? GetTrackingRotation(camera) : transform.rotation;
                transform.SetPositionAndRotation(pos, rot);
            }
        }

        public void TrackingScreenCamera() => trackingPoint = LocalPlayerCameraTrackerCameraType.ScreenCamera;
        public void TrackingPhotoCamera() => trackingPoint = LocalPlayerCameraTrackerCameraType.PhotoCamera;

        protected virtual Vector3 GetTrackingPosition(VRCCameraSettings trackingTarget)
        {
            return trackingTarget.Position;
        }

        protected virtual Quaternion GetTrackingRotation(VRCCameraSettings trackingTarget)
        {
            return trackingTarget.Rotation;
        }

        private VRCCameraSettings GetCamera(LocalPlayerCameraTrackerCameraType cameraType)
        {
            if (cameraType == LocalPlayerCameraTrackerCameraType.PhotoCamera)
            {
                return VRCCameraSettings.PhotoCamera;
            }
            
            return VRCCameraSettings.ScreenCamera;
        }
    }
}