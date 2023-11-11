/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    using VRC.Udon.Common;
    //using VRC.SDK3.Components;

    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/Input Manager")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class SC2InputManager : UdonSharpBehaviour
    {
        [HideInInspector]
        public SC2SeatAdjuster seatAdjuster;
        [HideInInspector]
        public SC2Caster caster;

        [SerializeField]
        private Animator _tooltipAnimator;

        [Min(0.0f), Tooltip("sec")]
        public float exitLatency = 0.8f;

        private Rigidbody _casterRigidbody;
        private SwivelChairPlayerPlatform _platform = default;
        private SwivelChairInputMode _inputMode = default;
        private float _turnValue, _prevTurnValue;
        private Vector3 _moveValue, _prevMoveValue;
        private bool _isJump = false;
        private float _inputJumpInterval = 0.0f;

        // Tooltipアニメーター用
        private int _param_OnStationEnter = Animator.StringToHash("OnStationEnter");
        private int _param_OnModeVertical = Animator.StringToHash("OnModeVertical");
        private int _param_OnModeHorizontal = Animator.StringToHash("OnModeHorizontal");
        private int _param_OnModeCasterMove = Animator.StringToHash("OnModeCasterMove");

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _platform = (Networking.LocalPlayer.IsUserInVR()) ? SwivelChairPlayerPlatform.PCVR : SwivelChairPlayerPlatform.Desktop;
#if UNITY_ANDROID
            _platform = (Networking.LocalPlayer.IsUserInVR()) ? SwivelChairPlayerPlatform.Quest : SwivelChairPlayerPlatform.Android;
#endif

            if (caster) { _casterRigidbody = caster.GetComponent<Rigidbody>(); }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            _turnValue = 0.0f;
            _prevTurnValue = 0.0f;
            _moveValue = Vector3.zero;
            _prevMoveValue = Vector3.zero;
            _isJump = false;
            _inputJumpInterval = 0.0f;

            if (_tooltipAnimator)
            {
                _tooltipAnimator.gameObject.SetActive(true);
                _tooltipAnimator.SetTrigger(_param_OnStationEnter);
                ChangeInputMode(_inputMode);
            }
        }

        private void OnDisable()
        {
            if (_tooltipAnimator)
            {
                _tooltipAnimator.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // ジャンプボタン長押し処理
            if (_isJump) { _inputJumpInterval += Time.deltaTime; }
            if (_inputJumpInterval > exitLatency) { seatAdjuster.Exit(); }

            // 入力値を各種操作に反映
            if (_turnValue != 0.0f || _prevTurnValue != 0.0f)
            {
                if (_inputMode == SwivelChairInputMode.CasterMove)
                {
                    if (!_casterRigidbody)
                    {
                        caster.Turn(_turnValue);
                    }
                }
                else
                {
                    seatAdjuster.Revolve(_turnValue);
                }
                _prevTurnValue = _turnValue;
            }
            if (_moveValue != Vector3.zero || _prevMoveValue != Vector3.zero)
            {
                if (_inputMode == SwivelChairInputMode.CasterMove)
                {
                    if (!_casterRigidbody)
                    {
                        caster.Move(_moveValue);
                    }
                }
                else
                {
                    seatAdjuster.Adjust(_moveValue);
                }
                _prevMoveValue = _moveValue;
            }
        }

        private void FixedUpdate()
        {
            if (!_casterRigidbody) { return; }
            if (_inputMode != SwivelChairInputMode.CasterMove) { return; }

            // 入力値を各種操作に反映
            if (_turnValue != 0.0f || _prevTurnValue != 0.0f)
            {
                caster.Turn(_turnValue);
                _prevTurnValue = _turnValue;
            }

            if (_moveValue != Vector3.zero || _prevMoveValue != Vector3.zero)
            {
                caster.Move(_moveValue);
                _prevMoveValue = _moveValue;
            }
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            if (_inputMode == SwivelChairInputMode.Vertical)
            {
                _moveValue.y = value;
            }
            if (_inputMode == SwivelChairInputMode.Horizontal
            || _inputMode == SwivelChairInputMode.CasterMove)
            {
                _moveValue.z = value;
            }
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            if (_platform == SwivelChairPlayerPlatform.PCVR
             || _platform == SwivelChairPlayerPlatform.Quest)
            {
                _moveValue.x = value;
                return;
            }

            if (_inputMode == SwivelChairInputMode.Vertical
             || _inputMode == SwivelChairInputMode.CasterMove)
            {
                _turnValue = value;
            }
            if (_inputMode == SwivelChairInputMode.Horizontal)
            {
                _moveValue.x = value;
            }
        }

        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            if (_platform == SwivelChairPlayerPlatform.PCVR
             || _platform == SwivelChairPlayerPlatform.Quest)
            {
                _turnValue = value;
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            _isJump = value;
            _inputJumpInterval = 0.0f;

            if (value) { return; }

            var tmpInputMode = SwivelChairInputMode.Vertical;
            switch (_inputMode)
            {
                case SwivelChairInputMode.Vertical: tmpInputMode = SwivelChairInputMode.Horizontal; break;
                case SwivelChairInputMode.Horizontal: tmpInputMode = SwivelChairInputMode.CasterMove; break;
                case SwivelChairInputMode.CasterMove: tmpInputMode = SwivelChairInputMode.Vertical; break;
            }
            if (tmpInputMode == SwivelChairInputMode.CasterMove && !caster)
            {
                tmpInputMode = SwivelChairInputMode.Vertical;
            }
            // 無効なモードがあればFix


            if (tmpInputMode != _inputMode)
            {
                _turnValue = 0.0f;
                _prevTurnValue = 0.0f;
                _moveValue = Vector3.zero;
                _prevMoveValue = Vector3.zero;

                _inputMode = tmpInputMode;
            }

            if (_tooltipAnimator) { ChangeInputMode(_inputMode); }
        }

        private void ChangeInputMode(SwivelChairInputMode mode)
        {
            switch (mode)
            {
                case SwivelChairInputMode.Vertical: _tooltipAnimator.SetTrigger(_param_OnModeVertical); break;
                case SwivelChairInputMode.Horizontal: _tooltipAnimator.SetTrigger(_param_OnModeHorizontal); break;
                case SwivelChairInputMode.CasterMove: _tooltipAnimator.SetTrigger(_param_OnModeCasterMove); break;
            }
        }
    }
}
