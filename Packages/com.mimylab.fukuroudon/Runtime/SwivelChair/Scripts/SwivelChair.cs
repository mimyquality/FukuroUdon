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
    // 調節機能の切替
    public enum ChairFixMode
    {
        Hybrid,      //0：複合
        Sagittal,    // 1：座深(前後)
        Vertical     // 2:座高(上下)
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SwivelChair : UdonSharpBehaviour
    {
        [Header("Swivel Settings")]
        public bool enableSwivel = true;    // スイベルターン機能の有効化
        [Tooltip("degree/sec")]
        public float rotateSpeed = 60.0f;   // 入力量に対する回転速度の係数

        [Header("Fix Settings")]
        public bool enableFix = true;  // 調節機能の有効化
        [Tooltip("Hybrid: Hight and Sit back\nSagittal: Sit back Only\nVertical:Hight Only")]
        public ChairFixMode fixMode = ChairFixMode.Hybrid;
        [Tooltip("meter/sec")]
        public float fixSpeed = 0.5f;   // 入力量に対する調節速度の係数

        [SerializeField]
        float maxHight = 0.5f, minHight = -0.5f;    // 座高の調節可能範囲

        [SerializeField]
        float maxForward = 0.3f, maxBack = -0.3f;   // 座深の調節可能範囲

        [Header("Caster Move Settings")]
        [FieldChangeCallback(nameof(EnableCasterMove))]
        [SerializeField]
        bool _enableCasterMove = false;   // 移動機能の有効化
        public bool EnableCasterMove
        {
            get => _enableCasterMove;
            set
            {
                _enableCasterMove = value;
                if (station)
                {
                    station.disableStationExit = value;
                    station.PlayerMobility = (value) ? VRCStation.Mobility.ImmobilizeForVehicle : VRCStation.Mobility.Immobilize;
                }
            }
        }
        [Tooltip("meter/sec")]
        public float moveSpeed = 2.0f;  // 入力量に対する移動速度の係数
        [Tooltip("degree/sec")]
        public float moveRotateSpeed = 60.0f;   // 入力量に対する回転速度の係数

        [Header("Input config for VR")]
        [Range(0.0f, 0.5f)]
        public float deadZone = 0.05f;   // 不感帯、入力がこれ以下なら無効

        // 同期用
        [UdonSynced(UdonSyncMode.None)]
        Vector3 seatPosition;
        [UdonSynced(UdonSyncMode.None)]
        Quaternion seatRotation;

        // コンポーネントのキャッシュ用
        VRCStation station;
        Transform enterPoint;
        Transform parent;

        // ローカル処理用
        VRCPlayerApi lPlayer;
        bool isSit = false;    // 自分がこの椅子に座っているか判定
        bool fixShift = false, lUse = false, rUse = false;   // インプット渡し用
        float lookVerticalValue = 0f, lookHorizontalValue = 0f;   // インプット渡し用
        float vertical, sagittal;    // 計算用
        Vector3 fixValue, rotateValue;  // 計算用
        Vector3 savedSeatPosition;  // 座高のローカル保持用
        float moveVerticalValue = 0f, moveHorizontalValue = 0f;   // インプット渡し用
        Vector3 moveDirection;   //移動方向

        void Start()
        {
            // ローカルプレイヤー参照使い回し用
            lPlayer = (Utilities.IsValid(Networking.LocalPlayer)) ? Networking.LocalPlayer : null;

            // VRC StationにEnter Pointがあるか判定
            station = (VRCStation)GetComponent(typeof(VRCStation));
            enterPoint = (station.stationEnterPlayerLocation) ? station.stationEnterPlayerLocation : this.transform;

            // 移動用
            parent = this.transform.parent;

            // 初期化
            savedSeatPosition = enterPoint.localPosition;
            EnableCasterMove = EnableCasterMove;
        }

        void Update()
        {
            // DTモードの時キーボード入力受付            
            if (Utilities.IsValid(lPlayer) && !lPlayer.IsUserInVR())
            {
                InputKeyboard();
            }
        }

        void FixedUpdate()
        {
            // 自分が座ってる時だけ入力受付処理
            if (!isSit) { return; }

            // スイベル有効かつ移動無効なら回転操作
            if (enableSwivel && !EnableCasterMove)
            {
                // 横視点インプットを元に椅子操作
                if (lookHorizontalValue != 0f)
                {
                    // スイベル回転
                    rotateValue = lookHorizontalValue * rotateSpeed * Time.deltaTime * Vector3.up;
                    this.transform.Rotate(rotateValue, Space.Self);
                    seatRotation = this.transform.localRotation;

                    RequestSerialization();
                }
            }

            // 調節機能有効なら移動操作
            if (enableFix)
            {
                // 縦視点インプットを元に椅子操作
                if (lookVerticalValue != 0f)
                {
                    vertical = enterPoint.localPosition.y;
                    sagittal = enterPoint.localPosition.z;

                    if (fixMode == ChairFixMode.Sagittal)
                    {
                        // 座深調節
                        sagittal += lookVerticalValue * fixSpeed * Time.deltaTime;
                    }
                    else if (fixMode == ChairFixMode.Vertical)
                    {
                        // 座高調節
                        vertical += lookVerticalValue * fixSpeed * Time.deltaTime;
                    }
                    else // fixMode == FixMode.Hybrid
                    {
                        // シフト中は前後調節
                        if (fixShift)
                        {
                            sagittal += lookVerticalValue * fixSpeed * Time.deltaTime;
                        }
                        else
                        {
                            vertical += lookVerticalValue * fixSpeed * Time.deltaTime;
                        }
                    }

                    // 可動域バリデーション
                    fixValue.x = enterPoint.localPosition.x;
                    fixValue.y = Mathf.Clamp(vertical, minHight, maxHight);
                    fixValue.z = Mathf.Clamp(sagittal, maxBack, maxForward);

                    // 座点調節
                    enterPoint.localPosition = fixValue;
                    seatPosition = fixValue;

                    RequestSerialization();
                }
            }

            // 移動有効なら椅子移動操作
            if (EnableCasterMove)
            {
                // 椅子ごと回転
                if (lookHorizontalValue != 0f)
                {
                    rotateValue = lookHorizontalValue * moveRotateSpeed * Time.deltaTime * Vector3.up;
                    parent.Rotate(rotateValue, Space.World);
                }

                // 縦横移動インプットを元に移動量計算
                if (moveHorizontalValue != 0f || moveVerticalValue != 0f)
                {
                    moveDirection = new Vector3(moveHorizontalValue, 0f, moveVerticalValue);
                    moveDirection.Normalize();
                    moveDirection = moveSpeed * Time.deltaTime * moveDirection;
                    parent.Translate(moveDirection, Space.Self);
                }
            }
        }

        /******************************
         インプット(DTモード)
        ******************************/
        void InputKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // ↑
                lookVerticalValue = 1.0f;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // ↓
                lookVerticalValue = -1.0f;
            }
            if (Input.GetKeyUp(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)
             || Input.GetKeyUp(KeyCode.DownArrow) && !Input.GetKey(KeyCode.UpArrow))
            {
                // ↑↓の入力なし
                lookVerticalValue = 0f;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // →
                lookHorizontalValue = 1.0f;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // ←
                lookHorizontalValue = -1.0f;
            }
            if (Input.GetKeyUp(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)
             || Input.GetKeyUp(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                // ←→の入力なし
                lookHorizontalValue = 0f;
            }

            // 左右Shift
            fixShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }

        /******************************
         インプット(VRモード)
        ******************************/
        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (!Utilities.IsValid(lPlayer) || !lPlayer.IsUserInVR()) { return; }
            
            // 上下
            lookVerticalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            if (!Utilities.IsValid(lPlayer) || !lPlayer.IsUserInVR()) { return; }
            
            // 左右
            lookHorizontalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (!Utilities.IsValid(lPlayer) || !lPlayer.IsUserInVR()) { return; }

            if (args.handType == HandType.RIGHT) { rUse = value; }
            if (args.handType == HandType.LEFT) { lUse = value; }
            fixShift = (rUse || lUse);
        }

        /******************************
         インプット(共通)
        ******************************/
        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            // 前後
            moveVerticalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            // 左右
            moveHorizontalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!Utilities.IsValid(lPlayer)) { return; }

            if (EnableCasterMove && isSit && value)
            {
                station.ExitStation(lPlayer);
            }
        }

        /******************************
         Station関連
        ******************************/
        public override void Interact()
        {
            if (!Utilities.IsValid(lPlayer)) { return; }
            Networking.SetOwner(lPlayer, this.gameObject);
            Networking.SetOwner(lPlayer, parent.gameObject);
            station.UseStation(lPlayer);
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || !player.isLocal) { return; }
            isSit = true;

            // 座高の書き戻し処理
            enterPoint.localPosition = savedSeatPosition;
            seatPosition = savedSeatPosition;
            RequestSerialization();
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || !player.isLocal) { return; }
            isSit = false;

            // 調整した座高のローカル保持
            savedSeatPosition = seatPosition;
        }

        /******************************
         同期処理
        ******************************/
        public override void OnDeserialization()
        {
            enterPoint.localPosition = seatPosition;
            this.transform.localRotation = seatRotation;
        }
    }
}
