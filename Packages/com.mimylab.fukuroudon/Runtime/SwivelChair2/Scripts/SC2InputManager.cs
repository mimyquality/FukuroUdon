﻿/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    using VRC.Udon.Common;
    //using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/SC2 Input Manager")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class SC2InputManager : UdonSharpBehaviour
    {
        internal SC2SeatAdjuster seatAdjuster;
        internal SC2Caster caster;

        [SerializeField]
        [Tooltip("0 = PCVR \n1 = Desktop \n2 = Quest \n3 = Android")]
        private GameObject[] _tooltip = new GameObject[0];

        [Min(0.0f), Tooltip("sec")]
        public float longPushDuration = 0.8f;
        [Min(0.0f), Tooltip("sec")]
        public float doubleTapDuration = 0.2f;

        private Rigidbody _casterRigidbody;
        private Animator[] _tooltipAnimator;
        private SwivelChairPlayerPlatform _platform = default;
        private SwivelChairInputMode _inputMode = SwivelChairInputMode.Vertical;
        private float _turnValue, _prevTurnValue;
        private Vector3 _moveValue, _prevMoveValue;
        private bool _isJump;
        private float _inputJumpInterval;
        private float _inputDoubleJumpInterval;

        // Tooltipアニメーター用
        private int _param_OnStationEnter = Animator.StringToHash("OnStationEnter");
        private int _param_OnModeChange = Animator.StringToHash("OnModeChange");
        private int _param_InputMode = Animator.StringToHash("InputMode");

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

#if UNITY_STANDALONE_WIN
            _platform = Networking.LocalPlayer.IsUserInVR() ? SwivelChairPlayerPlatform.PCVR : SwivelChairPlayerPlatform.Desktop;
#endif
#if UNITY_ANDROID || UNITY_IOS
            _platform = Networking.LocalPlayer.IsUserInVR() ? SwivelChairPlayerPlatform.StandaloneVR : SwivelChairPlayerPlatform.Mobile;
#endif

            if (caster) { _casterRigidbody = caster.GetComponent<Rigidbody>(); }

            _tooltipAnimator = new Animator[_tooltip.Length];
            for (int i = 0; i < _tooltip.Length; i++)
            {
                _tooltipAnimator[i] = _tooltip[i] ? _tooltip[i].GetComponentInChildren<Animator>(true) : null;
            }

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            _turnValue = 0.0f;
            _prevTurnValue = 0.0f;
            _moveValue = Vector3.zero;
            _prevMoveValue = Vector3.zero;
            _isJump = false;
            _inputJumpInterval = 0.0f;
            _inputDoubleJumpInterval = doubleTapDuration;

            for (int i = 0; i < _tooltip.Length; i++)
            {
                if (i == (int)_platform)
                {
                    if (_tooltip[i]) { _tooltip[i].SetActive(true); }
                }
            }
            if (_tooltipAnimator[(int)_platform])
            {
                _tooltipAnimator[(int)_platform].SetTrigger(_param_OnStationEnter);
                ChangeInputMode(_inputMode);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _tooltip.Length; i++)
            {
                if (_tooltip[i]) { _tooltip[i].SetActive(false); }
            }
        }

        private void Update()
        {
            // ジャンプボタン長押し処理
            if (_isJump) { _inputJumpInterval += Time.deltaTime; }
            if (_inputJumpInterval > longPushDuration) { seatAdjuster.Exit(); }

            // ジャンプボタン二度押し処理
            if (!_isJump && _inputDoubleJumpInterval < doubleTapDuration) { _inputDoubleJumpInterval += Time.deltaTime; }

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
             || _platform == SwivelChairPlayerPlatform.StandaloneVR)
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
            if (_inputMode == SwivelChairInputMode.Disable) { return; }

            if (_platform == SwivelChairPlayerPlatform.PCVR
             || _platform == SwivelChairPlayerPlatform.StandaloneVR)
            {
                _turnValue = value;
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            _isJump = value;
            _inputJumpInterval = 0.0f;

            if (value)
            {
                if (_platform == SwivelChairPlayerPlatform.Mobile)
                {
                    if (_inputDoubleJumpInterval < doubleTapDuration) { seatAdjuster.Exit(); }
                }

                return;
            }
            // ここからJumpボタンpull時の処理

            _inputDoubleJumpInterval = 0.0f;

            var tmpInputMode = (SwivelChairInputMode)default;
            switch (_inputMode)
            {
                case SwivelChairInputMode.Disable: tmpInputMode = SwivelChairInputMode.Vertical; break;
                case SwivelChairInputMode.Vertical: tmpInputMode = SwivelChairInputMode.Horizontal; break;
                case SwivelChairInputMode.Horizontal: tmpInputMode = SwivelChairInputMode.CasterMove; break;
                case SwivelChairInputMode.CasterMove: tmpInputMode = SwivelChairInputMode.Disable; break;
            }
            if (tmpInputMode == SwivelChairInputMode.CasterMove && !caster)
            {
                tmpInputMode = SwivelChairInputMode.Disable;
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

            if (_tooltipAnimator[(int)_platform]) { ChangeInputMode(_inputMode); }
        }

        private void ChangeInputMode(SwivelChairInputMode mode)
        {
            _tooltipAnimator[(int)_platform].SetTrigger(_param_OnModeChange);
            _tooltipAnimator[(int)_platform].SetInteger(_param_InputMode, (int)mode);
        }
    }
}
