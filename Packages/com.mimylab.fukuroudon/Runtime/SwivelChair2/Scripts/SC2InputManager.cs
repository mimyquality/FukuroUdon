/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;
    using VRC.SDK3.Rendering;
    using VRC.Udon.Common;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Swivel-Chair-2#sc2-input-manager")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/SC2 Input Manager")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SC2InputManager : UdonSharpBehaviour
    {
        [SerializeField]
        [Tooltip("0 = VR \n1 = Desktop \n2 = Quest \n3 = Mobile")]
        private GameObject[] _tooltip = new GameObject[0];

        [Min(0.0f), Tooltip("sec")]
        public float longPushDuration = 0.8f;
        [Min(0.0f), Tooltip("sec")]
        public float doubleTapDuration = 0.2f;

        [System.NonSerialized]
        public float _exitProgress;

        internal SC2SeatAdjuster _seatAdjuster;
        internal SC2Caster _caster;

        private Rigidbody _casterRigidbody;
        private Animator[] _tooltipAnimator;
        private SwivelChairPlayerPlatform _platform = default;
        private SwivelChairInputMode _inputMode = SwivelChairInputMode.Vertical;
        private float _turnValue, _prevTurnValue;
        private Vector3 _moveValue, _prevMoveValue;
        private VRCTweenHandle _exitProgressHandle;

        // Tooltipアニメーター用
        private int _param_OnStationEnter = Animator.StringToHash("OnStationEnter");
        private int _param_OnModeChange = Animator.StringToHash("OnModeChange");
        private int _param_InputMode = Animator.StringToHash("InputMode");
        private int _param_OnExitStart = Animator.StringToHash("OnExitStart");
        private int _param_ExitProgress = Animator.StringToHash("ExitProgress");

        private VRCCameraSettings _photoCamera;
        private bool _existPhotoCamera = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _platform = Networking.LocalPlayer.IsUserInVR() ? SwivelChairPlayerPlatform.VR : SwivelChairPlayerPlatform.Desktop;
            if (InputManager.GetLastUsedInputMethod() == VRCInputMethod.Touch) { _platform = SwivelChairPlayerPlatform.Mobile; }

            float duration = _platform == SwivelChairPlayerPlatform.Mobile ? doubleTapDuration : longPushDuration;
            _exitProgressHandle = VRCTween.TweenFloat(0.0f, 1.0f, duration, this, nameof(_exitProgress), nameof(_OnExitProgressUpdate), VRCTweenEase.Linear)
                .OnComplete(this, nameof(_OnExitProgressComplete))
                .Pause();

            if (_caster) { _casterRigidbody = _caster.GetComponent<Rigidbody>(); }

            _tooltipAnimator = new Animator[_tooltip.Length];
            for (int i = 0; i < _tooltip.Length; i++)
            {
                _tooltipAnimator[i] = _tooltip[i] ? _tooltip[i].GetComponentInChildren<Animator>(true) : null;
            }

            _photoCamera = VRCCameraSettings.PhotoCamera;
            _existPhotoCamera = Utilities.IsValid(_photoCamera);

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            _turnValue = 0.0f;
            _prevTurnValue = 0.0f;
            _moveValue = Vector3.zero;
            _prevMoveValue = Vector3.zero;

            for (int i = 0; i < _tooltip.Length; i++)
            {
                if (_tooltip[i]) { _tooltip[i].SetActive(i == (int)_platform); }
            }
            if (_tooltipAnimator[(int)_platform])
            {
                _tooltipAnimator[(int)_platform].SetTrigger(_param_OnStationEnter);
            }
            ChangeInputMode(_inputMode);
        }

        private void OnDisable()
        {
            _exitProgressHandle.Goto(0.0f, false);

            for (int i = 0; i < _tooltip.Length; i++)
            {
                if (_tooltip[i]) { _tooltip[i].SetActive(false); }
            }
        }

        private void Update()
        {
            // カメラを出してる間は動かさない
            if (_existPhotoCamera && _photoCamera.Active) { return; }

            // 入力値を各種操作に反映
            if (!(_turnValue == 0.0f && _prevTurnValue == 0.0f))
            {
                if (_inputMode == SwivelChairInputMode.CasterMove)
                {
                    if (!_casterRigidbody)
                    {
                        _caster.Turn(_turnValue);
                    }
                }
                else
                {
                    _seatAdjuster.Revolve(_turnValue);
                }
                _prevTurnValue = _turnValue;
            }

            if (!(_moveValue == Vector3.zero && _prevMoveValue == Vector3.zero))
            {
                if (_inputMode == SwivelChairInputMode.CasterMove)
                {
                    if (!_casterRigidbody)
                    {
                        _caster.Move(_moveValue);
                    }
                }
                else
                {
                    _seatAdjuster.Adjust(_moveValue);
                }
                _prevMoveValue = _moveValue;
            }
        }

        private void FixedUpdate()
        {
            if (!_casterRigidbody) { return; }
            if (_inputMode != SwivelChairInputMode.CasterMove) { return; }

            // カメラを出してる間は動かさない
            if (_existPhotoCamera && _photoCamera.Active) { return; }

            // 入力値を各種操作に反映
            if (!(_turnValue == 0.0f && _prevTurnValue == 0.0f))
            {
                _caster.Turn(_turnValue);
                _prevTurnValue = _turnValue;
            }

            if (!(_moveValue == Vector3.zero && _prevMoveValue == Vector3.zero))
            {
                _caster.Move(_moveValue);
                _prevMoveValue = _moveValue;
            }
        }
        
        private void OnDestroy()
        {
            gameObject.KillAllTweens();
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
            if (_platform == SwivelChairPlayerPlatform.VR)
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

            if (_platform == SwivelChairPlayerPlatform.VR)
            {
                _turnValue = value;
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (value)
            {
                // スマホは長押しできないため、ダブルタップで Exit する
                if (_platform == SwivelChairPlayerPlatform.Mobile && _exitProgressHandle.IsPlaying)
                {
                    _exitProgressHandle.Goto(0.0f, false);
                    _seatAdjuster.Exit();

                    return;
                }

                _exitProgressHandle.Restart();

                if (_tooltipAnimator[(int)_platform])
                {
                    _tooltipAnimator[(int)_platform].SetTrigger(_param_OnExitStart);
                }
            }
            else
            {
                if (_platform != SwivelChairPlayerPlatform.Mobile)
                {
                    _exitProgressHandle.Goto(0.0f, false);
                }

                var tmpInputMode = (SwivelChairInputMode)default;
                switch (_inputMode)
                {
                    case SwivelChairInputMode.Disable: tmpInputMode = SwivelChairInputMode.Vertical; break;
                    case SwivelChairInputMode.Vertical: tmpInputMode = SwivelChairInputMode.Horizontal; break;
                    case SwivelChairInputMode.Horizontal: tmpInputMode = SwivelChairInputMode.CasterMove; break;
                    case SwivelChairInputMode.CasterMove: tmpInputMode = SwivelChairInputMode.Disable; break;
                }
                if (tmpInputMode == SwivelChairInputMode.CasterMove && !_caster)
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

                ChangeInputMode(_inputMode);
            }
        }

        public void _OnExitProgressUpdate()
        {
            if (_tooltipAnimator[(int)_platform])
            {
                _tooltipAnimator[(int)_platform].SetFloat(_param_ExitProgress, _exitProgress);
            }
        }

        public void _OnExitProgressComplete()
        {
            _exitProgressHandle.Goto(0.0f, false);

            if (_platform != SwivelChairPlayerPlatform.Mobile)
            {
                _seatAdjuster.Exit();
            }
        }

        private void ChangeInputMode(SwivelChairInputMode mode)
        {
            if (_tooltipAnimator[(int)_platform])
            {
                _tooltipAnimator[(int)_platform].SetTrigger(_param_OnModeChange);
                _tooltipAnimator[(int)_platform].SetInteger(_param_InputMode, (int)mode);
            }
        }
    }
}
