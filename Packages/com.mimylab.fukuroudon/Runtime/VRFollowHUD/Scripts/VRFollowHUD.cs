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
        Transform selfTf;
        Vector3 selfPos, targetPos;
        Quaternion selfRot, targetRot;
        float distance, angle;
        VRCPlayerApi lPlayer;
        bool isVR, isPause, isRest;

        public void TrackingHead() { trackingPoint = VRCPlayerApi.TrackingDataType.Head; }
        public void TrackingLeftHand() { trackingPoint = VRCPlayerApi.TrackingDataType.LeftHand; }
        public void TrackingRightHand() { trackingPoint = VRCPlayerApi.TrackingDataType.RightHand; }
        public void TrackingOrigin() { trackingPoint = VRCPlayerApi.TrackingDataType.Origin; }

        void OnEnable()
        {
            selfTf = transform;
            if (Utilities.IsValid(Networking.LocalPlayer))
            {
                lPlayer = Networking.LocalPlayer;

                // 初期位置にリセット
                selfTf.position = lPlayer.GetTrackingData(trackingPoint).position;
                selfTf.rotation = lPlayer.GetTrackingData(trackingPoint).rotation;

                // VRか判定
                isVR = lPlayer.IsUserInVR();
            }
        }

        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(lPlayer)) { return; }

            targetPos = lPlayer.GetTrackingData(trackingPoint).position;
            targetRot = lPlayer.GetTrackingData(trackingPoint).rotation;

            if (vROnly && !isVR)
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
            selfTf.SetPositionAndRotation(selfPos, selfRot);
        }

        // 位置同期
        void SyncPosition()
        {
            selfPos = targetPos;
        }

        // 回転同期
        void SyncRotation()
        {
            selfRot = targetRot;
        }

        // 位置の遅延追従
        void FollowPosition()
        {
            // moveThresholdの5%を閾値に使う
            pauseThreshold = moveThreshold * 0.05f;

            // 相対距離を算出
            distance = (selfPos - targetPos).sqrMagnitude;

            // 閾値判定
            if (distance >= moveThreshold * moveThreshold)
            {
                isPause = false;
            }
            else if (distance < pauseThreshold * pauseThreshold)
            {
                isPause = true;
            }

            if (distance > distanceRange * distanceRange)
            {
                // 離散距離の制限
                selfPos = Vector3.MoveTowards(targetPos, selfPos, Mathf.Abs(distanceRange));
            }
            else if (!isPause)
            {
                // 位置を遅延追従
                selfPos = Vector3.Lerp(selfPos, targetPos, followMoveSpeed);
            }
        }

        // 回転の遅延追従
        void FollowRotation()
        {
            angleRange = Mathf.Clamp(angleRange, 0.0f, 180.0f);
            RotateThreshold = Mathf.Clamp(RotateThreshold, 0.0f, 180.0f);
            restThreshold = RotateThreshold * 0.05f; // angleThresholdの5%を閾値に使う

            // 相対角度を算出
            angle = Quaternion.Angle(selfRot, targetRot);

            // 閾値判定
            if (angle >= RotateThreshold)
            {
                isRest = false;
            }
            else if (angle < restThreshold)
            {
                isRest = true;
            }

            // 軸毎に遅延しない追従を追加処理
            if (lockRoll) { selfRot = Quaternion.LookRotation(selfRot * Vector3.forward, targetRot * Vector3.up); }
            if (lockPitch) { selfRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(selfRot * Vector3.forward, targetRot * Vector3.up), selfRot * Vector3.up); }
            if (lockYaw) { selfRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(selfRot * Vector3.forward, targetRot * Vector3.right), selfRot * Vector3.up); }

            if (!isRest)
            {
                if (angle > angleRange)
                {
                    // 距離が遠いので加速して回転を遅延追従
                    selfRot = Quaternion.Lerp(selfRot, targetRot, followRotateSpeed * 4);
                }
                else
                {
                    // 回転を遅延追従
                    selfRot = Quaternion.Lerp(selfRot, targetRot, followRotateSpeed);
                }
            }
        }
    }
}
