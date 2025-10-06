/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/VR-Follow-HUD#%E4%BD%BF%E3%81%84%E6%96%B9")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/LocalPlayer Tracking Tracker")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalPlayerTrackingTracker : UdonSharpBehaviour
    {
        [Header("General Settings")]
        public VRCPlayerApi.TrackingDataType trackingPoint = VRCPlayerApi.TrackingDataType.Head;    // 追跡箇所
        public bool enablePosition = true;
        public bool enableRotation = true;

        // Updateで使う変数キャッシュ用
        protected VRCPlayerApi _localPlayer;

        private void OnEnable()
        {
            if (Utilities.IsValid(Networking.LocalPlayer))
            {
                _localPlayer = Networking.LocalPlayer;

                // 初期位置にリセット
                var pos = enablePosition ? _localPlayer.GetTrackingData(trackingPoint).position : transform.position;
                var rot = enableRotation ? _localPlayer.GetTrackingData(trackingPoint).rotation : transform.rotation;
                transform.SetPositionAndRotation(pos, rot);
            }
        }

        public override void PostLateUpdate()
        {
            if (!enablePosition & !enableRotation) { return; }
            if (!Utilities.IsValid(_localPlayer)) { return; }

            var pos = enablePosition ? GetTrackingPosition(trackingPoint) : transform.position;
            var rot = enableRotation ? GetTrackingRotation(trackingPoint) : transform.rotation;
            transform.SetPositionAndRotation(pos, rot);
        }

        public void TrackingHead() { trackingPoint = VRCPlayerApi.TrackingDataType.Head; }
        public void TrackingLeftHand() { trackingPoint = VRCPlayerApi.TrackingDataType.LeftHand; }
        public void TrackingRightHand() { trackingPoint = VRCPlayerApi.TrackingDataType.RightHand; }
        public void TrackingOrigin() { trackingPoint = VRCPlayerApi.TrackingDataType.Origin; }
        public void TrackingAvatarRoot() { trackingPoint = VRCPlayerApi.TrackingDataType.AvatarRoot; }

        protected virtual Vector3 GetTrackingPosition(VRCPlayerApi.TrackingDataType trackingTarget)
        {
            return _localPlayer.GetTrackingData(trackingTarget).position;
        }

        protected virtual Quaternion GetTrackingRotation(VRCPlayerApi.TrackingDataType trackingTarget)
        {
            return _localPlayer.GetTrackingData(trackingTarget).rotation;
        }
    }
}
