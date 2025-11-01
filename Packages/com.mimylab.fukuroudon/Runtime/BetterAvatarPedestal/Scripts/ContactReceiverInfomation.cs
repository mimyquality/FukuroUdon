/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Dynamics.Contact.Components;
    using VRC.Dynamics;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Better-AvatarPedestal#dynamics-parameter-transfer")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Better AvatarPedestal/Contact Receiver Infomation")]
    [RequireComponent(typeof(VRCContactReceiver))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactReceiverInfomation : UdonSharpBehaviour
    {
        private const int MaxSenderCache = 128;

        [SerializeField]
        private string _parameter = "";
        [SerializeField, Min(0.0f), Tooltip("Minimum collision velocity to trigger OnEnter. m/s")]
        private float _minVelocity = 0.0f;

        private VRCContactReceiver _receiver;
        private ContactSenderProxy[] _senders = new ContactSenderProxy[MaxSenderCache];
        private bool _isSenderStay = false;
        private bool _isOnEnterFrame = false;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _receiver = GetComponent<VRCContactReceiver>();

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            if (contactInfo.enterVelocity.sqrMagnitude < _minVelocity * _minVelocity) { return; }

            AddSender(contactInfo.contactSender);
        }

        public override void OnContactExit(ContactExitInfo contactInfo)
        {
            RemoveSender(contactInfo.contactSender);
        }

        /******************************
         VRCContactReceiver Alies Property
         ******************************/
        public ContactSenderProxy[] StaySenders { get => _senders; }
        public bool Constant { get => _isSenderStay; } // 未実装
        public bool OnEnter { get => _isOnEnterFrame; } // 未実装
        public float Proximity { get => CalculateProximity(); }

        /******************************
         VRCContactReceiver Alies Method
         ******************************/
        public float CalculateProximity()
        {
            Initialize();

            var result = 0.0f;

            for (int i = 0; i < _senders.Length; i++)
            {
                if (_senders[i] == null) { continue; }
                if (!_senders[i].isValid)
                {
                    _senders[i] = null; continue;
                }

                var proximity = _receiver.CalculateProximity(_senders[i]);
                result = Mathf.Max(result, proximity);
            }

            return result;
        }

        public float CalculateProximity(VRCContactSender contactSender)
        {
            Initialize();

            if (!Utilities.IsValid(contactSender)) { return 0.0f; }

            return _receiver.CalculateProximity(contactSender);
        }

        public float CalculateProximity(ContactSenderProxy contactSender)
        {
            Initialize();

            if (!Utilities.IsValid(contactSender)) { return 0.0f; }
            if (!contactSender.isValid) { return 0.0f; }

            return _receiver.CalculateProximity(contactSender);

        }

        /******************************
         Internal Method
         ******************************/
        private void AddSender(ContactSenderProxy sender)
        {
            for (int i = 0; i < _senders.Length; i++)
            {
                if (!_senders[i].isValid)
                {
                    _senders[i] = null;
                }
            }

            var index = System.Array.IndexOf(_senders, null);
            if (index > -1)
            {
                _senders[index] = sender;
            }
        }

        private void RemoveSender(ContactSenderProxy sender)
        {
            int index;
            while ((index = System.Array.IndexOf(_senders, sender)) > -1)
            {
                _senders[index] = null;
            }
        }
    }
}
