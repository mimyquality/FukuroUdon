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
        KeyCode riseKeyCode = KeyCode.Q;    // 上昇
        [SerializeField]
        KeyCode fallKeyCode = KeyCode.E;    // 下降

        // 計算用
        VRCPlayerApi lPlayer = null;
        float defaultGravity, elapsedTime;
        bool isFly, dampInput;
        Vector3 inputDirection = Vector3.zero;
        Vector3 direction, velocity, dampVelocity;

        void Start()
        {
            lPlayer = Networking.LocalPlayer;
        }

        void Update()
        {
            if (!Utilities.IsValid(lPlayer)) { return; }

            // DTモードのみキーボード入力受付
            if (!lPlayer.IsUserInVR())
            {
                InputKeyboard();
            }

            // 着地したら移動制御を返す
            if (isFly && lPlayer.IsPlayerGrounded())
            {
                Fly(false);
            }
        }

        void FixedUpdate()
        {
            if (!Utilities.IsValid(lPlayer)) { return; }

            // 飛行処理
            if (EnableFlight && isFly)
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
                inputDirection.y = (flipInput) ? 1.0f : -1.0f;
            }
            if (Input.GetKeyDown(riseKeyCode))
            {
                // 上昇
                inputDirection.y = (flipInput) ? -1.0f : 1.0f;
            }
            if (Input.GetKeyUp(fallKeyCode) && !Input.GetKey(riseKeyCode)
             || Input.GetKeyUp(riseKeyCode) && !Input.GetKey(fallKeyCode))
            {
                // 上昇中でも下降中でも無くなったら停止
                inputDirection.y = 0.0f;
            }
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            inputDirection.z = (Mathf.Abs(value) >= deadZone) ? value : 0.0f;
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            inputDirection.x = (Mathf.Abs(value) >= deadZone) ? value : 0.0f;
        }

        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            // DTモードは無効
            if (!Utilities.IsValid(lPlayer) || !lPlayer.IsUserInVR()) { return; }

            inputDirection.y = (Mathf.Abs(value) >= deadZone) ? (flipInput) ? -value : value : 0.0f;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            // 飛行モードへの移行判定
            if (!EnableFlight) { return; }
            if (!Utilities.IsValid(lPlayer)) { return; }

            if (isFly)
            {
                // 飛行モード中ならジャンプ中上昇
                inputDirection.y = (value) ? 1.0f : 0.0f;
            }
            else if (value && !lPlayer.IsPlayerGrounded())
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
            isFly = mode;
            lPlayer.Immobilize(mode);
            if (mode)
            {
                defaultGravity = lPlayer.GetGravityStrength();
                lPlayer.SetGravityStrength(flightGravity);

                // 飛行計算用変数の初期化
                dampInput = false;
                velocity = Vector3.zero;
                elapsedTime = 0.0f;
            }
            else
            {
                lPlayer.SetGravityStrength(defaultGravity);
            }
        }

        void Flight()
        {
            // インプットの正規化
            direction = Vector3.ClampMagnitude(inputDirection, 1.0f);

            if (direction.sqrMagnitude > 0)
            {
                if (!dampInput)
                {
                    // 速度減衰フラグ書き戻し
                    dampInput = true;

                    // 昇降中は重力を切る
                    lPlayer.SetGravityStrength(0.0f);
                }

                elapsedTime = 0.0f;
                velocity = lPlayer.GetRotation() * direction * flightSpeed;
                lPlayer.SetVelocity(velocity);
            }
            else if (dampInput)
            {
                // 最後に入力された速度から徐々に減衰
                elapsedTime = Mathf.Clamp01(elapsedTime + Time.deltaTime);
                dampVelocity = Vector3.Lerp(velocity, Vector3.zero, elapsedTime / dampTime);
                lPlayer.SetVelocity(dampVelocity);

                if (elapsedTime >= 1.0f)
                {
                    // 速度減衰終わり
                    dampInput = false;

                    // 入力が無くなったので飛行中重力に戻す
                    lPlayer.SetGravityStrength(flightGravity);
                }
            }
        }
    }
}
