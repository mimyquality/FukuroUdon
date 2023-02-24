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
        // 追従先オブジェクト
        [SerializeField]
        Transform target;

        [Header("Constraint")]
        // 追従させる軸の有効化(グローバル)    
        [SerializeField]
        bool enableX = true;
        [SerializeField]
        bool enableY = true;
        [SerializeField]
        bool enableZ = true;

        [Header("X Axis Limit Setting")]

        // X軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        float minX = float.NegativeInfinity;
        [SerializeField]
        Transform[] activateWhenReachMinX;
        [SerializeField]
        Transform[] inactivateWhenReachMinX;
        [SerializeField]
        float maxX = float.PositiveInfinity;
        [SerializeField]
        Transform[] activateWhenReachMaxX;
        [SerializeField]
        Transform[] inactivateWhenReachMaxX;

        [Header("Y Axis Limit Setting")]

        // Y軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        float minY = float.NegativeInfinity;
        [SerializeField]
        Transform[] activateWhenReachMinY;
        [SerializeField]
        Transform[] inactivateWhenReachMinY;
        [SerializeField]
        float maxY = float.PositiveInfinity;
        [SerializeField]
        Transform[] activateWhenReachMaxY;
        [SerializeField]
        Transform[] inactivateWhenReachMaxY;

        [Header("Z Axis Limit Setting")]

        // Z軸の制限範囲(ローカル)と末端到達時のアクション
        [SerializeField]
        float minZ = float.NegativeInfinity;
        [SerializeField]
        Transform[] activateWhenReachMinZ;
        [SerializeField]
        Transform[] inactivateWhenReachMinZ;
        [SerializeField]
        float maxZ = float.PositiveInfinity;
        [SerializeField]
        Transform[] activateWhenReachMaxZ;
        [SerializeField]
        Transform[] inactivateWhenReachMaxZ;

        // 計算用
        Vector3 offset;     // 追従先とのDistance詰め用
        Vector3 fixPosition;
        float localX, localY, localZ;
        bool reachMinX = false, reachMaxX = false;
        bool reachMinY = false, reachMaxY = false;
        bool reachMinZ = false, reachMaxZ = false;

        void Start()
        {
            offset = target.position - this.transform.position;
        }

        void Update()
        {
            // 各軸ごとにtargetに追従させる
            fixPosition = this.transform.position;
            if (enableX)
            {
                fixPosition.x = target.position.x - offset.x;
            }
            if (enableY)
            {
                fixPosition.y = target.position.y - offset.y;
            }
            if (enableZ)
            {
                fixPosition.z = target.position.z - offset.z;
            }
            this.transform.position = fixPosition;

            // ローカル座標で可動域を超えていたらClampする
            localX = Mathf.Clamp(this.transform.localPosition.x, minX, maxX);
            localY = Mathf.Clamp(this.transform.localPosition.y, minY, maxY);
            localZ = Mathf.Clamp(this.transform.localPosition.z, minZ, maxZ);
            this.transform.localPosition = new Vector3(localX, localY, localZ);

            // minX到達時の処理
            if (this.transform.localPosition.x <= minX + 0.0001f)
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
            if (this.transform.localPosition.x >= maxX - 0.0001f)
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
            if (this.transform.localPosition.y <= minY + 0.0001f)
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
            if (this.transform.localPosition.y >= maxY - 0.0001f)
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
            if (this.transform.localPosition.z <= minZ + 0.0001f)
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
            if (this.transform.localPosition.z >= maxZ - 0.0001f)
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

        void ToggleActiveMinX(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMinX.Length; i++)
            {
                if (activateWhenReachMinX[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachMinX[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachMinX[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachMinX.Length; j++)
            {
                if (inactivateWhenReachMinX[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachMinX[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachMinX[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActiveMaxX(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMaxX.Length; i++)
            {
                if (activateWhenReachMaxX[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachMaxX[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachMaxX[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachMaxX.Length; j++)
            {
                if (inactivateWhenReachMaxX[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachMaxX[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachMaxX[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActiveMinY(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMinY.Length; i++)
            {
                if (activateWhenReachMinY[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachMinY[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachMinY[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachMinY.Length; j++)
            {
                if (inactivateWhenReachMinY[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachMinY[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachMinY[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActiveMaxY(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMaxY.Length; i++)
            {
                if (activateWhenReachMaxY[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachMaxY[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachMaxY[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachMaxY.Length; j++)
            {
                if (inactivateWhenReachMaxY[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachMaxY[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachMaxY[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActiveMinZ(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMinZ.Length; i++)
            {
                if (activateWhenReachMinZ[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachMinZ[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachMinZ[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachMinZ.Length; j++)
            {
                if (inactivateWhenReachMinZ[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachMinZ[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachMinZ[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }

        void ToggleActiveMaxZ(bool isReach)
        {
            for (int i = 0; i < activateWhenReachMaxZ.Length; i++)
            {
                if (activateWhenReachMaxZ[i] == null) { continue; }
                OcclusionPortal op = activateWhenReachMaxZ[i].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    activateWhenReachMaxZ[i].gameObject.SetActive(isReach);
                }
                else
                {
                    op.open = isReach;
                }
            }
            for (int j = 0; j < inactivateWhenReachMaxZ.Length; j++)
            {
                if (inactivateWhenReachMaxZ[j] == null) { continue; }
                OcclusionPortal op = inactivateWhenReachMaxZ[j].gameObject.GetComponent<OcclusionPortal>();
                if (op == null)
                {
                    inactivateWhenReachMaxZ[j].gameObject.SetActive(!isReach);
                }
                else
                {
                    op.open = !isReach;
                }
            }
        }
    }
}