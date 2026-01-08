/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/VR-Follow-HUD#%E4%BD%BF%E3%81%84%E6%96%B9")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/VR Follow HUD/Tracking Follow Tracker")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1000)]
    public class VRFollowHUD : LocalPlayerTrackingTracker
    {
        [SerializeField]
        private bool _vROnly = true; // VRモードでのみ有効

        [Header("Position Settings")]
        public bool syncPosition = true;  // 位置同期
        [Range(0.0f, 1.0f)]
        public float followMoveSpeed = 0.1f;   // 追跡速度
        [Min(0.0f)]
        public float distanceRange = 1.0f; // 最大離散距離
        [Min(0.0f)]
        public float moveThreshold = 0.0f;  // 追跡開始閾値

        [Header("Rotation Settings")]
        public bool syncRotation = false;   // 回転同期
        [Range(0.0f, 1.0f)]
        public float followRotateSpeed = 0.02f;    // 追跡回転速度
        [Range(0.0f, 180.0f)]
        public float angleRange = 60.0f;  // 回転速度の加速閾値
        [Range(0.0f, 180.0f)]
        public float rotateThreshold = 0.0f; // 追跡回転開始閾値        
        public bool lockRoll = true, lockPitch = false, lockYaw = false;   // 遅延せず回転同期(軸毎)

        // 計算用
        private float _pauseThreshold; // 追跡終了閾値、追跡開始閾値の5%を使う
        private float _restThreshold;  // 追跡回転終了閾値、追跡回転開始閾値の5%を使う
        private bool _isPause, _isRest;

        protected override Vector3 GetTrackingPosition(VRCPlayerApi.TrackingDataType trackingTarget)
        {
            if (syncPosition)
            {
                return base.GetTrackingPosition(trackingTarget);
            }

            if (_vROnly && !_localPlayer.IsUserInVR())
            {
                return base.GetTrackingPosition(trackingTarget);
            }

            return GetFollowPosition(_localPlayer.GetTrackingData(trackingPoint).position);
        }

        protected override Quaternion GetTrackingRotation(VRCPlayerApi.TrackingDataType trackingTarget)
        {
            if (syncRotation)
            {
                return base.GetTrackingRotation(trackingTarget);
            }
            if (_vROnly && !_localPlayer.IsUserInVR())
            {
                return base.GetTrackingRotation(trackingTarget);
            }

            return GetFollowRotation(_localPlayer.GetTrackingData(trackingPoint).rotation);
        }


        // 位置の遅延追従
        private Vector3 GetFollowPosition(Vector3 targetPosition)
        {
            var pos = transform.position;

            // moveThresholdの5%を閾値に使う
            _pauseThreshold = moveThreshold * 0.05f;

            // 相対距離を算出
            var distance = (pos - targetPosition).sqrMagnitude;

            // 閾値判定
            if (distance >= moveThreshold * moveThreshold)
            {
                _isPause = false;
            }
            else if (distance < _pauseThreshold * _pauseThreshold)
            {
                _isPause = true;
            }

            if (distance > distanceRange * distanceRange)
            {
                // 離散距離の制限
                pos = Vector3.MoveTowards(targetPosition, pos, Mathf.Abs(distanceRange));
            }
            else if (!_isPause)
            {
                // 位置を遅延追従
                pos = Vector3.Lerp(pos, targetPosition, followMoveSpeed);
            }

            return pos;
        }

        // 回転の遅延追従
        private Quaternion GetFollowRotation(Quaternion targetRotation)
        {
            var rot = transform.rotation;

            angleRange = Mathf.Clamp(angleRange, 0.0f, 180.0f);
            rotateThreshold = Mathf.Clamp(rotateThreshold, 0.0f, 180.0f);
            _restThreshold = rotateThreshold * 0.05f; // RotateThresholdの5%を閾値に使う

            // 相対角度を算出
            var angle = Quaternion.Angle(rot, targetRotation);

            // 閾値判定
            if (angle >= rotateThreshold)
            {
                _isRest = false;
            }
            else if (angle < _restThreshold)
            {
                _isRest = true;
            }

            // 軸毎に遅延しない追従を追加処理
            if (lockRoll) { rot = Quaternion.LookRotation(rot * Vector3.forward, targetRotation * Vector3.up); }
            if (lockPitch) { rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(rot * Vector3.forward, targetRotation * Vector3.up), rot * Vector3.up); }
            if (lockYaw) { rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(rot * Vector3.forward, targetRotation * Vector3.right), rot * Vector3.up); }

            if (!_isRest)
            {
                if (angle > angleRange)
                {
                    // 距離が遠いので加速して回転を遅延追従
                    rot = Quaternion.Lerp(rot, targetRotation, followRotateSpeed * 4);
                }
                else
                {
                    // 回転を遅延追従
                    rot = Quaternion.Lerp(rot, targetRotation, followRotateSpeed);
                }
            }

            return rot;
        }
    }
}
