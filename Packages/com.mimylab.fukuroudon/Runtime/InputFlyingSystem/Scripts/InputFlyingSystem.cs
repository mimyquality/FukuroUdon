/*
Copyright (c) 2022 Mimy Quality
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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Input-Flying-System#%E4%BD%BF%E3%81%84%E6%96%B9")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/Input Flying System")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputFlyingSystem : UdonSharpBehaviour
    {
        [Header("General Settings")]
        [SerializeField]
        [FieldChangeCallback(nameof(EnableFlight))]
        private bool _enableFlight = true;
        public bool EnableFlight    // 飛行システムを有効化する
        {
            get => _enableFlight;
            set
            {
                _enableFlight = value;

                if (!value && _isFlying)
                {
                    Fly(false);
                }
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
        private KeyCode riseKeyCode = KeyCode.E;    // 上昇
        [SerializeField]
        private KeyCode fallKeyCode = KeyCode.Q;    // 下降

        // 計算用
        private VRCPlayerApi _localPlayer = null;
        private Vector3 _inputDirection = Vector3.zero;
        private bool _isFlying = false;
        private bool _ignoreGravity = false;
        private float _defaultGravity;
        private Vector3 _velocity;
        private float _elapsedTime = 0.0f;

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
        }

        private void OnDisable()
        {
            if (_isFlying) { Fly(false); }
        }

        private void Update()
        {
            InputKeyboard();

            // 着地したら移動制御を返す
            if (_isFlying && _localPlayer.IsPlayerGrounded())
            {
                Fly(false);
            }
        }

        private void FixedUpdate()
        {
            // 飛行処理
            if (_enableFlight && _isFlying)
            {
                Flight();
            }
        }

        /******************************
         Input処理
        ******************************/
        private void InputKeyboard()
        {
            // 非VRのみキーボード入力受付
            if (_localPlayer.IsUserInVR()) { return; }

            if (Input.GetKeyDown(fallKeyCode))
            {
                // 下降
                _inputDirection.y = flipInput ? 1.0f : -1.0f;
            }
            if (Input.GetKeyDown(riseKeyCode))
            {
                // 上昇
                _inputDirection.y = flipInput ? -1.0f : 1.0f;
            }
            if (Input.GetKeyUp(fallKeyCode))
            {
                if (Input.GetKey(riseKeyCode))
                {
                    // 上昇に戻す
                    _inputDirection.y = flipInput ? -1.0f : 1.0f;
                }
                else
                {
                    // 上昇中でも下降中でも無くなったら停止
                    _inputDirection.y = 0.0f;
                }
            }
            if (Input.GetKeyUp(riseKeyCode))
            {
                if (Input.GetKey(fallKeyCode))
                {
                    // 下降に戻す
                    _inputDirection.y = flipInput ? 1.0f : -1.0f;
                }
                else
                {
                    // 上昇中でも下降中でも無くなったら停止
                    _inputDirection.y = 0.0f;
                }
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
            if (!_localPlayer.IsUserInVR()) { return; }

            _inputDirection.y = (Mathf.Abs(value) >= deadZone) ? flipInput ? -value : value : 0.0f;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!_enableFlight) { return; }

            // 飛行モードへの移行判定
            if (!_isFlying && value && !_localPlayer.IsPlayerGrounded())
            {
                // 空中でジャンプしたら飛行モードに移行、移動制御を奪う
                Fly(true);
            }
        }

        /******************************
         FlySystem処理
        ******************************/
        private void Fly(bool mode)
        {
            _isFlying = mode;
            _localPlayer.Immobilize(mode);
            if (mode)
            {
                _defaultGravity = _localPlayer.GetGravityStrength();
                _localPlayer.SetGravityStrength(flightGravity);

                // 飛行計算用変数の初期化
                _elapsedTime = 0.0f;
                _ignoreGravity = true;
                _velocity = _localPlayer.GetVelocity();
            }
            else
            {
                _localPlayer.SetGravityStrength(_defaultGravity);
            }
        }

        private void Flight()
        {
            // インプットを速度に反映
            var direction = Vector3.ClampMagnitude(_inputDirection, 1.0f);
            if (direction.sqrMagnitude > 0.0f)
            {
                // 昇降中は重力を切る
                if (!_ignoreGravity)
                {
                    _ignoreGravity = true;
                    _localPlayer.SetGravityStrength(0.0f);
                }

                _elapsedTime = 0.0f;
                var rotation = _localPlayer.IsUserInVR() ? _localPlayer.GetRotation() : _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                _velocity = rotation * direction * flightSpeed;
                _localPlayer.SetVelocity(_velocity);

                return;
            }

            if (_ignoreGravity)
            {
                // 最後に入力された速度から徐々に減衰
                if (dampTime > 0.0f && _elapsedTime < dampTime)
                {
                    _elapsedTime += Time.deltaTime;
                    var velocity = Vector3.Lerp(_velocity, Vector3.zero, _elapsedTime / dampTime);
                    _localPlayer.SetVelocity(velocity);

                    return;
                }

                // 入力速度が無くなったので飛行中重力に戻す
                _ignoreGravity = false;
                _localPlayer.SetGravityStrength(flightGravity);
            }
        }
    }
}
