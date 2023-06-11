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
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Position")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedPositionConstraint : UdonSharpBehaviour
    {
        [Header("Source")]
        [SerializeField]
        private Transform target;   // 追従先オブジェクト

        [Header("Constraint")]
        // 追従させる軸の有効化(グローバル)    
        [SerializeField]
        private bool enableX = true;
        [SerializeField]
        private bool enableY = true;
        [SerializeField]
        private bool enableZ = true;

        [Header("X Axis Limit Setting")]
        // X軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        private float minX = float.NegativeInfinity;
        [SerializeField]
        private Transform[] activateWhenReachMinX;
        [SerializeField]
        private Transform[] inactivateWhenReachMinX;
        [SerializeField]
        private float maxX = float.PositiveInfinity;
        [SerializeField]
        private Transform[] activateWhenReachMaxX;
        [SerializeField]
        private Transform[] inactivateWhenReachMaxX;

        [Header("Y Axis Limit Setting")]
        // Y軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        private float minY = float.NegativeInfinity;
        [SerializeField]
        private Transform[] activateWhenReachMinY;
        [SerializeField]
        private Transform[] inactivateWhenReachMinY;
        [SerializeField]
        private float maxY = float.PositiveInfinity;
        [SerializeField]
        private Transform[] activateWhenReachMaxY;
        [SerializeField]
        private Transform[] inactivateWhenReachMaxY;

        [Header("Z Axis Limit Setting")]
        // Z軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        private float minZ = float.NegativeInfinity;
        [SerializeField]
        private Transform[] activateWhenReachMinZ;
        [SerializeField]
        private Transform[] inactivateWhenReachMinZ;
        [SerializeField]
        private float maxZ = float.PositiveInfinity;
        [SerializeField]
        private Transform[] activateWhenReachMaxZ;
        [SerializeField]
        private Transform[] inactivateWhenReachMaxZ;

        // 計算用
        private Vector3 offset;     // 追従先とのDistance詰め用
        private Vector3 fixPosition, targetPosition;
        private float localX, localY, localZ;
        private bool reachMinX = false, reachMaxX = false;
        private bool reachMinY = false, reachMaxY = false;
        private bool reachMinZ = false, reachMaxZ = false;

        private void Start()
        {
            offset = target.position - this.transform.position;
        }

        private void Update()
        {
            // 各軸ごとにtargetに追従させる
            fixPosition = this.transform.position;
            targetPosition = target.position;
            if (enableX)
            {
                fixPosition.x = targetPosition.x - offset.x;
            }
            if (enableY)
            {
                fixPosition.y = targetPosition.y - offset.y;
            }
            if (enableZ)
            {
                fixPosition.z = targetPosition.z - offset.z;
            }

            // ここからローカル座標で計算
            fixPosition = this.transform.parent.InverseTransformPoint(fixPosition);
            localX = Mathf.Clamp(fixPosition.x, minX, maxX);
            localY = Mathf.Clamp(fixPosition.y, minY, maxY);
            localZ = Mathf.Clamp(fixPosition.z, minZ, maxZ);

            this.transform.localPosition = new Vector3(localX, localY, localZ);

            // minX到達時の処理
            if (localX <= minX + Vector3.kEpsilon)
            {
                if (!reachMinX)
                {
                    reachMinX = true;
                    ToggleActiveMinX(reachMinX);
                }
            }
            else
            {
                if (reachMinX)
                {
                    reachMinX = false;
                    ToggleActiveMinX(reachMinX);
                }
            }

            // maxX到達時の処理
            if (localX >= maxX - Vector3.kEpsilon)
            {
                if (!reachMaxX)
                {
                    reachMaxX = true;
                    ToggleActiveMaxX(reachMaxX);
                }
            }
            else
            {
                if (reachMaxX)
                {
                    reachMaxX = false;
                    ToggleActiveMaxX(reachMaxX);
                }
            }

            // minY到達時の処理
            if (localY <= minY + Vector3.kEpsilon)
            {
                if (!reachMinY)
                {
                    reachMinY = true;
                    ToggleActiveMinY(reachMinY);
                }
            }
            else
            {
                if (reachMinY)
                {
                    reachMinY = false;
                    ToggleActiveMinY(reachMinY);
                }
            }

            // maxY到達時の処理
            if (localY >= maxY - Vector3.kEpsilon)
            {
                if (!reachMaxY)
                {
                    reachMaxY = true;
                    ToggleActiveMaxY(reachMaxY);
                }
            }
            else
            {
                if (reachMaxY)
                {
                    reachMaxY = false;
                    ToggleActiveMaxY(reachMaxY);
                }
            }


            // minZ到達時の処理
            if (localZ <= minZ + Vector3.kEpsilon)
            {
                if (!reachMinZ)
                {
                    reachMinZ = true;
                    ToggleActiveMinZ(reachMinZ);

                }
            }
            else
            {
                if (reachMinZ)
                {
                    reachMinZ = false;
                    ToggleActiveMinZ(reachMinZ);
                }
            }

            // maxZ到達時の処理
            if (localZ >= maxZ - Vector3.kEpsilon)
            {
                if (!reachMaxZ)
                {
                    reachMaxZ = true;
                    ToggleActiveMaxZ(reachMaxZ);
                }
            }
            else
            {
                if (reachMaxZ)
                {
                    reachMaxZ = false;
                    ToggleActiveMaxZ(reachMaxZ);
                }
            }
        }

        private void ToggleActiveMinX(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMinX.Length; i++)
            {
                if (!activateWhenReachMinX[i]) { continue; }
                OcclusionPortal op = activateWhenReachMinX[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachMinX[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachMinX.Length; j++)
            {
                if (!inactivateWhenReachMinX[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachMinX[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachMinX[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActiveMaxX(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMaxX.Length; i++)
            {
                if (!activateWhenReachMaxX[i]) { continue; }
                OcclusionPortal op = activateWhenReachMaxX[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachMaxX[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachMaxX.Length; j++)
            {
                if (!inactivateWhenReachMaxX[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachMaxX[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachMaxX[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActiveMinY(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMinY.Length; i++)
            {
                if (!activateWhenReachMinY[i]) { continue; }
                OcclusionPortal op = activateWhenReachMinY[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachMinY[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachMinY.Length; j++)
            {
                if (!inactivateWhenReachMinY[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachMinY[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachMinY[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActiveMaxY(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMaxY.Length; i++)
            {
                if (!activateWhenReachMaxY[i]) { continue; }
                OcclusionPortal op = activateWhenReachMaxY[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachMaxY[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachMaxY.Length; j++)
            {
                if (!inactivateWhenReachMaxY[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachMaxY[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachMaxY[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActiveMinZ(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMinZ.Length; i++)
            {
                if (!activateWhenReachMinZ[i]) { continue; }
                OcclusionPortal op = activateWhenReachMinZ[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachMinZ[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachMinZ.Length; j++)
            {
                if (!inactivateWhenReachMinZ[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachMinZ[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachMinZ[j].gameObject.SetActive(!isReach);
                }
            }
        }

        private void ToggleActiveMaxZ(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMaxZ.Length; i++)
            {
                if (!activateWhenReachMaxZ[i]) { continue; }
                OcclusionPortal op = activateWhenReachMaxZ[i].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = isReach;
                }
                else
                {
                    activateWhenReachMaxZ[i].gameObject.SetActive(isReach);
                }
            }
            for (int j = 0; j < inactivateWhenReachMaxZ.Length; j++)
            {
                if (!inactivateWhenReachMaxZ[j]) { continue; }
                OcclusionPortal op = inactivateWhenReachMaxZ[j].GetComponent<OcclusionPortal>();
                if (op)
                {
                    op.open = !isReach;
                }
                else
                {
                    inactivateWhenReachMaxZ[j].gameObject.SetActive(!isReach);
                }
            }
        }
    }
}
