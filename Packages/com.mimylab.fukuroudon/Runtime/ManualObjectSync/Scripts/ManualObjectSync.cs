/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
//using VRC.Udon;

namespace MimyLab
{
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Manual ObjectSync")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ManualObjectSync : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("The tick rate depends on the fps of this game object owner's client.")]
        [Min(1)]
        public int moveCheckTickRate = 30;  // 変動確認の周期(fps依存)
        public Space moveCheckSpace = Space.Self;    // 変動確認をローカル座標系でするか
        [Space]
        public float respawnHightY = -100.0f;   // ここより落下したらリスポーンする

        [Header("Option Settings")]
        public Transform attachPoint = null;   // アタッチモード時の追従先

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
                }
                if (_rigidbody) { IsKinematic = IsKinematic; }
                if (_pickup) { Pickupable = Pickupable; }
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
                    _isAttached = false;
                    if (_pickup && _pickup.IsHeld) { _pickup.Drop(); }
                }
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
                    _isEquiped = false;
                    if (_pickup && _pickup.IsHeld) { _pickup.Drop(); }
                }
                RequestSerialization();
            }
        }

        [UdonSynced] Vector3 _syncPosition = Vector3.zero; // 位置同期用、ピックアップ時はオフセット用
        [UdonSynced] Quaternion _syncRotation = Quaternion.identity; // 回転同期用、ピックアップ時はオフセット用
        [UdonSynced] Vector3 _syncScale = Vector3.one;    // 拡縮同期用

        [FieldChangeCallback(nameof(UseGravity))]
        [UdonSynced] bool _useGravity = false;
        [FieldChangeCallback(nameof(IsKinematic))]
        [UdonSynced] bool _isKinematic = true;
        [FieldChangeCallback(nameof(Pickupable))]
        [UdonSynced] bool _pickupable = true;
        [FieldChangeCallback(nameof(IsHeld))]
        [UdonSynced] bool _isHeld = false;
        [UdonSynced] byte _equipBone = (byte)VRCPickup.PickupHand.None;

        [FieldChangeCallback(nameof(IsEquiped))]
        [UdonSynced] bool _isEquiped = false;

        [FieldChangeCallback(nameof(IsAttached))]
        [UdonSynced] bool _isAttached = false;

        // 初期値保存用
        Vector3 _startPosition, _localPosition;
        Quaternion _startRotation, _localRotation;
        Vector3 _startScale = Vector3.one, _localScale = Vector3.one;

        // 計算用
        Transform _transform;
        Rigidbody _rigidbody = null;
        VRCPickup _pickup = null;
        VRCPlayerApi _localPlayer, _ownerPlayer;
        int _moveCheckTiming;
        bool _syncHasChanged = false;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                var rootGameObjects = activeScene.GetRootGameObjects();
                foreach (var item in rootGameObjects)
                {
                    var sceneDesctiptor = item.GetComponentInChildren<VRC_SceneDescriptor>(true);
                    if (sceneDesctiptor)
                    {
                        respawnHightY = sceneDesctiptor.RespawnHeightY;
                        break;
                    }
                }
            }
        }
#endif

        bool _initialized = false;
        void Initialize()
        {
            if (_initialized) { return; }

            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
            _pickup = GetComponent<VRCPickup>();
            _localPlayer = Networking.LocalPlayer;

            _startPosition = _transform.position;
            _startRotation = _transform.rotation;
            _startScale = _transform.localScale;
            _localPosition = _transform.localPosition;
            _localRotation = _transform.localRotation;
            _localScale = _transform.localScale;

            _moveCheckTiming = GetInstanceID() % moveCheckTickRate;

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
            else
            {
                _syncHasChanged = true;
            }

            _initialized = true;
        }
        void Start()
        {
            Initialize();

            UseGravity = UseGravity;
            IsKinematic = IsKinematic;
            Pickupable = Pickupable;
        }

        public override void PostLateUpdate()
        {
            if (_isAttached && attachPoint)
            {
                AttachToTransform();
                return;
            }

            if (_isEquiped)
            {
                EquipBone();
                return;
            }

            if (Networking.IsOwner(this.gameObject))
            {
                if (_isHeld)
                {
                    if (Time.frameCount % moveCheckTickRate == _moveCheckTiming)
                    {
                        PickupOffsetCheck();
                    }
                }
                else if (_transform.hasChanged)
                {
                    if (_transform.position.y <= respawnHightY)
                    {
                        Respawn();
                    }
                    else if (Time.frameCount % moveCheckTickRate == _moveCheckTiming)
                    {
                        TransformMoveCheck();
                    }
                }
            }
            else
            {
                if (_isHeld)
                {
                    HoldingOther();
                }
                else if (_syncHasChanged)
                {
                    ApplySyncTransform();
                }
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            Initialize();

            if (_pickup)
            {
                if (player.isLocal)
                {
                    // 自分がOwner化＝ピックアップを奪ったか、Ownerだった人が落ちた
                    if (!_pickup.IsHeld)
                    {
                        IsHeld = false;
                        RequestSerialization();
                    }
                }
                else
                {
                    // 他人がOwner化＝ピックアップを奪われた
                    _pickup.Drop();
                }
            }

            // Ownerに物理演算書き戻し
            if (_rigidbody)
            {
                _rigidbody.isKinematic = (player.isLocal) ? IsKinematic : true;
            }
        }

        public override void OnDeserialization()
        {
            Initialize();

            // 本当に_syncPosition/Rotation/Scaleに変化があったかは見ない
            _syncHasChanged = true;
        }

        // OnOwnershipTransferred()が発火しないバグが修正されたため、この処理は不要になった
        /* public override void OnPlayerLeft(VRCPlayerApi player)
        {
            Initialize();

            if (_pickup)
            {
                if (Networking.IsOwner(this.gameObject))
                {
                    // Pickup抱えたまま落ちたかもしれないのでピックアップ状況更新
                    _isHeld = _pickup.IsHeld;
                    _pickup.pickupable = Pickupable;
                    RequestSerialization();
                }
            }

            // Ownerが落ちたかもしれないので改めて物理演算更新
            if (_rigidbody)
            {
                _rigidbody.isKinematic = (Networking.IsOwner(this.gameObject)) ? IsKinematic : true;
            }
        } */

        // VRCPickupとRigidbodyがある
        public override void OnPickup()
        {
            Networking.SetOwner(_localPlayer, this.gameObject);
            IsHeld = true;

            PickupOffsetCheck();

            RequestSerialization();
        }

        // VRCPickupとRigidbodyがある
        public override void OnDrop()
        {
            IsHeld = false;

            _syncPosition = _transform.position;
            _syncRotation = _transform.rotation;
            _localPosition = _transform.localPosition;
            _localRotation = _transform.localRotation;

            RequestSerialization();

            _transform.hasChanged = false;
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
                    _transform.SetPositionAndRotation(_startPosition, _startRotation);
                }

                _syncPosition = _startPosition;
                _syncRotation = _startRotation;
                _localPosition = _transform.localPosition;
                _localRotation = _transform.localRotation;

                RequestSerialization();

                _transform.hasChanged = false;
            }
        }

        public void ResetScale()
        {
            Initialize();

            if (Networking.IsOwner(this.gameObject))
            {
                _transform.localScale = _startScale;

                _syncScale = _startScale;
                _localScale = _transform.localScale;

                RequestSerialization();

                _transform.hasChanged = false;
            }
        }

        public void Equip(HumanBodyBones targetBone)
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }

            IsEquiped = true;

            _equipBone = (byte)targetBone;
            var bonePosition = _localPlayer.GetBonePosition(targetBone);
            var boneRotation = _localPlayer.GetBoneRotation(targetBone);
            _syncPosition = (bonePosition.Equals(Vector3.zero)) ? Vector3.zero : Quaternion.Inverse(boneRotation) * (_transform.position - bonePosition);
            _syncRotation = (boneRotation.Equals(Quaternion.identity)) ? Quaternion.identity : (Quaternion.Inverse(boneRotation) * _transform.rotation);

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

        void TransformMoveCheck()
        {
            if (moveCheckSpace == Space.Self
            && (_transform.localPosition != _localPosition
             || _transform.localRotation != _localRotation))
            {
                SyncLocation();
            }
            else if (moveCheckSpace == Space.World
            && (_transform.position != _syncPosition
             || _transform.rotation != _syncRotation))
            {
                SyncLocation();
            }
            if (_transform.localScale != _localScale)
            {
                SyncScale();
            }

            _transform.hasChanged = false;
        }
        void SyncLocation()
        {
            _syncPosition = _transform.position;
            _syncRotation = _transform.rotation;
            _localPosition = _transform.localPosition;
            _localRotation = _transform.localRotation;

            RequestSerialization();
        }
        void SyncScale()
        {
            _syncScale = _transform.localScale;
            _localScale = _transform.localScale;

            RequestSerialization();
        }

        void ApplySyncTransform()
        {
            if (_rigidbody)
            {
                _rigidbody.MovePosition(_syncPosition);
                _rigidbody.MoveRotation(_syncRotation);
            }
            else
            {
                _transform.SetPositionAndRotation(_syncPosition, _syncRotation);
            }
            _localPosition = _syncPosition;
            _localRotation = _syncRotation;

            if (_transform.localScale != _syncScale)
            {
                _transform.localScale = _syncScale;
                _localScale = _syncScale;
            }

            _syncHasChanged = false;
        }

        // _isHeldならVRCPickupとRigidbodyが付いている
        void PickupOffsetCheck()
        {
            var pickupHandBone = (_pickup.currentHand == VRCPickup.PickupHand.Left) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
            if (_equipBone != (byte)pickupHandBone)
            {
                _equipBone = (byte)pickupHandBone;
                RequestSerialization();
            }

            var handPosition = _localPlayer.GetBonePosition(pickupHandBone);
            var handRotation = _localPlayer.GetBoneRotation(pickupHandBone);

            var offsetPosition = (handPosition.Equals(Vector3.zero)) ? Vector3.zero : Quaternion.Inverse(handRotation) * (_rigidbody.position - handPosition);
            var offsetRotation = (handRotation.Equals(Quaternion.identity)) ? Quaternion.identity : (Quaternion.Inverse(handRotation) * _rigidbody.rotation);

            if (offsetPosition != _syncPosition
             || offsetRotation != _syncRotation)
            {
                _syncPosition = offsetPosition;
                _syncRotation = offsetRotation;

                RequestSerialization();
            }
        }

        // _isHeldならVRCPickupとRigidbodyが付いている
        void HoldingOther()
        {
            _ownerPlayer = Networking.GetOwner(this.gameObject);
            if (!Utilities.IsValid(_ownerPlayer)) { return; }

            var pickupHandBone = (_equipBone == (byte)HumanBodyBones.LeftHand) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
            var handPosition = _ownerPlayer.GetBonePosition(pickupHandBone);
            var handRotation = _ownerPlayer.GetBoneRotation(pickupHandBone);

            if (handPosition.Equals(Vector3.zero)
             || handRotation.Equals(Quaternion.identity))
            {
                // ボーン情報の代わりにプレイヤー原点からの固定値
                handPosition = new Vector3((_equipBone == (byte)HumanBodyBones.LeftHand) ? -0.2f : 0.2f, 1.0f, 0.3f);
                _rigidbody.MovePosition(_ownerPlayer.GetPosition() + (_ownerPlayer.GetRotation() * handPosition));
                _rigidbody.MoveRotation(_ownerPlayer.GetRotation());
            }
            else
            {
                _rigidbody.MovePosition(handPosition + (handRotation * _syncPosition));
                _rigidbody.MoveRotation(handRotation * _syncRotation);
            }
        }

        void EquipBone()
        {
            _ownerPlayer = Networking.GetOwner(this.gameObject);
            if (!Utilities.IsValid(_ownerPlayer)) { return; }

            var bonePosition = _ownerPlayer.GetBonePosition((HumanBodyBones)_equipBone);
            var boneRotation = _ownerPlayer.GetBoneRotation((HumanBodyBones)_equipBone);
            if (bonePosition.Equals(Vector3.zero) || boneRotation.Equals(Quaternion.identity)) { return; }

            var equipPosition = bonePosition + (boneRotation * _syncPosition);
            var equipRotation = boneRotation * _syncRotation;

            _transform.SetPositionAndRotation(equipPosition, equipRotation);
            _syncHasChanged = false;
        }

        void AttachToTransform()
        {
            _transform.SetPositionAndRotation(attachPoint.position, attachPoint.rotation);
            _syncHasChanged = false;
        }
    }
}
