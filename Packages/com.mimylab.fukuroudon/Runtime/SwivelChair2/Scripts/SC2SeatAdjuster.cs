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
    using VRCStation = VRC.SDK3.Components.VRCStation;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Swivel-Chair-2#sc2-seat-adjuster")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/SC2 Seat Adjuster")]
    [RequireComponent(typeof(VRCStation))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SC2SeatAdjuster : UdonSharpBehaviour
    {
        [Header("Position Settings")]
        public Vector3 adjustMinLimit = new Vector3(0.0f, -0.5f, -0.3f);
        public Vector3 adjustMaxLimit = new Vector3(0.0f, 0.5f, 0.3f);
        [Min(0.0f), Tooltip("meter/sec")]
        public float adjustSpeed = 0.5f;

        [Space]
        public bool autoAdjustWhenSitting = false;
        [SerializeField]
        private Transform _seatPoint = null;

        [Header("Rotation Settings")]
        [Min(0.0f), Tooltip("degree")]
        public float forwardSnapThrethold = 5.0f;
        [Min(0.0f), Tooltip("degree/sec")]
        public float rotateSpeed = 60.0f;

        [UdonSynced, FieldChangeCallback(nameof(Offset))]
        private Vector3 _offset;
        [UdonSynced, FieldChangeCallback(nameof(Direction))]
        private Quaternion _direction;

        internal SwivelChair2 _swivelChair2;
        internal SC2AdjustmentSync _adjustmentSync;
        internal bool _isSitting = false;

        private VRCStation _station;
        private Transform _seat;
        private Transform _enterPoint;
        private Vector3 _localOffset;
        private bool _hasAvatarChangedSinceStandUp = false;
        private float _avatarEyeHeight = 0.0f;

        private Vector3 Offset
        {
            get => _offset;
            set
            {
                Initialize();
                value = Vector3.Max(value, adjustMinLimit);
                value = Vector3.Min(value, adjustMaxLimit);
                _enterPoint.localPosition = value;
                _offset = value;
                RequestSerialization();
            }
        }
        private Quaternion Direction
        {
            get => _direction;
            set
            {
                Initialize();
                _seat.localRotation = value;
                _direction = value;
                RequestSerialization();
            }
        }

        private Vector3 LocalOffset { get => _adjustmentSync && _adjustmentSync._hasSaved ? _adjustmentSync._localOffset : _localOffset; }
        private float AvatarEyeHeight { get => _adjustmentSync && _adjustmentSync._hasSaved ? _adjustmentSync._avatarEyeHeight : _avatarEyeHeight; }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _station = GetComponent<VRCStation>();
            _seat = _station.transform;
            _enterPoint = _station.stationEnterPlayerLocation ? _station.stationEnterPlayerLocation : _seat;
            _station.disableStationExit = true;

            _offset = _enterPoint.localPosition;
            _localOffset = _offset;
            _direction = _seat.localRotation;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void Interact()
        {
            Enter();
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }

            _isSitting = true;

            Networking.SetOwner(player, this.gameObject);
            Offset = LocalOffset;

            _swivelChair2.OnSitDown();

            if (autoAdjustWhenSitting &&
                _hasAvatarChangedSinceStandUp &&
                player.GetAvatarEyeHeightAsMeters() != AvatarEyeHeight)
            {
                SendCustomEventDelayedSeconds(nameof(AutoAdjust), 1.0f);
            }
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }

            _isSitting = false;
            _hasAvatarChangedSinceStandUp = false;

            _localOffset = _offset;
            _avatarEyeHeight = player.GetAvatarEyeHeightAsMeters();
            if (_adjustmentSync) { _adjustmentSync.Save(_offset, _avatarEyeHeight); }

            _swivelChair2.OnStandUp();
        }

        public override void OnAvatarChanged(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }

            _hasAvatarChangedSinceStandUp = true;

            if (autoAdjustWhenSitting && _isSitting)
            {
                SendCustomEventDelayedSeconds(nameof(AutoAdjust), 1.0f);
            }
        }

        public void Enter()
        {
            Initialize();
            _station.UseStation(Networking.LocalPlayer);
        }

        public void Exit()
        {
            Initialize();
            _station.ExitStation(Networking.LocalPlayer);
        }

        public void Revolve(float inputValue)
        {
            float angle = Time.deltaTime * rotateSpeed * inputValue;
            Quaternion result = _direction * Quaternion.AngleAxis(angle, Vector3.up);

            if (Mathf.Approximately(angle, 0.0f))
            {
                if (Quaternion.Angle(Quaternion.identity, result) < forwardSnapThrethold)
                {
                    result = Quaternion.identity;
                }
            }

            Direction = result;
        }

        public void Adjust(Vector3 inputValue)
        {
            Offset = _offset + (Time.deltaTime * adjustSpeed * inputValue);
        }

        public void AutoAdjust()
        {
            if (!autoAdjustWhenSitting) { return; }
            if (!_seatPoint) { return; }
            if (!_isSitting) { return; }

            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            Vector3 hipsPosition = localPlayer.GetBonePosition(HumanBodyBones.Hips);
            if (hipsPosition.Equals(Vector3.zero)) { return; }

            Transform parent = _enterPoint.parent;
            if (parent)
            {
                Offset = _offset + (parent.InverseTransformPoint(_seatPoint.position) - parent.InverseTransformPoint(hipsPosition));
            }
            else
            {
                Offset = _offset + (_seatPoint.position - hipsPosition);
            }
        }
    }
}
