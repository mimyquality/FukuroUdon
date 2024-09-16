/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;
    using VRCStation = VRC.SDK3.Components.VRCStation;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/SC2 Seat Adjuster")]
    [RequireComponent(typeof(VRCStation))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SC2SeatAdjuster : UdonSharpBehaviour
    {
        [HideInInspector]
        public SwivelChair2 swivelChair2;

        public Vector3 adjustMinLimit = new Vector3(0.0f, -0.5f, -0.3f);
        public Vector3 adjustMaxLimit = new Vector3(0.0f, 0.5f, 0.3f);
        [Min(0.0f), Tooltip("meter/sec")]
        public float adjustSpeed = 0.5f;
        [Space]
        [Min(0.0f), Tooltip("degree")]
        public float forwardSnapThrethold = 5.0f;
        [Min(0.0f), Tooltip("degree/sec")]
        public float rotateSpeed = 60.0f;

        [UdonSynced, FieldChangeCallback(nameof(Offset))]
        private Vector3 _offset;
        [UdonSynced, FieldChangeCallback(nameof(Direction))]
        private Quaternion _direction;

        private VRCStation _station;
        private Transform _seat;
        private Transform _enterPoint;
        private Vector3 _localOffset;

        private Vector3 Offset
        {
            get => _offset;
            set
            {
                Initialize();
                _enterPoint.localPosition = value;
                _offset = value;
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
            }
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _station = GetComponent<VRCStation>();
            _seat = _station.transform;
            _enterPoint = (_station.stationEnterPlayerLocation) ? _station.stationEnterPlayerLocation : _seat;
            _station.disableStationExit = true;

            _offset = _enterPoint.localPosition;
            _localOffset = _offset;
            _direction = _seat.localRotation;
            _localOffset = _offset;

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

            swivelChair2.OnSitDown();

            Networking.SetOwner(player, this.gameObject);

            Offset = _localOffset;

            RequestSerialization();
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }

            swivelChair2.OnStandUp();

            _localOffset = Offset;
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
            var angle = Time.deltaTime * rotateSpeed * inputValue;
            var result = _direction * Quaternion.AngleAxis(angle, Vector3.up);

            if (Mathf.Approximately(angle, 0.0f))
            {
                if (Quaternion.Angle(Quaternion.identity, result) < forwardSnapThrethold)
                {
                    result = Quaternion.identity;
                }
            }

            Direction = result;
            RequestSerialization();
        }

        public void Adjust(Vector3 inputValue)
        {
            var shift = Time.deltaTime * adjustSpeed * inputValue;
            var result = _offset + shift;

            result = Vector3.Max(result, adjustMinLimit);
            result = Vector3.Min(result, adjustMaxLimit);

            Offset = result;
            RequestSerialization();
        }
    }
}
