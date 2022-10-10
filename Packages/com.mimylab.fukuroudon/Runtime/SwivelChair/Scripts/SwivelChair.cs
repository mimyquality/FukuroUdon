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

    [RequireComponent(typeof(VRCStation))]
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
                if (_station)
                {
                    _station.disableStationExit = value;
                    _station.PlayerMobility = (value) ? VRCStation.Mobility.ImmobilizeForVehicle : VRCStation.Mobility.Immobilize;
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
        Vector3 _seatPosition;
        [UdonSynced(UdonSyncMode.None)]
        Quaternion _seatRotation;

        // コンポーネントのキャッシュ用
        VRCStation _station;
        Transform _enterPoint;
        Transform _parent;

        // ローカル処理用
        VRCPlayerApi _lPlayer;
        bool _isSit = false;    // 自分がこの椅子に座っているか判定
        bool _fixShift = false, _lUse = false, _rUse = false;   // インプット渡し用
        float _lookVerticalValue = 0f, _lookHorizontalValue = 0f;   // インプット渡し用
        float _vertical, _sagittal;    // 計算用
        Vector3 _fixValue, _rotateValue;  // 計算用
        Vector3 _savedSeatPosition;  // 座高のローカル保持用
        float _moveVerticalValue = 0f, _moveHorizontalValue = 0f;   // インプット渡し用
        Vector3 _moveDirection;   //移動方向

        void Start()
        {
            // ローカルプレイヤー参照使い回し用
            _lPlayer = Networking.LocalPlayer;

            // VRC StationにEnter Pointがあるか判定
            _station = GetComponent<VRCStation>();
            _enterPoint = (_station.stationEnterPlayerLocation) ? _station.stationEnterPlayerLocation : this.transform;

            // 移動用
            _parent = this.transform.parent;

            // 初期化
            _savedSeatPosition = _enterPoint.localPosition;
            EnableCasterMove = EnableCasterMove;
        }

        void Update()
        {
            // DTモードの時キーボード入力受付            
            if (Utilities.IsValid(_lPlayer) && !_lPlayer.IsUserInVR())
            {
                InputKeyboard();
            }
        }

        void FixedUpdate()
        {
            // 自分が座ってる時だけ入力受付処理
            if (!_isSit) { return; }

            // スイベル有効かつ移動無効なら回転操作
            if (enableSwivel && !EnableCasterMove)
            {
                // 横視点インプットを元に椅子操作
                if (_lookHorizontalValue != 0f)
                {
                    // スイベル回転
                    _rotateValue = _lookHorizontalValue * rotateSpeed * Time.deltaTime * Vector3.up;
                    this.transform.Rotate(_rotateValue, Space.Self);
                    _seatRotation = this.transform.localRotation;

                    RequestSerialization();
                }
            }

            // 調節機能有効なら移動操作
            if (enableFix)
            {
                // 縦視点インプットを元に椅子操作
                if (_lookVerticalValue != 0f)
                {
                    _vertical = _enterPoint.localPosition.y;
                    _sagittal = _enterPoint.localPosition.z;

                    if (fixMode == ChairFixMode.Sagittal)
                    {
                        // 座深調節
                        _sagittal += _lookVerticalValue * fixSpeed * Time.deltaTime;
                    }
                    else if (fixMode == ChairFixMode.Vertical)
                    {
                        // 座高調節
                        _vertical += _lookVerticalValue * fixSpeed * Time.deltaTime;
                    }
                    else // fixMode == FixMode.Hybrid
                    {
                        // シフト中は前後調節
                        if (_fixShift)
                        {
                            _sagittal += _lookVerticalValue * fixSpeed * Time.deltaTime;
                        }
                        else
                        {
                            _vertical += _lookVerticalValue * fixSpeed * Time.deltaTime;
                        }
                    }

                    // 可動域バリデーション
                    _fixValue.x = _enterPoint.localPosition.x;
                    _fixValue.y = Mathf.Clamp(_vertical, minHight, maxHight);
                    _fixValue.z = Mathf.Clamp(_sagittal, maxBack, maxForward);

                    // 座点調節
                    _enterPoint.localPosition = _fixValue;
                    _seatPosition = _fixValue;

                    RequestSerialization();
                }
            }

            // 移動有効なら椅子移動操作
            if (EnableCasterMove)
            {
                // 椅子ごと回転
                if (_lookHorizontalValue != 0f)
                {
                    _rotateValue = _lookHorizontalValue * moveRotateSpeed * Time.deltaTime * Vector3.up;
                    _parent.Rotate(_rotateValue, Space.World);
                }

                // 縦横移動インプットを元に移動量計算
                if (_moveHorizontalValue != 0f || _moveVerticalValue != 0f)
                {
                    _moveDirection = new Vector3(_moveHorizontalValue, 0f, _moveVerticalValue);
                    _moveDirection.Normalize();
                    _moveDirection = moveSpeed * Time.deltaTime * _moveDirection;
                    _parent.Translate(_moveDirection, Space.Self);
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
                _lookVerticalValue = 1.0f;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // ↓
                _lookVerticalValue = -1.0f;
            }
            if (Input.GetKeyUp(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)
             || Input.GetKeyUp(KeyCode.DownArrow) && !Input.GetKey(KeyCode.UpArrow))
            {
                // ↑↓の入力なし
                _lookVerticalValue = 0f;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // →
                _lookHorizontalValue = 1.0f;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // ←
                _lookHorizontalValue = -1.0f;
            }
            if (Input.GetKeyUp(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)
             || Input.GetKeyUp(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                // ←→の入力なし
                _lookHorizontalValue = 0f;
            }

            // 左右Shift
            _fixShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }

        /******************************
         インプット(VRモード)
        ******************************/
        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (!_lPlayer.IsValid() || !_lPlayer.IsUserInVR()) { return; }

            // 上下
            _lookVerticalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            if (!_lPlayer.IsValid() || !_lPlayer.IsUserInVR()) { return; }

            // 左右
            _lookHorizontalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (!_lPlayer.IsValid() || !_lPlayer.IsUserInVR()) { return; }

            if (args.handType == HandType.RIGHT) { _rUse = value; }
            if (args.handType == HandType.LEFT) { _lUse = value; }
            _fixShift = (_rUse || _lUse);
        }

        /******************************
         インプット(共通)
        ******************************/
        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            // 前後
            _moveVerticalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            // 左右
            _moveHorizontalValue = (Mathf.Abs(value) > deadZone) ? value : 0f;
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!_lPlayer.IsValid()) { return; }

            if (EnableCasterMove && _isSit && value)
            {
                _station.ExitStation(_lPlayer);
            }
        }

        /******************************
         Station関連
        ******************************/
        public override void Interact()
        {
            if (!_lPlayer.IsValid()) { return; }
            Networking.SetOwner(_lPlayer, this.gameObject);
            Networking.SetOwner(_lPlayer, _parent.gameObject);
            _station.UseStation(_lPlayer);
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (!player.IsValid() || !player.isLocal) { return; }
            _isSit = true;

            // 座高の書き戻し処理
            _enterPoint.localPosition = _savedSeatPosition;
            _seatPosition = _savedSeatPosition;
            RequestSerialization();
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!player.IsValid() || !player.isLocal) { return; }
            _isSit = false;

            // 調整した座高のローカル保持
            _savedSeatPosition = _seatPosition;
        }

        /******************************
         同期処理
        ******************************/
        public override void OnDeserialization()
        {
            _enterPoint.localPosition = _seatPosition;
            this.transform.localRotation = _seatRotation;
        }
    }
}
