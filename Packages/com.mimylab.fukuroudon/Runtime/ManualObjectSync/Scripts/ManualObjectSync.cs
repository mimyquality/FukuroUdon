/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using System;
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.Serialization;
    using VRC.SDK3.Components;
    using VRC.SDK3.UdonNetworkCalling;
    using VRC.SDKBase;
    using VRC.Udon;
    using VRC.Udon.Common;
    using VRC.Udon.Common.Enums;

    [Flags]
    public enum ManualObjectSyncSyncStates
    {
        UseGravity = 1 << 0,
        IsKinematic = 1 << 1,
        Pickupable = 1 << 2,
        IsHeld = 1 << 3,
        IsEquipped = 1 << 4,
        IsAttached = 1 << 5,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#manual-objectsync")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Manual ObjectSync")]
    [RequireComponent(typeof(MOSFixer))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ManualObjectSync : UdonSharpBehaviour
    {
        private const float PickupCheckTickRate = 1.0f;

        private const string EventName_Equip = "OnEquip";
        private const string EventName_Unequip = "OnUnequip";
        private const string EventName_Attach = "OnAttach";
        private const string EventName_Detach = "OnDetach";

        [SerializeField]
        private bool _autoSync = true; // 周期的に同期チェックするか

        [Min(2), Tooltip("単位[fps]")]
        public int moveCheckTickRate = 30; // チェック周期(fps依存)

        [Space]
        [FormerlySerializedAs("moveCheckSpace")]
        [SerializeField]
        private Space _relativeTo = Space.World; // 変動確認をする空間

        public float _respawnHightY = -100.0f; // ここより落下したらリスポーンする
        
        [SerializeField]
        private bool _enableSmoothing = false;

        [SerializeField]
        private bool _enableScaleSync = false; // スケールも同期するか

        public Transform attachPoint = null; // アタッチモード時の追従先

        [UdonSynced]
        private Vector3 n_position;

        [UdonSynced]
        private Vector3 n_rotation;

        [UdonSynced]
        private Vector3 n_scale = Vector3.one;

        [UdonSynced]
        private byte n_equipBone;

        [UdonSynced]
        private byte n_states;

        // 同期用パラメーター
        [FieldChangeCallback(nameof(UseGravity))]
        private bool _useGravity = false;

        [FieldChangeCallback(nameof(IsKinematic))]
        private bool _isKinematic = true;

        [FieldChangeCallback(nameof(Pickupable))]
        private bool _pickupable = true;

        private Vector3 _syncPosition = Vector3.zero; // 位置同期用、ピックアップ時はオフセット用
        private Quaternion _syncRotation = Quaternion.identity; // 回転同期用、ピックアップ時はオフセット用
        private Vector3 _syncScale = Vector3.one; // 拡縮同期用
        private HumanBodyBones _equipBone = HumanBodyBones.LastBone; // 装着ボーン、ピックアップ時は掴んでいる手
        private bool _isHeld = false; // ピックアップ中か
        private bool _isEquipped = false; // ボーンに装着中か
        private bool _isAttached = false;　// オブジェクトにアタッチ中か

        // 初期値保存用
        private Vector3 _startPosition = Vector3.zero;
        private Quaternion _startRotation = Quaternion.identity;
        private Vector3 _startScale = Vector3.one;

        // 計算用
        private MOSFixer _fixer;
        private Rigidbody _rigidbody;
        private VRCPickup _pickup;
        private VRCPlayerApi _localPlayer;
        private int _firstMoveCheck = 1;
        private bool _isMoveCheckInterval = false;
        private VRCTweenHandle _tweenMove;
        private float _duration = 1.0f;
        private float _lastReceiveTime;
        private Vector3 _fromPosition;
        private Quaternion _fromRotation;
        private VRCTweenHandle _tweenScale;
        private bool _isPickupCheckInterval;

        [NonSerialized]
        public float _tweenValue;

        private UdonBehaviour[] _eventReceivers = Array.Empty<UdonBehaviour>();

        // Rigidbody.useGravity 同期用
        public bool UseGravity
        {
            get => _useGravity;
            set
            {
                Initialize();
                SetUseGravity(value);
                RequestSerialization();
            }
        }

        private void SetUseGravity(bool value)
        {
            _useGravity = value;
            if (_rigidbody)
            {
                _rigidbody.useGravity = value;
            }
        }

        // Rigidbody.isKinematic 同期用
        public bool IsKinematic
        {
            get => _isKinematic;
            set
            {
                Initialize();
                SetIsKinematic(value);
                RequestSerialization();
            }
        }

        private void SetIsKinematic(bool value)
        {
            _isKinematic = value;
            if (_rigidbody)
            {
                _rigidbody.isKinematic = value || _isAttached || _isEquipped || !Networking.IsOwner(gameObject);
            }
        }

        // VRCPickup.pickupable 同期用
        public bool Pickupable
        {
            get => _pickupable;
            set
            {
                Initialize();
                SetPickupable(value);
                RequestSerialization();
            }
        }

        private void SetPickupable(bool value)
        {
            _pickupable = value;
            if (_pickup)
            {
                _pickup.pickupable = value && !(_pickup.DisallowTheft && _isHeld && !Networking.IsOwner(gameObject));
            }
        }

        public bool IsHeld
        {
            get => _isHeld;
        }


        private void SetIsHeld(bool value)
        {
            if (_isHeld != value)
            {
                _isHeld = value;
                if (value)
                {
                    _isEquipped = false;
                    _isAttached = false;
                }

                if (_pickup)
                {
                    SetPickupable(_pickupable);
                }

                if (_rigidbody)
                {
                    SetIsKinematic(_isKinematic);
                }

                ToggleFixer();
            }
        }

        public bool IsEquipped
        {
            get => _isEquipped;
        }

        private void SetIsEquipped(bool value)
        {
            if (_isEquipped != value)
            {
                _isEquipped = value;
                if (value)
                {
                    if (_pickup && _pickup.IsHeld)
                    {
                        _pickup.Drop();
                    }

                    _isAttached = false;
                }

                if (_rigidbody)
                {
                    SetIsKinematic(_isKinematic);
                }

                ToggleFixer();

                if (value)
                {
                    SendAdditionalEvent(EventName_Equip);
                }
                else
                {
                    SendAdditionalEvent(EventName_Unequip);
                }
            }
        }

        public bool IsAttached
        {
            get => _isAttached;
        }

        private void SetIsAttached(bool value)
        {
            if (_isAttached != value)
            {
                _isAttached = value;
                if (value)
                {
                    if (_pickup && _pickup.IsHeld)
                    {
                        _pickup.Drop();
                    }

                    _isEquipped = false;
                }

                if (_rigidbody)
                {
                    SetIsKinematic(_isKinematic);
                }

                ToggleFixer();

                if (value)
                {
                    SendAdditionalEvent(EventName_Attach);
                }
                else
                {
                    SendAdditionalEvent(EventName_Detach);
                }
            }
        }

        private Vector3 SyncPosition
        {
            set
            {
                _syncPosition = value;
                _fixer.syncPosition = value;
            }
        }

        private Quaternion SyncRotation
        {
            set
            {
                _syncRotation = value;
                _fixer.syncRotation = value;
            }
        }

        private HumanBodyBones EquipBone
        {
            set
            {
                _equipBone = value;
                _fixer.equipBone = value;
            }
        }

        // ピックアップしてる方の手
        public VRC_Pickup.PickupHand PickupHand
        {
            get
            {
                if (_isHeld)
                {
                    if (_equipBone == HumanBodyBones.RightHand) return VRC_Pickup.PickupHand.Right;
                    if (_equipBone == HumanBodyBones.LeftHand) return VRC_Pickup.PickupHand.Left;
                }

                return VRC_Pickup.PickupHand.None;
            }
        }

        /******************************
         Core Event
         ******************************/
        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            _fixer = GetComponent<MOSFixer>();
            _rigidbody = GetComponent<Rigidbody>();
            _pickup = GetComponent<VRCPickup>();
            _localPlayer = Networking.LocalPlayer;

            _startPosition = GetPosition();
            _startRotation = GetRotation();
            _startScale = transform.localScale;

            moveCheckTickRate = Mathf.Max(moveCheckTickRate, 2);
            _firstMoveCheck = moveCheckTickRate + Mathf.Abs(GetInstanceID()) % moveCheckTickRate;

            _fixer.enabled = false;

            if (_rigidbody)
            {
                SetUseGravity(_rigidbody.useGravity);
                SetIsKinematic(_rigidbody.isKinematic);
            }

            if (_pickup)
            {
                SetPickupable(_pickup.pickupable);
            }

            if (_syncPosition.Equals(Vector3.zero)
                && _syncRotation.Equals(Quaternion.identity)
                && _syncScale.Equals(Vector3.one))
            {
                // _sync 系が全部初期値なら Initialize 時点では同期されてきてないと見なして初期化
                _syncPosition = _startPosition;
                _syncRotation = _startRotation;
                _syncScale = _startScale;
            }

            _tweenMove = VRCTween.TweenFloat(0.0f, 1.0f, _duration, this, nameof(_tweenValue),
                nameof(_OnLocationSmoothing),
                VRCTweenEase.Linear).Pause();
            _tweenScale = transform.TweenScale(_startScale, _duration, VRCTweenEase.Linear).Pause();

            var udonBehaviours = GetComponents<UdonBehaviour>();
            var tmp_eventRecveivers = new UdonBehaviour[udonBehaviours.Length];
            var receiverCount = 0;
            for (int i = 0; i < udonBehaviours.Length; i++)
            {
                if (!Utilities.IsValid(udonBehaviours[i])) continue;
                if (udonBehaviours[i] == this) continue;
                if (udonBehaviours[i] == _fixer) continue;

                tmp_eventRecveivers[receiverCount++] = udonBehaviours[i];
            }

            if (receiverCount > 0)
            {
                _eventReceivers = new UdonBehaviour[receiverCount];
                Array.Copy(tmp_eventRecveivers, _eventReceivers, receiverCount);
            }

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            ToggleFixer();

            BeginCheckObjectMove();
        }

        private void OnDisable()
        {
            ToggleFixer();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            Initialize();

            BeginCheckObjectMove();

            // 他人が Owner 化＝ピックアップを奪われた
            if (_pickup && !player.isLocal)
            {
                _pickup.Drop();
            }

            // 新 Owner が改めて同期するものとして、いったん強制リセット
            if (_isHeld)
            {
                SetIsHeld(false);
                SyncLocation();
            }

            if (_isEquipped)
            {
                SetIsEquipped(false);
                SyncLocation();
            }
        }

        public override void OnPreSerialization()
        {
            n_position = _syncPosition;
            n_rotation = _syncRotation.eulerAngles;
            n_scale = _syncScale;

            var intCast = (int)_equipBone;
            n_equipBone = (byte)intCast;

            n_states = 0;
            if (_useGravity) n_states += (int)ManualObjectSyncSyncStates.UseGravity;
            if (_isKinematic) n_states += (int)ManualObjectSyncSyncStates.IsKinematic;
            if (_pickupable) n_states += (int)ManualObjectSyncSyncStates.Pickupable;
            if (_isHeld) n_states += (int)ManualObjectSyncSyncStates.IsHeld;
            if (_isEquipped) n_states += (int)ManualObjectSyncSyncStates.IsEquipped;
            if (_isAttached) n_states += (int)ManualObjectSyncSyncStates.IsAttached;
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            Initialize();

            SyncPosition = n_position;
            SyncRotation = Quaternion.Euler(n_rotation);
            _syncScale = n_scale;

            EquipBone = (HumanBodyBones)Mathf.Clamp(n_equipBone, 0, (int)HumanBodyBones.LastBone);

            bool isHeld = (n_states & (int)ManualObjectSyncSyncStates.IsHeld) > 0;
            if (result.isFromStorage && isHeld)
            {
                // ピックアップ中に退室していたと見做す
                SetIsHeld(false);
                _syncPosition = _startPosition;
                _syncRotation = _startRotation;
            }
            else
            {
                SetIsHeld(isHeld);
            }

            SetUseGravity((n_states & (int)ManualObjectSyncSyncStates.UseGravity) > 0);
            SetIsKinematic((n_states & (int)ManualObjectSyncSyncStates.IsKinematic) > 0);
            SetPickupable((n_states & (int)ManualObjectSyncSyncStates.Pickupable) > 0);
            SetIsEquipped((n_states & (int)ManualObjectSyncSyncStates.IsEquipped) > 0);
            SetIsAttached((n_states & (int)ManualObjectSyncSyncStates.IsAttached) > 0);

            _duration = Mathf.Clamp(result.receiveTime - _lastReceiveTime, 0.1f, 1.0f);
            _lastReceiveTime = result.receiveTime;

            ApplySyncToLocation();

            if (_enableScaleSync && transform.localScale != _syncScale)
            {
                if (_enableSmoothing)
                {
                    _tweenScale.SetDuration(_duration).ChangeEndValue(_syncScale, true).Restart();
                }
                else
                {
                    transform.localScale = _syncScale;
                }
            }
        }

        private void ApplySyncToLocation()
        {
            if (_isHeld) return;
            if (_isEquipped) return;
            if (_isAttached) return;

            if (_enableSmoothing)
            {
                _fromPosition = GetPosition();
                _fromRotation = GetRotation();

                _tweenMove.SetDuration(_duration).Restart();
                return;
            }

            SetLocation(_syncPosition, _syncRotation);
        }

        public void _OnLocationSmoothing()
        {
            Vector3 position = Vector3.Lerp(_fromPosition, _syncPosition, _tweenValue);
            Quaternion rotation = Quaternion.Slerp(_fromRotation, _syncRotation, _tweenValue);

            SetLocation(position, rotation);
        }

        public override void OnPickup()
        {
            Networking.SetOwner(_localPlayer, gameObject);

            SetIsHeld(true);
            BeginCheckPickupOffset();
            RequestSerialization();
        }

        public override void OnDrop()
        {
            SetIsHeld(false);
            SyncLocation();
            RequestSerialization();
        }

        /******************************
         public Method
         ******************************/
        public void Equip(HumanBodyBones targetBone)
        {
            Equip(targetBone, Vector3.zero, Quaternion.identity);
        }

        public void Equip(HumanBodyBones targetBone, Vector3 offsetPosition, Quaternion offsetRotation)
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            SetIsEquipped(true);
            SyncPosition = offsetPosition;
            SyncRotation = offsetRotation;
            EquipBone = targetBone;

            RequestSerialization();
        }

        public void Unequip()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            SetIsEquipped(false);
            SyncLocation();

            RequestSerialization();
        }

        public void Attach()
        {
            Attach(Vector3.zero, Quaternion.identity);
        }

        public void Attach(Vector3 offsetPosition, Quaternion offsetRotation)
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            SetIsAttached(true);
            SyncPosition = offsetPosition;
            SyncRotation = offsetRotation;

            RequestSerialization();
        }

        public void Detach()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            SetIsAttached(false);
            SyncLocation();

            RequestSerialization();
        }

        /******************************
         Network Method
        ******************************/
        [NetworkCallable]
        public void ManualSync()
        {
            if (!Networking.IsOwner(gameObject)) return;

            if (!(GetPosition() == _syncPosition && GetRotation() == _syncRotation))
            {
                SyncLocation();
            }

            if (_enableScaleSync && transform.localScale != _syncScale)
            {
                SyncScale();
            }

            RequestSerialization();
        }

        [NetworkCallable(1)]
        public void Respawn()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            if (_pickup) _pickup.Drop();
            SetIsEquipped(false);
            SetIsAttached(false);

            if (_rigidbody)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.position = _startPosition;
                _rigidbody.rotation = _startRotation;
            }
            else
            {
                transform.SetPositionAndRotation(_startPosition, _startRotation);
            }

            SyncLocation();
        }

        [NetworkCallable(1)]
        public void ResetScale()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            transform.localScale = _startScale;
            _syncScale = _startScale;

            RequestSerialization();
        }

        [NetworkCallable(1)]
        public void CallEquip(int targetBone)
        {
            if (targetBone < 0) return;
            if (targetBone >= (int)HumanBodyBones.LastBone) return;

            Equip((HumanBodyBones)targetBone);
        }

        [NetworkCallable(1)]
        public void CallEquipOffset(int targetBone, Vector3 offsetPosition, Quaternion offsetRotation)
        {
            if (targetBone < 0) return;
            if (targetBone >= (int)HumanBodyBones.LastBone) return;

            Equip((HumanBodyBones)targetBone, offsetPosition, offsetRotation);
        }

        [NetworkCallable(1)]
        public void CallUnequip()
        {
            Unequip();
        }

        [NetworkCallable(1)]
        public void CallAttach()
        {
            Attach();
        }

        [NetworkCallable(1)]
        public void CallAttachOffset(Vector3 offsetPosition, Quaternion offsetRotation)
        {
            Attach(offsetPosition, offsetRotation);
        }

        [NetworkCallable(1)]
        public void CallDetach()
        {
            Detach();
        }

        /******************************
         Owner Method
        ******************************/
        private void BeginCheckObjectMove()
        {
            if (!_autoSync) return;
            if (!isActiveAndEnabled) return;
            if (_isMoveCheckInterval) return;
            if (!Networking.IsOwner(gameObject)) return;

            _isMoveCheckInterval = true;
            SendCustomEventDelayedFrames(nameof(_AutoCheckObjectMove), _firstMoveCheck, EventTiming.LateUpdate);
        }

        public void _AutoCheckObjectMove()
        {
            _isMoveCheckInterval = false;

            if (!_autoSync) return;
            if (!isActiveAndEnabled) return;
            if (!Networking.IsOwner(gameObject)) return;

            if (!_isHeld && !_isEquipped && !_isAttached)
            {
                if (transform.position.y <= _respawnHightY)
                {
                    Respawn();
                }
                else if (!(GetPosition() == _syncPosition && GetRotation() == _syncRotation))
                {
                    SyncLocation();
                }
            }

            if (_enableScaleSync && transform.localScale != _syncScale)
            {
                SyncScale();
            }

            _isMoveCheckInterval = true;
            SendCustomEventDelayedFrames(nameof(_AutoCheckObjectMove), moveCheckTickRate, EventTiming.LateUpdate);
        }

        private void SyncLocation()
        {
            if (_isHeld) return;
            if (_isEquipped) return;
            if (_isAttached) return;

            _syncPosition = GetPosition();
            _syncRotation = GetRotation();

            RequestSerialization();
        }

        private void SyncScale()
        {
            _syncScale = transform.localScale;

            RequestSerialization();
        }

        private void BeginCheckPickupOffset()
        {
            CheckPickupOffset();

            if (!_isPickupCheckInterval)
            {
                SendCustomEventDelayedSeconds(nameof(_LoopCheckPickupOffset), 0.2f, EventTiming.PostLateUpdate);
            }
        }

        public void _LoopCheckPickupOffset()
        {
            _isPickupCheckInterval = false;

            if (!_pickup.IsHeld) return;
            if (!Networking.IsOwner(gameObject)) return;

            CheckPickupOffset();

            _isPickupCheckInterval = true;
            SendCustomEventDelayedSeconds(nameof(_LoopCheckPickupOffset), PickupCheckTickRate, EventTiming.PostLateUpdate);
        }

        private void CheckPickupOffset()
        {
            var pickupHandBone = _pickup.currentHand == VRC_Pickup.PickupHand.Left
                ? HumanBodyBones.LeftHand
                : HumanBodyBones.RightHand;
            if (_equipBone != pickupHandBone)
            {
                EquipBone = pickupHandBone;
                RequestSerialization();
            }

            if (!_localPlayer.IsUserInVR())
            {
                pickupHandBone = HumanBodyBones.Head;
            }

            Vector3 handPosition = _localPlayer.GetBonePosition(pickupHandBone);
            Quaternion handRotation = _localPlayer.GetBoneRotation(pickupHandBone);
            if (handPosition.Equals(Vector3.zero) && handRotation.Equals(Quaternion.identity))
            {
                // ボーンが無い＝ジェネリックアバター
                handPosition = _localPlayer.GetPosition();
                handRotation = _localPlayer.GetRotation();
            }

            Vector3 offsetPosition = Quaternion.Inverse(handRotation) * (_rigidbody.position - handPosition);
            Quaternion offsetRotation = Quaternion.Inverse(handRotation) * _rigidbody.rotation;
            if (!(offsetPosition == _syncPosition && offsetRotation == _syncRotation))
            {
                _syncPosition = offsetPosition;
                _syncRotation = offsetRotation;

                RequestSerialization();
            }
        }

        /******************************
         private Method
        ******************************/
        private Vector3 GetPosition()
        {
            if (_rigidbody)
            {
                return _rigidbody.position;
            }

            if (_relativeTo == Space.Self)
            {
                return transform.localPosition;
            }

            return transform.position;
        }

        private Quaternion GetRotation()
        {
            if (_rigidbody)
            {
                return _rigidbody.rotation;
            }

            if (_relativeTo == Space.Self)
            {
                return transform.localRotation;
            }

            return transform.rotation;
        }

        private void SetLocation(Vector3 position, Quaternion rotation)
        {
            if (_rigidbody)
            {
                _rigidbody.Move(position, rotation);
            }
            else if (_relativeTo == Space.Self)
            {
                transform.SetLocalPositionAndRotation(position, rotation);
            }
            else
            {
                transform.SetPositionAndRotation(position, rotation);
            }
        }

        private void ToggleFixer()
        {
            _fixer.IsHeld = _isHeld;
            _fixer.IsEquipped = _isEquipped;
            _fixer.IsAttached = _isAttached;
            _fixer.enabled = enabled && (_isHeld || _isEquipped || _isAttached);

            if (_fixer.enabled)
            {
                _tweenMove.Pause();
            }
        }

        private void SendAdditionalEvent(string eventName)
        {
            for (int i = 0; i < _eventReceivers.Length; i++)
            {
                if (!Utilities.IsValid(_eventReceivers[i])) continue;

                _eventReceivers[i].SendCustomEvent(eventName);
            }
        }
    }
}