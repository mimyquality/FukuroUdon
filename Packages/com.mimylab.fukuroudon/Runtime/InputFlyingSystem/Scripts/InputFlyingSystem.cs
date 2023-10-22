/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace MimyLab
{
    [AddComponentMenu("Fukuro Udon/Input Flying System")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputFlyingSystem : UdonSharpBehaviour
    {
        [Header("General Settings")]
        [SerializeField]
        [FieldChangeCallback(nameof(EnableFlight))]
        bool _enableFlight = true;
        public bool EnableFlight    // 飛行システムを有効化する
        {
            get => _enableFlight;
            set
            {
                _enableFlight = value;
                if (!value) Fly(false);
            }
        }

        public bool flipInput = false;  // 上下入力を反転する

        [Range(0.0f, 10.0f)]
        public float flightSpeed = 6.0f;    // 飛行速度
        [Range(0.0f, 10.0f)]
        public float flightGravity = 0.1f;    // 飛行中の重力
        [Range(0.0f, 10.0f)]
        public float dampTime = 0.5f;   // 飛行速度の減衰時間

        [Header("Input config for VR")]
        [Range(0.0f, 0.5f)]
        public float deadZone = 0.05f;   // 不感帯、入力がこれ以下なら無効

        [Header("Input config for desktop")]
        [SerializeField]
        KeyCode riseKeyCode = KeyCode.E;    // 上昇
        [SerializeField]
        KeyCode fallKeyCode = KeyCode.Q;    // 下降

        // 計算用
        VRCPlayerApi _lPlayer = null;
        float _defaultGravity, _elapsedTime;
        bool _isFly, _dampInput;
        Vector3 _inputDirection = Vector3.zero;
        Vector3 _direction, _velocity, _dampVelocity;

        void Start()
        {
            _lPlayer = Networking.LocalPlayer;
        }

        void Update()
        {
            if (!Utilities.IsValid(_lPlayer)) { return; }

            // 非VRのみキーボード入力受付
            if (!_lPlayer.IsUserInVR()) { InputKeyboard(); }

            // 着地したら移動制御を返す
            if (_isFly && _lPlayer.IsPlayerGrounded())
            {
                Fly(false);
            }
        }

        void FixedUpdate()
        {
            if (!Utilities.IsValid(_lPlayer)) { return; }

            // 飛行処理
            if (EnableFlight && _isFly)
            {
                Flight();
            }
        }

        /******************************
         Input処理
        ******************************/
        void InputKeyboard()
        {
            if (Input.GetKeyDown(fallKeyCode))
            {
                // 下降
                _inputDirection.y = (flipInput) ? 1.0f : -1.0f;
            }
            if (Input.GetKeyDown(riseKeyCode))
            {
                // 上昇
                _inputDirection.y = (flipInput) ? -1.0f : 1.0f;
            }
            if (Input.GetKeyUp(fallKeyCode) && !Input.GetKey(riseKeyCode)
             || Input.GetKeyUp(riseKeyCode) && !Input.GetKey(fallKeyCode))
            {
                // 上昇中でも下降中でも無くなったら停止
                _inputDirection.y = 0.0f;
            }
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            _inputDirection.z = (Mathf.Abs(value) >= deadZone) ? value : 0.0f;
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            _inputDirection.x = (Mathf.Abs(value) >= deadZone) ? value : 0.0f;
        }

        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            // 非VRは無効
            if (!_lPlayer.IsUserInVR()) { return; }

            _inputDirection.y = (Mathf.Abs(value) >= deadZone) ? (flipInput) ? -value : value : 0.0f;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            // 飛行モードへの移行判定
            if (!EnableFlight) { return; }
            if (!Utilities.IsValid(_lPlayer)) { return; }

            if (_isFly)
            {
                // 飛行モード中ならジャンプ中上昇
                _inputDirection.y = (value) ? 1.0f : 0.0f;
            }
            else if (value && !_lPlayer.IsPlayerGrounded())
            {
                // 空中でジャンプしたら飛行モードに移行、移動制御を奪う
                Fly(true);
            }
        }

        /******************************
         FlySystem処理
        ******************************/
        void Fly(bool mode)
        {
            _isFly = mode;
            _lPlayer.Immobilize(mode);
            if (mode)
            {
                _defaultGravity = _lPlayer.GetGravityStrength();
                _lPlayer.SetGravityStrength(flightGravity);

                // 飛行計算用変数の初期化
                _dampInput = false;
                _velocity = Vector3.zero;
                _elapsedTime = 0.0f;
            }
            else
            {
                _lPlayer.SetGravityStrength(_defaultGravity);
            }
        }

        void Flight()
        {
            // インプットの正規化
            _direction = Vector3.ClampMagnitude(_inputDirection, 1.0f);

            if (_direction.sqrMagnitude > 0)
            {
                if (!_dampInput)
                {
                    // 速度減衰フラグ書き戻し
                    _dampInput = true;

                    // 昇降中は重力を切る
                    _lPlayer.SetGravityStrength(0.0f);
                }

                _elapsedTime = 0.0f;
                _velocity = _lPlayer.GetRotation() * _direction * flightSpeed;
                _lPlayer.SetVelocity(_velocity);
            }
            else if (_dampInput)
            {
                // 最後に入力された速度から徐々に減衰
                _elapsedTime = Mathf.Clamp01(_elapsedTime + Time.deltaTime);
                _dampVelocity = Vector3.Lerp(_velocity, Vector3.zero, _elapsedTime / dampTime);
                _lPlayer.SetVelocity(_dampVelocity);

                if (_elapsedTime >= 1.0f)
                {
                    // 速度減衰終わり
                    _dampInput = false;

                    // 入力が無くなったので飛行中重力に戻す
                    _lPlayer.SetGravityStrength(flightGravity);
                }
            }
        }
    }
}
