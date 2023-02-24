/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MimyLab
{
    [AddComponentMenu("Fukuro Udon/VR Follow HUD")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRFollowHUD : UdonSharpBehaviour
    {
        [Header("General Settings")]
        public VRCPlayerApi.TrackingDataType trackingPoint = VRCPlayerApi.TrackingDataType.Head;    // 追跡箇所
        [SerializeField]
        bool vROnly = true; // VRモードでのみ有効

        [Header("Position Settings")]
        public bool syncPosition = true;  // 位置同期
        [Range(0.0f, 1.0f)]
        public float followMoveSpeed = 0.1f;   // 追跡速度
        public float distanceRange = 1.0f; // 最大離散距離
        public float moveThreshold = 0.0f;  // 追跡開始閾値
        float pauseThreshold; // 追跡終了閾値、追跡開始閾値の5%を使う


        [Header("Rotation Settings")]
        public bool syncRotation = false;   // 回転同期
        [Range(0.0f, 1.0f)]
        public float followRotateSpeed = 0.02f;    // 追跡回転速度
        [Range(0.0f, 180.0f)]
        public float angleRange = 60.0f;  // 回転速度の加速閾値
        [Range(0.0f, 180.0f)]
        public float RotateThreshold = 0.0f; // 追跡回転開始閾値
        [Range(0.0f, 180.0f)]
        float restThreshold;  // 追跡回転終了閾値、追跡回転開始閾値の5%を使う
        public bool lockRoll = true, lockPitch = false, lockYaw = false;   // 遅延せず回転同期(軸毎)

        // Updateで使う変数キャッシュ用
        Transform _selfTf;
        Vector3 _selfPos, _targetPos;
        Quaternion _selfRot, _targetRot;
        float _distance, _angle;
        VRCPlayerApi _lPlayer;
        bool _isVR, _isPause, _isRest;

        public void TrackingHead() { trackingPoint = VRCPlayerApi.TrackingDataType.Head; }
        public void TrackingLeftHand() { trackingPoint = VRCPlayerApi.TrackingDataType.LeftHand; }
        public void TrackingRightHand() { trackingPoint = VRCPlayerApi.TrackingDataType.RightHand; }
        public void TrackingOrigin() { trackingPoint = VRCPlayerApi.TrackingDataType.Origin; }

        void OnEnable()
        {
            _selfTf = transform;
            if (Networking.LocalPlayer.IsValid())
            {
                _lPlayer = Networking.LocalPlayer;

                // 初期位置にリセット
                _selfTf.position = _lPlayer.GetTrackingData(trackingPoint).position;
                _selfTf.rotation = _lPlayer.GetTrackingData(trackingPoint).rotation;

                // VRか判定
                _isVR = _lPlayer.IsUserInVR();
            }
        }

        public override void PostLateUpdate()
        {
            if (!_lPlayer.IsValid()) { return; }

            _targetPos = _lPlayer.GetTrackingData(trackingPoint).position;
            _targetRot = _lPlayer.GetTrackingData(trackingPoint).rotation;

            if (vROnly && !_isVR)
            {
                // VR時のみ有効かつDTモードなら
                SyncPosition();
                SyncRotation();
            }
            else
            {
                if (syncPosition)
                {
                    SyncPosition();
                }
                else
                {
                    FollowPosition();
                }

                if (syncRotation)
                {
                    SyncRotation();
                }
                else
                {
                    FollowRotation();
                }
            }
            _selfTf.SetPositionAndRotation(_selfPos, _selfRot);
        }

        // 位置同期
        void SyncPosition()
        {
            _selfPos = _targetPos;
        }

        // 回転同期
        void SyncRotation()
        {
            _selfRot = _targetRot;
        }

        // 位置の遅延追従
        void FollowPosition()
        {
            // moveThresholdの5%を閾値に使う
            pauseThreshold = moveThreshold * 0.05f;

            // 相対距離を算出
            _distance = (_selfPos - _targetPos).sqrMagnitude;

            // 閾値判定
            if (_distance >= moveThreshold * moveThreshold)
            {
                _isPause = false;
            }
            else if (_distance < pauseThreshold * pauseThreshold)
            {
                _isPause = true;
            }

            if (_distance > distanceRange * distanceRange)
            {
                // 離散距離の制限
                _selfPos = Vector3.MoveTowards(_targetPos, _selfPos, Mathf.Abs(distanceRange));
            }
            else if (!_isPause)
            {
                // 位置を遅延追従
                _selfPos = Vector3.Lerp(_selfPos, _targetPos, followMoveSpeed);
            }
        }

        // 回転の遅延追従
        void FollowRotation()
        {
            angleRange = Mathf.Clamp(angleRange, 0.0f, 180.0f);
            RotateThreshold = Mathf.Clamp(RotateThreshold, 0.0f, 180.0f);
            restThreshold = RotateThreshold * 0.05f; // angleThresholdの5%を閾値に使う

            // 相対角度を算出
            _angle = Quaternion.Angle(_selfRot, _targetRot);

            // 閾値判定
            if (_angle >= RotateThreshold)
            {
                _isRest = false;
            }
            else if (_angle < restThreshold)
            {
                _isRest = true;
            }

            // 軸毎に遅延しない追従を追加処理
            if (lockRoll) { _selfRot = Quaternion.LookRotation(_selfRot * Vector3.forward, _targetRot * Vector3.up); }
            if (lockPitch) { _selfRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(_selfRot * Vector3.forward, _targetRot * Vector3.up), _selfRot * Vector3.up); }
            if (lockYaw) { _selfRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(_selfRot * Vector3.forward, _targetRot * Vector3.right), _selfRot * Vector3.up); }

            if (!_isRest)
            {
                if (_angle > angleRange)
                {
                    // 距離が遠いので加速して回転を遅延追従
                    _selfRot = Quaternion.Lerp(_selfRot, _targetRot, followRotateSpeed * 4);
                }
                else
                {
                    // 回転を遅延追従
                    _selfRot = Quaternion.Lerp(_selfRot, _targetRot, followRotateSpeed);
                }
            }
        }
    }
}
