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
    using VRC.Udon.Common.Enums;

#if UNITY_EDITOR
    using UdonSharpEditor;
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif

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
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ManualObjectSync : UdonSharpBehaviour
    {
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
        private Space _relativeTo = Space.World; // 変動確認をローカル座標系でするか

        [SerializeField]
        private bool _enableSmoothing = false;

        [SerializeField]
        private bool _enableScaleSync = false; // スケールも同期するか

        [Header("Option Settings")]
        public Transform attachPoint = null; // アタッチモード時の追従先

        [HideInInspector]
        public MOSUpdateManager _updateManager = null;

        [HideInInspector]
        public float _respawnHightY = -100.0f; // ここより落下したらリスポーンする

        private readonly string _UpdateManagerPrefabGUID = "51374f5e01425074ca9cb544fa44007d";

        [UdonSynced]
        private Vector3 n_position, n_rotation, n_scale = Vector3.one;

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
        private Rigidbody _rigidbody = null;
        private VRCPickup _pickup = null;
        private VRCPlayerApi _localPlayer = null;
        private int _firstMoveCheck = 1;
        private bool _isMoveCheckInterval = false;
        private VRCTweenHandle _tweenMove;
        private float _duration = 1.0f;
        private Vector3 _fromPosition;
        private Quaternion _fromRotation;
        private VRCTweenHandle _tweenScale;

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

                _useGravity = value;
                if (_rigidbody)
                {
                    _rigidbody.useGravity = value;
                }

                RequestSerialization();
            }
        }

        // Rigidbody.isKinematic 同期用
        public bool IsKinematic
        {
            get => _isKinematic;
            set
            {
                Initialize();

                _isKinematic = value;
                if (_rigidbody)
                {
                    _rigidbody.isKinematic = _isAttached || _isEquipped || !Networking.IsOwner(gameObject) || value;
                }

                RequestSerialization();
            }
        }

        // VRCPickup.pickupable 同期用
        public bool Pickupable
        {
            get => _pickupable;
            set
            {
                Initialize();

                _pickupable = value;
                if (_pickup)
                {
                    _pickup.pickupable =
                        (!_pickup.DisallowTheft || !_isHeld || Networking.IsOwner(gameObject)) && value;
                }

                RequestSerialization();
            }
        }

        public bool IsHeld
        {
            get => _isHeld;
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

        public bool IsEquipped
        {
            get => _isEquipped;
        }

        public bool IsAttached
        {
            get => _isAttached;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            EditorApplication.delayCall += () =>
            {
                if (this) MakeUpdateManager();
            };
        }

        private void MakeUpdateManager()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            if (PrefabUtility.IsPartOfPrefabAsset(this)) return;
            if (_updateManager) return;

            _updateManager = FindObjectOfType<MOSUpdateManager>();
            if (_updateManager)
            {
                _respawnHightY = _updateManager._respawnHeightY;
                RecordSelf();
                return;
            }

            var prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_UpdateManagerPrefabGUID));
            if (!prefab)
            {
                Debug.LogError("MOSUpdateManager prefab could not be loaded.");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Created MOSUpdateManager prefab");
            Debug.Log("Created MOSUpdateManager prefab");

            instance.GetComponentInChildren<MOSUpdateManager>().SetupAllMOS();
        }

        internal void SetUpdateManager(MOSUpdateManager um)
        {
            _updateManager = um;
        }

        internal void SetRespawnHeightY(float y)
        {
            _respawnHightY = y;
        }

        internal void RecordSelf()
        {
            UdonSharpEditorUtility.CopyProxyToUdon(this);
            EditorUtility.SetDirty(this);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
#endif

        /******************************
         Core Event
         ******************************/
        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            _rigidbody = GetComponent<Rigidbody>();
            _pickup = GetComponent<VRCPickup>();
            _localPlayer = Networking.LocalPlayer;

            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _startScale = transform.localScale;

            moveCheckTickRate = Mathf.Max(moveCheckTickRate, 2);
            _firstMoveCheck = moveCheckTickRate + Mathf.Abs(GetInstanceID()) % moveCheckTickRate;

            if (_rigidbody)
            {
                _useGravity = _rigidbody.useGravity;
                _isKinematic = _rigidbody.isKinematic;
            }

            if (_pickup)
            {
                _pickupable = _pickup.pickupable;
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

            _tweenMove = VRCTween.TweenFloat(0.0f, 1.0f, 1.0f, this, nameof(_tweenValue), nameof(_OnMoveUpdate),
                VRCTweenEase.Linear).Pause();
            _tweenScale = transform.TweenScale(_startScale, 1.0f, VRCTweenEase.Linear).Pause();

            _eventReceivers = GetComponents<UdonBehaviour>();

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            BeginCheckObjectMove();
        }

        private void Start()
        {
            UseGravity = UseGravity;
            IsKinematic = IsKinematic;
            Pickupable = Pickupable;
        }

        public void _OnPostLateUpdate()
        {
            if (AttachToTransform()) return;
            if (EquipBone()) return;
            if (HoldingOther()) return;

            ApplySyncToLocation();

            _updateManager.DisablePostLateUpdate(this);
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
            SetIsHeld(false);

            // 装備は強制パージ
            SetIsEquipped(false);
        }

        public override void OnPreSerialization()
        {
            n_position = _syncPosition;
            n_rotation = _syncRotation.eulerAngles;
            n_scale = _syncScale;

            var intEquipBone = (int)_equipBone;
            n_equipBone = (byte)intEquipBone;

            n_states = 0;
            if (_useGravity) n_states += (int)ManualObjectSyncSyncStates.UseGravity;
            if (_isKinematic) n_states += (int)ManualObjectSyncSyncStates.IsKinematic;
            if (_pickupable) n_states += (int)ManualObjectSyncSyncStates.Pickupable;
            if (_isHeld) n_states += (int)ManualObjectSyncSyncStates.IsHeld;
            if (_isEquipped) n_states += (int)ManualObjectSyncSyncStates.IsEquipped;
            if (_isAttached) n_states += (int)ManualObjectSyncSyncStates.IsAttached;
        }

        public override void OnDeserialization()
        {
            Initialize();

            _syncPosition = n_position;
            _syncRotation = Quaternion.Euler(n_rotation);
            _syncScale = n_scale;

            _equipBone = (HumanBodyBones)Mathf.Clamp(n_equipBone, 0, (int)HumanBodyBones.LastBone);

            _useGravity = (n_states & (int)ManualObjectSyncSyncStates.UseGravity) > 0;
            _isKinematic = (n_states & (int)ManualObjectSyncSyncStates.IsKinematic) > 0;
            _pickupable = (n_states & (int)ManualObjectSyncSyncStates.Pickupable) > 0;
            _isHeld = (n_states & (int)ManualObjectSyncSyncStates.IsHeld) > 0;
            _isEquipped = (n_states & (int)ManualObjectSyncSyncStates.IsEquipped) > 0;
            _isAttached = (n_states & (int)ManualObjectSyncSyncStates.IsAttached) > 0;

            // 同期パラメーターを反映
            UseGravity = _useGravity;
            IsKinematic = _isKinematic;
            Pickupable = _pickupable;
            SetIsHeld(_isHeld);
            SetIsEquipped(_isEquipped);
            SetIsAttached(_isAttached);

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
                if (_rigidbody)
                {
                    _fromPosition = _rigidbody.position;
                    _fromRotation = _rigidbody.rotation;
                }
                else if (_relativeTo == Space.Self)
                {
                    _fromPosition = transform.localPosition;
                    _fromRotation = transform.localRotation;
                }
                else
                {
                    _fromPosition = transform.position;
                    _fromRotation = transform.rotation;
                }

                _tweenMove.SetDuration(_duration).Restart();
                return;
            }

            if (_rigidbody)
            {
                _rigidbody.Move(_syncPosition, _syncRotation);
            }
            else if (_relativeTo == Space.Self)
            {
                transform.SetLocalPositionAndRotation(_syncPosition, _syncRotation);
            }
            else
            {
                transform.SetPositionAndRotation(_syncPosition, _syncRotation);
            }
        }

        public void _OnMoveUpdate()
        {
            Vector3 position = Vector3.Lerp(_fromPosition, _syncPosition, _tweenValue);
            Quaternion rotation = Quaternion.Slerp(_fromRotation, _syncRotation, _tweenValue);

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

        // VRCPickup と Rigidbody がある
        public override void OnPickup()
        {
            Networking.SetOwner(_localPlayer, gameObject);

            SetIsHeld(true);
            SyncPickupOffset();

            RequestSerialization();
        }

        // VRCPickup  とRigidbody がある
        public override void OnDrop()
        {
            SetIsHeld(false);
            SyncLocation();

            RequestSerialization();
        }

        internal void OnEquip()
        {
            SetIsEquipped(true);

            RequestSerialization();

            SendAdditionalEvent(EventName_Equip);
        }

        internal void OnUnequip()
        {
            SetIsEquipped(false);
            SyncLocation();

            RequestSerialization();

            SendAdditionalEvent(EventName_Unequip);
        }

        internal void OnAttach()
        {
            SetIsAttached(true);

            RequestSerialization();

            SendAdditionalEvent(EventName_Attach);
        }

        internal void OnDetach()
        {
            SetIsAttached(false);
            SyncLocation();

            RequestSerialization();

            SendAdditionalEvent(EventName_Detach);
        }

        /******************************
         public Method
         ******************************/
        public void ManualSync()
        {
            if (!Networking.IsOwner(gameObject)) return;

            // ToDo:一通りのオブジェクト状態を見直して同期用変数に書き込む処理

            RequestSerialization();
        }

        public void Equip(HumanBodyBones targetBone)
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            if (_pickup && _pickup.IsHeld)
            {
                _pickup.Drop();
            }

            _equipBone = targetBone;
            Vector3 bonePosition = _localPlayer.GetBonePosition(targetBone);
            Quaternion boneRotation = _localPlayer.GetBoneRotation(targetBone);
            _syncPosition = bonePosition.Equals(Vector3.zero)
                ? Vector3.zero
                : Quaternion.Inverse(boneRotation) * (transform.position - bonePosition);
            _syncRotation = boneRotation.Equals(Quaternion.identity)
                ? Quaternion.identity
                : (Quaternion.Inverse(boneRotation) * transform.rotation);

            RequestSerialization();

            OnEquip();
        }

        public void Unequip()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            OnUnequip();
        }

        public void Attach()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            OnAttach();
        }

        public void Detach()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            OnDetach();
        }

        /******************************
         For SendCustomNetworkEvent Method
         ******************************/
        [NetworkCallable(1)]
        public void Respawn()
        {
            Initialize();

            if (!Networking.IsOwner(gameObject)) return;

            if (_pickup)
            {
                _pickup.Drop();
            }

            Unequip();
            Detach();

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
            if (0 <= targetBone && targetBone < (int)HumanBodyBones.LastBone)
            {
                Equip((HumanBodyBones)targetBone);
            }
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

            if (!_isEquipped && !_isAttached)
            {
                if (_isHeld)
                {
                    SyncPickupOffset();
                }
                else if (transform.position.y <= _respawnHightY)
                {
                    Respawn();
                }
                else if (CheckLocationChanged())
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

        private bool CheckLocationChanged()
        {
            if (!transform.hasChanged) return false;

            if (_rigidbody)
            {
                return !(_rigidbody.position == _syncPosition && _rigidbody.rotation == _syncRotation);
            }

            if (_relativeTo == Space.Self)
            {
                return !(transform.localPosition == _syncPosition && transform.localRotation == _syncRotation);
            }

            return !(transform.position == _syncPosition && transform.rotation == _syncRotation);
        }

        private void SyncLocation()
        {
            transform.hasChanged = false;

            if (_rigidbody)
            {
                _syncPosition = _rigidbody.position;
                _syncRotation = _rigidbody.rotation;
            }
            else if (_relativeTo == Space.Self)
            {
                _syncPosition = transform.localPosition;
                _syncRotation = transform.localRotation;
            }
            else
            {
                _syncPosition = transform.position;
                _syncRotation = transform.rotation;
            }

            RequestSerialization();
        }

        private void SyncScale()
        {
            _syncScale = transform.localScale;

            RequestSerialization();
        }

        // _isHeld なら VRCPickup と Rigidbody が付いている
        private void SyncPickupOffset()
        {
            var pickupHandBone = _pickup.currentHand == VRC_Pickup.PickupHand.Left
                ? HumanBodyBones.LeftHand
                : HumanBodyBones.RightHand;
            if (_equipBone != pickupHandBone)
            {
                _equipBone = pickupHandBone;
                RequestSerialization();
            }

            if (!_localPlayer.IsUserInVR())
            {
                pickupHandBone = HumanBodyBones.Head;
            }

            Vector3 handPosition = _localPlayer.GetBonePosition(pickupHandBone);
            Quaternion handRotation = _localPlayer.GetBoneRotation(pickupHandBone);

            Vector3 offsetPosition = handPosition.Equals(Vector3.zero)
                ? Vector3.zero
                : Quaternion.Inverse(handRotation) * (_rigidbody.position - handPosition);
            Quaternion offsetRotation = handRotation.Equals(Quaternion.identity)
                ? Quaternion.identity
                : (Quaternion.Inverse(handRotation) * _rigidbody.rotation);

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
        // _isHeld なら VRCPickup と Rigidbody が付いている
        private bool HoldingOther()
        {
            if (!_isHeld) return false;

            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            if (!Utilities.IsValid(owner)) return true;

            var pickupHandBone = owner.IsUserInVR()
                ? _equipBone == HumanBodyBones.LeftHand
                    ? HumanBodyBones.LeftHand
                    : HumanBodyBones.RightHand
                : HumanBodyBones.Head;
            Vector3 handPosition = owner.GetBonePosition(pickupHandBone);
            Quaternion handRotation = owner.GetBoneRotation(pickupHandBone);

            if (handPosition.Equals(Vector3.zero) || handRotation.Equals(Quaternion.identity))
            {
                // ボーン情報の代わりにプレイヤー原点からの固定値
                handPosition = new Vector3((_equipBone == HumanBodyBones.LeftHand) ? -0.2f : 0.2f, 1.0f, 0.3f);
                _rigidbody.MovePosition(owner.GetPosition() + (owner.GetRotation() * handPosition));
                _rigidbody.MoveRotation(owner.GetRotation());
            }
            else
            {
                _rigidbody.MovePosition(handPosition + (handRotation * _syncPosition));
                _rigidbody.MoveRotation(handRotation * _syncRotation);
            }

            return true;
        }

        private bool EquipBone()
        {
            if (!_isEquipped) return false;

            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            if (!Utilities.IsValid(owner)) return true;

            Vector3 bonePosition = owner.GetBonePosition(_equipBone);
            Quaternion boneRotation = owner.GetBoneRotation(_equipBone);
            if (bonePosition.Equals(Vector3.zero) || boneRotation.Equals(Quaternion.identity)) return _isEquipped;

            Vector3 equipPosition = bonePosition + (boneRotation * _syncPosition);
            Quaternion equipRotation = boneRotation * _syncRotation;

            transform.SetPositionAndRotation(equipPosition, equipRotation);

            return true;
        }

        private bool AttachToTransform()
        {
            if (!_isAttached) return false;

            transform.SetPositionAndRotation(attachPoint.position, attachPoint.rotation);

            return true;
        }

        private void SetIsHeld(bool value)
        {
            _isHeld = value;
            if (value)
            {
                _isEquipped = false;
                _isAttached = false;
            }

            if (_pickup)
            {
                Pickupable = Pickupable;
            }

            if (_rigidbody)
            {
                IsKinematic = IsKinematic;
            }
        }

        private void SetIsEquipped(bool value)
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
                IsKinematic = IsKinematic;
            }
        }

        private void SetIsAttached(bool value)
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
                IsKinematic = IsKinematic;
            }
        }

        private void SendAdditionalEvent(string eventName)
        {
            for (int i = 0; i < _eventReceivers.Length; i++)
            {
                if (!Utilities.IsValid(_eventReceivers[i])) continue;
                if (_eventReceivers[i] == this) continue;

                _eventReceivers[i].SendCustomEvent(eventName);
            }
        }
    }
}