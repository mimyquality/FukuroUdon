/*
Copyright (c) 2021 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grab-SlideDoor#limited-position-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited Position Constraint")]
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

        // キャッシュ用
        OcclusionPortal[] _openPortalWhenReachMinX;
        OcclusionPortal[] _closePortalWhenReachMinX;
        OcclusionPortal[] _openPortalWhenReachMaxX;
        OcclusionPortal[] _closePortalWhenReachMaxX;
        OcclusionPortal[] _openPortalWhenReachMinY;
        OcclusionPortal[] _closePortalWhenReachMinY;
        OcclusionPortal[] _openPortalWhenReachMaxY;
        OcclusionPortal[] _closePortalWhenReachMaxY;
        OcclusionPortal[] _openPortalWhenReachMinZ;
        OcclusionPortal[] _closePortalWhenReachMinZ;
        OcclusionPortal[] _openPortalWhenReachMaxZ;
        OcclusionPortal[] _closePortalWhenReachMaxZ;

        // 計算用
        private Transform _localAxis;    // 基準とするローカル座標系
        private Vector3 _offset;     // 追従先とのDistance詰め用
        private Vector3 _fixPosition, _targetPosition;
        private float _localX, _localY, _localZ;

        // オブジェクトのアクティブ切り替え用
        private bool _reachMinX = false;
        private bool ReachMinX
        {
            get => _reachMinX;
            set
            {
                if (_reachMinX == value) { return; }
                _reachMinX = value;

                ToggleActive(activateWhenReachMinX, value);
                ToggleOpen(_openPortalWhenReachMinX, value);

                ToggleActive(inactivateWhenReachMinX, !value);
                ToggleOpen(_closePortalWhenReachMinX, !value);
            }
        }

        private bool _reachMaxX = false;
        private bool ReachMaxX
        {
            get => _reachMaxX;
            set
            {
                if (_reachMaxX == value) { return; }
                _reachMaxX = value;

                ToggleActive(activateWhenReachMaxX, value);
                ToggleOpen(_openPortalWhenReachMaxX, value);

                ToggleActive(inactivateWhenReachMaxX, !value);
                ToggleOpen(_closePortalWhenReachMaxX, !value);
            }
        }

        private bool _reachMinY = false;
        private bool ReachMinY
        {
            get => _reachMinY;
            set
            {
                if (_reachMinY == value) { return; }
                _reachMinY = value;

                ToggleActive(activateWhenReachMinY, value);
                ToggleOpen(_openPortalWhenReachMinY, value);

                ToggleActive(inactivateWhenReachMinY, !value);
                ToggleOpen(_closePortalWhenReachMinY, !value);
            }
        }

        private bool _reachMaxY = false;
        private bool ReachMaxY
        {
            get => _reachMaxY;
            set
            {
                if (_reachMaxY == value) { return; }
                _reachMaxY = value;

                ToggleActive(activateWhenReachMaxY, value);
                ToggleOpen(_openPortalWhenReachMaxY, value);

                ToggleActive(inactivateWhenReachMaxY, !value);
                ToggleOpen(_closePortalWhenReachMaxY, !value);
            }
        }

        private bool _reachMinZ = false;
        private bool ReachMinZ
        {
            get => _reachMinZ;
            set
            {
                if (_reachMinZ == value) { return; }
                _reachMinZ = value;

                ToggleActive(activateWhenReachMinZ, value);
                ToggleOpen(_openPortalWhenReachMinZ, value);

                ToggleActive(inactivateWhenReachMinZ, !value);
                ToggleOpen(_closePortalWhenReachMinZ, !value);
            }
        }

        private bool _reachMaxZ = false;
        private bool ReachMaxZ
        {
            get => _reachMaxZ;
            set
            {
                if (_reachMaxZ == value) { return; }
                _reachMaxZ = value;

                ToggleActive(activateWhenReachMaxZ, value);
                ToggleOpen(_openPortalWhenReachMaxZ, value);

                ToggleActive(inactivateWhenReachMaxZ, !value);
                ToggleOpen(_closePortalWhenReachMaxZ, !value);
            }
        }

        private void Start()
        {
            // キャッシュ
            _localAxis = this.transform.parent;
            _offset = target.position - this.transform.position;

            _openPortalWhenReachMinX = GetOcclusionPortals(ref activateWhenReachMinX);
            _closePortalWhenReachMinX = GetOcclusionPortals(ref inactivateWhenReachMinX);
            _openPortalWhenReachMaxX = GetOcclusionPortals(ref activateWhenReachMaxX);
            _closePortalWhenReachMaxX = GetOcclusionPortals(ref inactivateWhenReachMaxX);

            _openPortalWhenReachMinY = GetOcclusionPortals(ref activateWhenReachMinY);
            _closePortalWhenReachMinY = GetOcclusionPortals(ref inactivateWhenReachMinY);
            _openPortalWhenReachMaxY = GetOcclusionPortals(ref activateWhenReachMaxY);
            _closePortalWhenReachMaxY = GetOcclusionPortals(ref inactivateWhenReachMaxY);

            _openPortalWhenReachMinZ = GetOcclusionPortals(ref activateWhenReachMinZ);
            _closePortalWhenReachMinZ = GetOcclusionPortals(ref inactivateWhenReachMinZ);
            _openPortalWhenReachMaxZ = GetOcclusionPortals(ref activateWhenReachMaxZ);
            _closePortalWhenReachMaxZ = GetOcclusionPortals(ref inactivateWhenReachMaxZ);
        }

        private void Update()
        {
            // 各軸ごとにtargetに追従させる
            _fixPosition = this.transform.position;
            _targetPosition = target.position;
            if (enableX)
            {
                _fixPosition.x = _targetPosition.x - _offset.x;
            }
            if (enableY)
            {
                _fixPosition.y = _targetPosition.y - _offset.y;
            }
            if (enableZ)
            {
                _fixPosition.z = _targetPosition.z - _offset.z;
            }

            // ここからローカル座標で計算
            _fixPosition = _localAxis.InverseTransformPoint(_fixPosition);
            _localX = Mathf.Clamp(_fixPosition.x, minX, maxX);
            _localY = Mathf.Clamp(_fixPosition.y, minY, maxY);
            _localZ = Mathf.Clamp(_fixPosition.z, minZ, maxZ);

            this.transform.localPosition = new Vector3(_localX, _localY, _localZ);

            ReachMinX = _localX <= minX + Vector3.kEpsilon;
            ReachMaxX = _localX >= maxX - Vector3.kEpsilon;

            ReachMinY = _localY <= minY + Vector3.kEpsilon;
            ReachMaxY = _localY >= maxY - Vector3.kEpsilon;

            ReachMinZ = _localZ <= minZ + Vector3.kEpsilon;
            ReachMaxZ = _localZ >= maxZ - Vector3.kEpsilon;
        }

        private OcclusionPortal[] GetOcclusionPortals(ref Transform[] objectArray)
        {
            var ops = new OcclusionPortal[objectArray.Length];
            for (int i = 0; i < objectArray.Length; i++)
            {
                ops[i] = objectArray[i].gameObject.GetComponent<OcclusionPortal>();
                if (ops[i]) { objectArray[i] = null; }
            }
            return ops;
        }

        private void ToggleActive(Transform[] transformArray, bool value)
        {
            for (int i = 0; i < transformArray.Length; i++)
            {
                if (!transformArray[i]) { continue; }

                transformArray[i].gameObject.SetActive(value);
            }
        }

        private void ToggleOpen(OcclusionPortal[] occlusionPortalArray, bool value)
        {
            for (int i = 0; i < occlusionPortalArray.Length; i++)
            {
                if (!occlusionPortalArray[i]) { continue; }

                occlusionPortalArray[i].open = value;
            }
        }
    }
}
