/*
Copyright (c) 2021 Mimy Quality
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
    public class LimitedLookConstraint : UdonSharpBehaviour
    {
        [Header("Source")]
        [SerializeField]
        Transform target;   // 追従先オブジェクト

        [Header("Constraint")]
        // 追従させる軸の有効化
        [SerializeField]
        bool enablePitch = false;

        [Header("Yaw Limit Setting")]
        // Y軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        [Range(180f, 0.0f)]
        float yawRight = 180f;
        [SerializeField]
        Transform[] activateWhenReachYawRight;
        [SerializeField]
        Transform[] inactivateWhenReachYawRight;
        [SerializeField]
        [Range(0.0f, 180f)]
        float yawLeft = 180f;
        [SerializeField]
        Transform[] activateWhenReachYawLeft;
        [SerializeField]
        Transform[] inactivateWhenReachYawLeft;

        [Header("Pitch Limit Setting")]
        // X軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        [Range(180f, 0.0f)]
        float pitchDown = 90f;
        [SerializeField]
        Transform[] activateWhenReachPitchDown;
        [SerializeField]
        Transform[] inactivateWhenReachPitchDown;
        [SerializeField]
        [Range(0.0f, 180f)]
        float pitchUp = 90f;
        [SerializeField]
        Transform[] activateWhenReachPitchUp;
        [SerializeField]
        Transform[] inactivateWhenReachPitchUp;

        // 計算用
        Transform localAxis;    // 基準とするローカル座標系
        Vector3 baseTarget, currentTarget, currentYawTarget, thisLocalPos;    // targetから算出した計算用座標(ローカル)
        Vector3 baseForward, currentForward, currentYawForward, dir;   // targetから算出した計算用ベクトル(ローカル)
        float yawAngle, pitchAngle;     // 制限角度計算用

        // オブジェクトのアクティブ切り替え用
        bool reachYawRight = false, reachYawLeft = false, reachPitchDown = false, reachPitchUp = false;

        void Start()
        {
            localAxis = this.transform.parent;

            // 基準ベクトル計算
            baseTarget = localAxis.InverseTransformPoint(target.position);
            baseTarget.y = (enablePitch) ? baseTarget.y : this.transform.localPosition.y;
            baseForward = baseTarget - this.transform.localPosition;

            // バリデーション
            yawLeft = Mathf.Clamp(yawLeft, 0.0f, 180f);
            yawRight = Mathf.Clamp(yawRight, 0.0f, 180f);
            pitchDown = Mathf.Clamp(pitchDown, 0.0f, 180f);
            pitchUp = Mathf.Clamp(pitchUp, 0.0f, 180f);
        }
        void Update()
        {
            // 使い回すので変数に格納する
            thisLocalPos = this.transform.localPosition;

            // ターゲットの現在方向ベクトル計算
            currentTarget = localAxis.InverseTransformPoint(target.position);
            currentTarget.y = (enablePitch) ? currentTarget.y : thisLocalPos.y;
            currentForward = currentTarget - thisLocalPos;

            // ターゲットのYaw方向ベクトル計算
            currentYawTarget = currentTarget;
            currentYawTarget.y = baseTarget.y;
            currentYawForward = currentYawTarget - thisLocalPos;

            // Yawの角度算出と可動域の制限
            dir = Vector3.Cross(baseForward, currentYawForward);
            yawAngle = Vector3.Angle(baseForward, currentYawForward) * (dir.y < 0 ? -1 : 1);
            yawAngle = Mathf.Clamp(yawAngle, -yawLeft, yawRight);
            // 計算結果を元に回転を反映
            this.transform.localRotation = Quaternion.AngleAxis(yawAngle, Vector3.up) * Quaternion.LookRotation(baseForward);


            // Pitchの可動も有効な場合は追加でPitch回転させる
            if (enablePitch)
            {
                // Pitchの角度算出と可動域の制限
                // dirは途中計算用なので使い回している
                dir = currentForward - currentYawForward;
                pitchAngle = Vector3.Angle(currentYawForward, currentForward) * (dir.y < 0 ? 1 : -1);
                pitchAngle = Mathf.Clamp(pitchAngle, -pitchUp, pitchDown);
                // 計算結果を元に回転を反映
                this.transform.localRotation *= Quaternion.AngleAxis(pitchAngle, Vector3.right);

                // PitchDown到達時の処理
                if (pitchAngle >= pitchDown - 0.01f)
                {
                    if (!reachPitchDown)
                    {
                        reachPitchDown = true;
                        ToggleActivePitchDown(reachPitchDown);
                    }
                }
                else
                {
                    if (reachPitchDown)
                    {
                        reachPitchDown = false;
                        ToggleActivePitchDown(reachPitchDown);
                    }
                }

                // PitchUp到達時の処理
                if (pitchAngle <= -pitchUp + 0.01f)
                {
                    if (!reachPitchUp)
                    {
                        reachPitchUp = true;
                        ToggleActivePitchUp(reachPitchUp);
                    }
                }
                else
                {
                    if (reachPitchUp)
                    {
                        reachPitchUp = false;
                        ToggleActivePitchUp(reachPitchUp);
                    }
                }
            }

            // YawRight到達時の処理
            if (yawAngle >= yawRight - 0.01f)
            {
                if (!reachYawRight)
                {
                    reachYawRight = true;
                    ToggleActiveYawRight(reachYawRight);
                }
            }
            else
            {
                if (reachYawRight)
                {
                    reachYawRight = false;
                    ToggleActiveYawRight(reachYawRight);
                }
            }

            // YawLeft到達時の処理
            if (yawAngle <= -yawLeft + 0.01f)
            {
                if (!reachYawLeft)
                {
                    reachYawLeft = true;
                    ToggleActiveYawLeft(reachYawLeft);
                }
            }
            else
            {
                if (reachYawLeft)
                {
                    reachYawLeft = false;
                    ToggleActiveYawLeft(reachYawLeft);
                }
            }
        }

        void ToggleActiveYawRight(bool isReach)
        {
            for (int i = 0; i < activateWhenReachYawRight.Length; i++)
            {
                if (activateWhenReachYawRight[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachYawRight[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachYawRight[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachYawRight.Length; j++)
            {
                if (inactivateWhenReachYawRight[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachYawRight[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachYawRight[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActiveYawLeft(bool isReach)
        {
            for (int i = 0; i < activateWhenReachYawLeft.Length; i++)
            {
                if (activateWhenReachYawLeft[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachYawLeft[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachYawLeft[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachYawLeft.Length; j++)
            {
                if (inactivateWhenReachYawLeft[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachYawLeft[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachYawLeft[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActivePitchDown(bool isReach)
        {
            for (int i = 0; i < activateWhenReachPitchDown.Length; i++)
            {
                if (activateWhenReachPitchDown[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachPitchDown[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachPitchDown[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachPitchDown.Length; j++)
            {
                if (inactivateWhenReachPitchDown[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachPitchDown[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachPitchDown[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActivePitchUp(bool isReach)
        {
            for (int i = 0; i < activateWhenReachPitchUp.Length; i++)
            {
                if (activateWhenReachPitchUp[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachPitchUp[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachPitchUp[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachPitchUp.Length; j++)
            {
                if (inactivateWhenReachPitchUp[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachPitchUp[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachPitchUp[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }
    }
}