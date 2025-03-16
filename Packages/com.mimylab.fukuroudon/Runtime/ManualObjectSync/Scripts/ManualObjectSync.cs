/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

#pragma warning disable 0414

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;
    //using VRC.Udon;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UdonSharpEditor;
#endif

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Manual ObjectSync")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ManualObjectSync : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("The tick rate depends on the fps of this game object owner's client.")]
        [Min(2)]
        public int moveCheckTickRate = 30;  // 変動確認の周期(fps依存)
        public Space moveCheckSpace = Space.Self;    // 変動確認をローカル座標系でするか

        [Header("Option Settings")]
        public Transform attachPoint = null;   // アタッチモード時の追従先

        [HideInInspector]
        public MOSUpdateManager _updateManager = null;
        [HideInInspector]
        public float _respawnHightY = -100.0f;   // ここより落下したらリスポーンする
        private readonly string _UpdateManagerPrefabGUID = "51374f5e01425074ca9cb544fa44007d";

        [UdonSynced]
        private Vector3 _syncPosition = Vector3.zero; // 位置同期用、ピックアップ時はオフセット用
        [UdonSynced]
        private Quaternion _syncRotation = Quaternion.identity; // 回転同期用、ピックアップ時はオフセット用
        [UdonSynced]
        private Vector3 _syncScale = Vector3.one;    // 拡縮同期用

        [UdonSynced, FieldChangeCallback(nameof(UseGravity))]
        private bool _useGravity = false;
        [UdonSynced, FieldChangeCallback(nameof(IsKinematic))]
        private bool _isKinematic = true;
        [UdonSynced, FieldChangeCallback(nameof(Pickupable))]
        private bool _pickupable = true;
        [UdonSynced, FieldChangeCallback(nameof(IsHeld))]
        private bool _isHeld = false;
        [UdonSynced]
        private byte _equipBone = (byte)VRCPickup.PickupHand.None;

        [UdonSynced, FieldChangeCallback(nameof(IsEquiped))]
        private bool _isEquiped = false;

        [UdonSynced, FieldChangeCallback(nameof(IsAttached))]
        private bool _isAttached = false;

        // 初期値保存用
        private Vector3 _startPosition, _localPosition;
        private Quaternion _startRotation, _localRotation;
        private Vector3 _startScale = Vector3.one, _localScale = Vector3.one;

        // 計算用
        private Rigidbody _rigidbody = null;
        private VRCPickup _pickup = null;
        private VRCPlayerApi _localPlayer;
        private int _firstCheckTiming;
        private bool _reservedInterval = false;
        private bool _syncHasChanged = false;

        public bool UseGravity  // Rigidbody.useGravity同期用
        {
            get => _useGravity;
            set
            {
                Initialize();

                _useGravity = value;
                if (_rigidbody) { _rigidbody.useGravity = value; }
                RequestSerialization();
            }
        }
        public bool IsKinematic // Rigidbody.isKinematic同期用
        {
            get => _isKinematic;
            set
            {
                Initialize();

                _isKinematic = value;
                if (_rigidbody) { _rigidbody.isKinematic = (_isAttached || _isEquiped || !Networking.IsOwner(this.gameObject)) ? true : value; }
                RequestSerialization();
            }
        }

        public bool Pickupable  // VRCPickup.pickupable同期用
        {
            get => _pickupable;
            set
            {
                Initialize();

                _pickupable = value;
                if (_pickup) { _pickup.pickupable = (_pickup.DisallowTheft && _isHeld && !Networking.IsOwner(this.gameObject)) ? false : value; }
                RequestSerialization();
            }
        }
        public bool IsHeld  // ピックアップ中か(自他問わず)
        {
            get => _isHeld;
            private set
            {
                Initialize();

                _isHeld = value;
                if (value)
                {
                    _isEquiped = false;
                    _isAttached = false;
                    _updateManager.EnablePostLateUpdate(this);
                }
                if (_pickup) { Pickupable = Pickupable; }
                if (_rigidbody) { IsKinematic = IsKinematic; }
                RequestSerialization();
            }
        }

        public VRCPickup.PickupHand PickupHand    // ピックアップしてる方の手
        {
            get
            {
                if (_isHeld)
                {
                    if (_equipBone == (byte)HumanBodyBones.LeftHand) { return VRCPickup.PickupHand.Left; }
                    if (_equipBone == (byte)HumanBodyBones.RightHand) { return VRCPickup.PickupHand.Right; }
                }
                return VRCPickup.PickupHand.None;
            }
        }

        public bool IsEquiped   // ボーンに装着モード
        {
            get => _isEquiped;
            private set
            {
                Initialize();

                _isEquiped = value;
                if (value)
                {
                    if (_pickup && _pickup.IsHeld) { _pickup.Drop(); }
                    _isAttached = false;
                    _updateManager.EnablePostLateUpdate(this);
                }
                if (_rigidbody) { IsKinematic = IsKinematic; }
                RequestSerialization();
            }
        }

        public bool IsAttached  // アタッチモード
        {
            get => _isAttached;
            private set
            {
                Initialize();

                _isAttached = value;
                if (value)
                {
                    if (_pickup && _pickup.IsHeld) { _pickup.Drop(); }
                    _isEquiped = false;
                    _updateManager.EnablePostLateUpdate(this);
                }
                if (_rigidbody) { IsKinematic = IsKinematic; }
                RequestSerialization();
            }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            EditorApplication.delayCall += () => { if (this) this.MakeUpdateManager(); };
        }

        private void MakeUpdateManager()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { return; }
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) { return; }
            if (PrefabUtility.IsPartOfPrefabAsset(this)) { return; }

            if (_updateManager) { return; }
            if (_updateManager = FindObjectOfType<MOSUpdateManager>())
            {
                _respawnHightY = _updateManager.respawnHeightY;
                RecordSelf();
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_UpdateManagerPrefabGUID));
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

        bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _rigidbody = GetComponent<Rigidbody>();
            _pickup = GetComponent<VRCPickup>();
            _localPlayer = Networking.LocalPlayer;

            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _startScale = transform.localScale;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
            _localScale = transform.localScale;

            moveCheckTickRate = Mathf.Max(moveCheckTickRate, 2);
            _firstCheckTiming = moveCheckTickRate + GetInstanceID() % moveCheckTickRate;

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
                // _sync系が全部初期値ならInitialize時点では同期されてきてないと見なして初期化
                _syncPosition = _startPosition;
                _syncRotation = _startRotation;
                _syncScale = _startScale;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();

            UseGravity = UseGravity;
            IsKinematic = IsKinematic;
            Pickupable = Pickupable;

            if (Networking.IsOwner(this.gameObject))
            {
                if (_reservedInterval) { return; }

                _reservedInterval = true;
                SendCustomEventDelayedFrames(nameof(_IntervalPostLateUpdate), _firstCheckTiming);
            }
        }

        public void _OnPostLateUpdate()
        {
            if (AttachToTransform()) { return; }

            if (EquipBone()) { return; }

            if (Networking.IsOwner(this.gameObject))
            {
                if (!PickupOffsetCheck())
                {
                    TransformMoveCheck();
                }
            }
            else
            {
                if (HoldingOther()) { return; }
            }

            ApplySyncTransform();

            _updateManager.DisablePostLateUpdate(this);
        }

        public void _IntervalPostLateUpdate()
        {
            if (_reservedInterval = Networking.IsOwner(this.gameObject))
            {
                _updateManager.EnablePostLateUpdate(this);
                SendCustomEventDelayedFrames(nameof(_IntervalPostLateUpdate), moveCheckTickRate);
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            Initialize();

            if (player.isLocal && !_reservedInterval)
            {
                _reservedInterval = true;
                SendCustomEventDelayedFrames(nameof(_IntervalPostLateUpdate), _firstCheckTiming);
            }

            // 他人がOwner化＝ピックアップを奪われた
            if (_pickup && !player.isLocal)
            {
                _pickup.Drop();
            }
            // 新Ownerが改めて同期するものとして、いったん強制リセット
            IsHeld = false;

            // 装備は強制パージ
            IsEquiped = false;
        }

        public override void OnDeserialization()
        {
            Initialize();

            // 本当に_syncPosition/Rotation/Scaleに変化があったかは見ない
            _syncHasChanged = true;
            _updateManager.EnablePostLateUpdate(this);
        }

        // VRCPickupとRigidbodyがある
        public override void OnPickup()
        {
            Networking.SetOwner(_localPlayer, this.gameObject);
            IsHeld = true;

            PickupOffsetCheck();
        }

        // VRCPickupとRigidbodyがある
        public override void OnDrop()
        {
            IsHeld = false;

            _syncPosition = transform.position;
            _syncRotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;

            RequestSerialization();

            transform.hasChanged = false;
        }

        public void Respawn()
        {
            Initialize();

            if (Networking.IsOwner(this.gameObject))
            {
                if (_pickup) { _pickup.Drop(); }
                IsEquiped = false;
                IsAttached = false;

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

                _syncPosition = _startPosition;
                _syncRotation = _startRotation;
                _localPosition = transform.localPosition;
                _localRotation = transform.localRotation;

                RequestSerialization();

                transform.hasChanged = false;
            }
        }

        public void ResetScale()
        {
            Initialize();

            if (Networking.IsOwner(this.gameObject))
            {
                transform.localScale = _startScale;

                _syncScale = _startScale;
                _localScale = transform.localScale;

                RequestSerialization();

                transform.hasChanged = false;
            }
        }

        public void Equip(HumanBodyBones targetBone)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            IsEquiped = true;

            _equipBone = (byte)targetBone;
            var bonePosition = _localPlayer.GetBonePosition(targetBone);
            var boneRotation = _localPlayer.GetBoneRotation(targetBone);
            _syncPosition = bonePosition.Equals(Vector3.zero) ? Vector3.zero : Quaternion.Inverse(boneRotation) * (transform.position - bonePosition);
            _syncRotation = boneRotation.Equals(Quaternion.identity) ? Quaternion.identity : (Quaternion.Inverse(boneRotation) * transform.rotation);

            RequestSerialization();
        }
        public void Unequip()
        {
            if (Networking.IsOwner(this.gameObject)) { IsEquiped = false; }
        }

        public void Attach()
        {
            if (Networking.IsOwner(this.gameObject)) { IsAttached = true; }
        }
        public void Detach()
        {
            if (Networking.IsOwner(this.gameObject)) { IsAttached = false; }
        }

        private bool TransformMoveCheck()
        {
            if (!transform.hasChanged) { return false; }

            if (transform.position.y <= _respawnHightY)
            {
                Respawn();
                return true;
            }

            if (moveCheckSpace == Space.Self &&
                (transform.localPosition != _localPosition ||
                 transform.localRotation != _localRotation))
            {
                SyncLocation();
            }
            else if (moveCheckSpace == Space.World &&
                    (transform.position != _syncPosition ||
                     transform.rotation != _syncRotation))
            {
                SyncLocation();
            }

            if (transform.localScale != _localScale)
            {
                SyncScale();
            }

            transform.hasChanged = false;

            return true;
        }
        private void SyncLocation()
        {
            _syncPosition = transform.position;
            _syncRotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;

            RequestSerialization();
        }
        private void SyncScale()
        {
            _syncScale = transform.localScale;
            _localScale = transform.localScale;

            RequestSerialization();
        }

        private bool ApplySyncTransform()
        {
            if (!_syncHasChanged) { return _syncHasChanged; }

            if (_rigidbody)
            {
                _rigidbody.MovePosition(_syncPosition);
                _rigidbody.MoveRotation(_syncRotation);
            }
            else
            {
                transform.SetPositionAndRotation(_syncPosition, _syncRotation);
            }
            var parent = transform.parent;
            _localPosition = parent ? transform.InverseTransformPoint(_syncPosition) : _syncPosition;
            _localRotation = parent ? Quaternion.Inverse(parent.rotation) * _syncRotation : _syncRotation;

            if (transform.localScale != _syncScale)
            {
                transform.localScale = _syncScale;
                _localScale = _syncScale;
            }

            _syncHasChanged = false;

            return true;
        }

        // _isHeldならVRCPickupとRigidbodyが付いている
        private bool PickupOffsetCheck()
        {
            if (!_isHeld) { return false; }

            var pickupHandBone = (_pickup.currentHand == VRCPickup.PickupHand.Left) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
            if (_equipBone != (byte)pickupHandBone)
            {
                _equipBone = (byte)pickupHandBone;
                RequestSerialization();
            }

            if (!_localPlayer.IsUserInVR()) { pickupHandBone = HumanBodyBones.Head; }

            var handPosition = _localPlayer.GetBonePosition(pickupHandBone);
            var handRotation = _localPlayer.GetBoneRotation(pickupHandBone);

            var offsetPosition = handPosition.Equals(Vector3.zero) ? Vector3.zero : Quaternion.Inverse(handRotation) * (_rigidbody.position - handPosition);
            var offsetRotation = handRotation.Equals(Quaternion.identity) ? Quaternion.identity : (Quaternion.Inverse(handRotation) * _rigidbody.rotation);

            if (offsetPosition != _syncPosition
             || offsetRotation != _syncRotation)
            {
                _syncPosition = offsetPosition;
                _syncRotation = offsetRotation;

                RequestSerialization();
            }

            return true;
        }

        // _isHeldならVRCPickupとRigidbodyが付いている
        private bool HoldingOther()
        {
            if (!_isHeld) { return false; }

            var owner = Networking.GetOwner(this.gameObject);
            if (!Utilities.IsValid(owner)) { return true; }

            var pickupHandBone = owner.IsUserInVR() ?
                (_equipBone == (byte)HumanBodyBones.LeftHand) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand :
                HumanBodyBones.Head;
            var handPosition = owner.GetBonePosition(pickupHandBone);
            var handRotation = owner.GetBoneRotation(pickupHandBone);

            if (handPosition.Equals(Vector3.zero) ||
                handRotation.Equals(Quaternion.identity))
            {
                // ボーン情報の代わりにプレイヤー原点からの固定値
                handPosition = new Vector3((_equipBone == (byte)HumanBodyBones.LeftHand) ? -0.2f : 0.2f, 1.0f, 0.3f);
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
            if (!_isEquiped) { return false; }

            var owner = Networking.GetOwner(this.gameObject);
            if (!Utilities.IsValid(owner)) { return true; }

            var bonePosition = owner.GetBonePosition((HumanBodyBones)_equipBone);
            var boneRotation = owner.GetBoneRotation((HumanBodyBones)_equipBone);
            if (bonePosition.Equals(Vector3.zero) || boneRotation.Equals(Quaternion.identity)) { return _isEquiped; }

            var equipPosition = bonePosition + (boneRotation * _syncPosition);
            var equipRotation = boneRotation * _syncRotation;

            transform.SetPositionAndRotation(equipPosition, equipRotation);
            _syncHasChanged = false;

            return true;
        }

        private bool AttachToTransform()
        {
            if (!_isAttached) { return false; }

            transform.SetPositionAndRotation(attachPoint.position, attachPoint.rotation);
            _syncHasChanged = false;

            return true;
        }
    }
}
