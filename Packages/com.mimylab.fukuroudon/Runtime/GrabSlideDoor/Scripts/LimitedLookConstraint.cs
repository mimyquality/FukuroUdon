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

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limited Constraint/Limited LookAt Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitedLookConstraint : UdonSharpBehaviour
    {
        private const float AngleEpsilon = 0.01f;

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

        // キャッシュ用
        private OcclusionPortal[] _openPortalWhenReachYawRight;
        private OcclusionPortal[] _closePortalWhenReachYawRight;
        private OcclusionPortal[] _openPortalWhenReachYawLeft;
        private OcclusionPortal[] _closePortalWhenReachYawLeft;
        private OcclusionPortal[] _openPortalWhenReachPitchDown;
        private OcclusionPortal[] _closePortalWhenReachPitchDown;
        private OcclusionPortal[] _openPortalWhenReachPitchUp;
        private OcclusionPortal[] _closePortalWhenReachPitchUp;

        // 計算用
        private Transform _localAxis;    // 基準とするローカル座標系
        private Vector3 _baseTarget, _currentTarget, _currentYawTarget, _thisLocalPos;    // targetから算出した計算用座標(ローカル)
        private Vector3 _baseForward, _currentForward, _currentYawForward, _sign;   // targetから算出した計算用ベクトル(ローカル)
        private float _yawAngle, _pitchAngle;     // 制限角度計算用

        // オブジェクトのアクティブ切り替え用
        private bool _reachYawRight = false;
        private bool ReachYawRight
        {
            get => _reachYawRight;
            set
            {
                if (_reachYawRight == value) { return; }
                _reachYawRight = value;

                ToggleActive(activateWhenReachYawRight, value);
                ToggleOpen(_openPortalWhenReachYawRight, value);

                ToggleActive(inactivateWhenReachYawRight, !value);
                ToggleOpen(_closePortalWhenReachYawRight, !value);
            }
        }

        private bool _reachYawLeft = false;
        private bool ReachYawLeft
        {
            get => _reachYawLeft;
            set
            {
                if (_reachYawLeft == value) { return; }
                _reachYawLeft = value;

                ToggleActive(activateWhenReachYawLeft, value);
                ToggleOpen(_openPortalWhenReachYawLeft, value);

                ToggleActive(inactivateWhenReachYawLeft, !value);
                ToggleOpen(_closePortalWhenReachYawLeft, !value);
            }
        }

        private bool _reachPitchDown = false;
        private bool ReachPitchDown
        {
            get => _reachPitchDown;
            set
            {
                if (_reachPitchDown == value) { return; }
                _reachPitchDown = value;

                ToggleActive(activateWhenReachPitchDown, value);
                ToggleOpen(_openPortalWhenReachPitchDown, value);

                ToggleActive(inactivateWhenReachPitchDown, !value);
                ToggleOpen(_closePortalWhenReachPitchDown, !value);
            }
        }

        private bool _reachPitchUp = false;
        private bool ReachPitchUp
        {
            get => _reachPitchUp;
            set
            {
                if (_reachPitchUp == value) { return; }
                _reachPitchUp = value;

                ToggleActive(activateWhenReachPitchUp, value);
                ToggleOpen(_openPortalWhenReachPitchUp, value);

                ToggleActive(inactivateWhenReachPitchUp, !value);
                ToggleOpen(_closePortalWhenReachPitchUp, !value);
            }
        }

        private void Start()
        {
            // キャッシュ
            _localAxis = this.transform.parent;

            _openPortalWhenReachYawRight = GetOcclusionPortals(ref activateWhenReachYawRight);
            _closePortalWhenReachYawRight = GetOcclusionPortals(ref inactivateWhenReachYawRight);
            _openPortalWhenReachYawLeft = GetOcclusionPortals(ref activateWhenReachYawLeft);
            _closePortalWhenReachYawLeft = GetOcclusionPortals(ref inactivateWhenReachYawLeft);

            _openPortalWhenReachPitchDown = GetOcclusionPortals(ref activateWhenReachPitchDown);
            _closePortalWhenReachPitchDown = GetOcclusionPortals(ref inactivateWhenReachPitchDown);
            _openPortalWhenReachPitchUp = GetOcclusionPortals(ref activateWhenReachPitchUp);
            _closePortalWhenReachPitchUp = GetOcclusionPortals(ref inactivateWhenReachPitchUp);

            // 基準ベクトル計算
            _baseTarget = _localAxis.InverseTransformPoint(target.position);
            _baseTarget.y = (enablePitch) ? _baseTarget.y : this.transform.localPosition.y;
            _baseForward = _baseTarget - this.transform.localPosition;

            // バリデーション
            yawLeft = Mathf.Clamp(yawLeft, 0.0f, 180f);
            yawRight = Mathf.Clamp(yawRight, 0.0f, 180f);
            pitchDown = Mathf.Clamp(pitchDown, 0.0f, 180f);
            pitchUp = Mathf.Clamp(pitchUp, 0.0f, 180f);
        }
        private void Update()
        {
            // 使い回すので変数に格納する
            _thisLocalPos = this.transform.localPosition;

            // ターゲットの現在方向ベクトル計算
            _currentTarget = _localAxis.InverseTransformPoint(target.position);
            _currentTarget.y = (enablePitch) ? _currentTarget.y : _thisLocalPos.y;
            _currentForward = _currentTarget - _thisLocalPos;

            // ターゲットのYaw方向ベクトル計算
            _currentYawTarget = _currentTarget;
            _currentYawTarget.y = _baseTarget.y;
            _currentYawForward = _currentYawTarget - _thisLocalPos;

            // Yawの角度算出と可動域の制限
            _sign = Vector3.Cross(_baseForward, _currentYawForward);
            _yawAngle = Vector3.Angle(_baseForward, _currentYawForward) * (_sign.y < 0.0f ? -1.0f : 1.0f);
            _yawAngle = Mathf.Clamp(_yawAngle, -yawLeft, yawRight);
            // 計算結果を元に回転を反映
            this.transform.localRotation = Quaternion.AngleAxis(_yawAngle, Vector3.up) * Quaternion.LookRotation(_baseForward);

            ReachYawRight = (_yawAngle >= yawRight - AngleEpsilon);
            ReachYawLeft = (_yawAngle <= -yawLeft + AngleEpsilon);

            // Pitchの可動も有効な場合は追加でPitch回転させる
            if (enablePitch)
            {
                // Pitchの角度算出と可動域の制限
                _sign = _currentForward - _currentYawForward;
                _pitchAngle = Vector3.Angle(_currentYawForward, _currentForward) * (_sign.y > 0.0f ? -1.0f : 1.0f);
                _pitchAngle = Mathf.Clamp(_pitchAngle, -pitchUp, pitchDown);
                // 計算結果を元に回転を反映
                this.transform.localRotation *= Quaternion.AngleAxis(_pitchAngle, Vector3.right);

                ReachPitchDown = (_pitchAngle >= pitchDown - AngleEpsilon);
                ReachPitchUp = (_pitchAngle <= -pitchUp + AngleEpsilon);
            }
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
