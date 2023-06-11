/*
Copyright (c) 2021 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
//using VRC.SDKBase;
//using VRC.Udon;

namespace MimyLab
{
    [AddComponentMenu("Fukuro Udon/Limited Constraint/LookAt")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedLookConstraint : UdonSharpBehaviour
    {
        private const float angleEpsilon = 0.01f;

        [Header("Source")]
        [SerializeField]
        private Transform target;   // 追従先オブジェクト

        [Header("Constraint")]
        // 追従させる軸の有効化
        [SerializeField]
        private bool enablePitch = false;

        [Header("Yaw Limit Setting")]
        // Y軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        [Range(180f, 0.0f)]
        private float yawRight = 180f;
        [SerializeField]
        private Transform[] activateWhenReachYawRight;
        [SerializeField]
        private Transform[] inactivateWhenReachYawRight;
        [SerializeField]
        [Range(0.0f, 180f)]
        private float yawLeft = 180f;
        [SerializeField]
        private Transform[] activateWhenReachYawLeft;
        [SerializeField]
        private Transform[] inactivateWhenReachYawLeft;

        [Header("Pitch Limit Setting")]
        // X軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        [Range(180f, 0.0f)]
        private float pitchDown = 90f;
        [SerializeField]
        private Transform[] activateWhenReachPitchDown;
        [SerializeField]
        private Transform[] inactivateWhenReachPitchDown;
        [SerializeField]
        [Range(0.0f, 180f)]
        private float pitchUp = 90f;
        [SerializeField]
        private Transform[] activateWhenReachPitchUp;
        [SerializeField]
        private Transform[] inactivateWhenReachPitchUp;

        // 計算用
        private Transform localAxis;    // 基準とするローカル座標系
        private Vector3 baseTarget, currentTarget, currentYawTarget, thisLocalPos;    // targetから算出した計算用座標(ローカル)
        private Vector3 baseForward, currentForward, currentYawForward, dir;   // targetから算出した計算用ベクトル(ローカル)
        private float yawAngle, pitchAngle;     // 制限角度計算用

        // オブジェクトのアクティブ切り替え用
        private bool reachYawRight = false, reachYawLeft = false, reachPitchDown = false, reachPitchUp = false;

        private void Start()
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
        private void Update()
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
                if (pitchAngle >= pitchDown - angleEpsilon)
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
                if (pitchAngle <= -pitchUp + angleEpsilon)
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
            if (yawAngle >= yawRight - angleEpsilon)
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
            if (yawAngle <= -yawLeft + angleEpsilon)
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

        private void ToggleActiveYawRight(bool isReach)
        {
            for (int i = 0; i < activateWhenReachYawRight.Length; i++)
            {
                if (!activateWhenReachYawRight[i]) { continue; }
                OcclusionPortal op = activateWhenReachYawRight[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachYawRight[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachYawRight.Length; j++)
            {
                if (!inactivateWhenReachYawRight[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachYawRight[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachYawRight[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActiveYawLeft(bool isReach)
        {
            for (int i = 0; i < activateWhenReachYawLeft.Length; i++)
            {
                if (!activateWhenReachYawLeft[i]) { continue; }
                OcclusionPortal op = activateWhenReachYawLeft[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachYawLeft[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachYawLeft.Length; j++)
            {
                if (!inactivateWhenReachYawLeft[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachYawLeft[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachYawLeft[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActivePitchDown(bool isReach)
        {
            for (int i = 0; i < activateWhenReachPitchDown.Length; i++)
            {
                if (!activateWhenReachPitchDown[i]) { continue; }
                OcclusionPortal op = activateWhenReachPitchDown[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachPitchDown[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachPitchDown.Length; j++)
            {
                if (!inactivateWhenReachPitchDown[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachPitchDown[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachPitchDown[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActivePitchUp(bool isReach)
        {
            for (int i = 0; i < activateWhenReachPitchUp.Length; i++)
            {
                if (!activateWhenReachPitchUp[i]) { continue; }
                OcclusionPortal op = activateWhenReachPitchUp[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachPitchUp[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachPitchUp.Length; j++)
            {
                if (!inactivateWhenReachPitchUp[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachPitchUp[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachPitchUp[j].gameObject.SetActive(!isReach);
                }
            }
        }
    }
}
