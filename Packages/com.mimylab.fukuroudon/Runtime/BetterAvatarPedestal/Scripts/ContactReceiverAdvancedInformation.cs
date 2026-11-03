/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using System;
using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;

namespace MimyLab.FukuroUdon
{
    [HelpURL(
        "https://github.com/mimyquality/FukuroUdon/wiki/Better-AvatarPedestal#contact-receiver-advanced-information")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Better AvatarPedestal/Contact Receiver Advanced Information")]
    [RequireComponent(typeof(VRCContactReceiver))]
    [DefaultExecutionOrder(-1)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContactReceiverAdvancedInformation : UdonSharpBehaviour
    {
        private const int MaxSenderCache = 128;

        private VRCContactReceiver _receiver;
        private ContactReceiverProxy _receiverProxy;
        private ContactSenderProxy[] _senders = new ContactSenderProxy[MaxSenderCache];
        private string[][] _matchingTagsArray = new string[MaxSenderCache][];
        private int _last = -1;

        private ContactSenderProxy _nearestSender;
        private float _proximity;
        private Vector3 _senderLocalPosition;
        private Vector3 _senderVelocity;
        //private float _pointSeparation;
        private float _proximitySpeed;
        private string[] _matchingTags = new string[16];

        private bool _hasPrevSender;
        private Vector3 _prevSenderLocalPosition;
        private float _prevProximity;

        private bool _initialized = false;

        private void Initialize()
        {
            if (_initialized) return;

            _receiver = GetComponent<VRCContactReceiver>();

            _initialized = true;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            _nearestSender = RefreshNearestSender(out _proximity);
            
            bool hasSender = Utilities.IsValid(_nearestSender) && _nearestSender.isValid;
            _senderLocalPosition = hasSender
                ? Quaternion.Inverse(_receiverProxy.rotation) * (_nearestSender.position - _receiverProxy.position)
                : Vector3.zero;

            _senderVelocity = hasSender && _hasPrevSender
                ? (_senderLocalPosition - _prevSenderLocalPosition) / Time.deltaTime
                : Vector3.zero;

            _proximitySpeed = hasSender && _hasPrevSender ? (_proximity - _prevProximity) / Time.deltaTime : 0.0f;
            if (Mathf.Abs(_proximitySpeed) < 0.001f) _proximitySpeed = 0.0f; 
            
            _hasPrevSender = hasSender;
            _prevSenderLocalPosition = _senderLocalPosition;
            _prevProximity = _proximity;
        }

        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            if(contactInfo.contactReceiver == _receiver)
            {
                _receiverProxy = contactInfo.contactReceiver;
                AddSender(contactInfo);
                RefreshMatchingTags();
            }
        }

        public override void OnContactExit(ContactExitInfo contactInfo)
        {
            if (contactInfo.contactReceiver == _receiver)
            {
                RemoveSender(contactInfo);
                RefreshMatchingTags();
            }
        }

        /******************************
         VRCContactReceiver Alies Property
         ******************************/
        public DynamicsUsage SenderType
        {
            get => Utilities.IsValid(_nearestSender) && _nearestSender.isValid
                ? _nearestSender.usage
                : DynamicsUsage.Unassigned;
        }
        public Vector3 SenderLocalPosition { get => _senderLocalPosition; }
        public Vector3 SenderLossyScale
        {
            get => Utilities.IsValid(_nearestSender) && _nearestSender.isValid ? _nearestSender.scale : Vector3.one;
        }
        public Vector3 SenderVelocity { get => _senderVelocity; }
        public float SenderVelocityMagnitude { get => _senderVelocity.magnitude; }
        //public float PointSeparation { get => _pointSeparation; }
        public float ProximitySpeed { get => _proximitySpeed; }
        public string[] MatchingTags { get => _matchingTags; }

        /******************************
         Internal Method
         ******************************/
        private ContactSenderProxy RefreshNearestSender(out float proximity)
        {
            ContactSenderProxy sender = null;
            proximity = 0.0f;
            
            for (int i = 0; i < _senders.Length; i++)
            {
                if(!Utilities.IsValid(_senders[i])) continue;
                if (!_senders[i].isValid) continue;

                float tmp_proximity = _receiver.CalculateProximity(_senders[i]);
                if (proximity < tmp_proximity)
                {
                    sender = _senders[i];
                    proximity = tmp_proximity;
                }
            }

            return sender;
        }

        private void RefreshMatchingTags()
        {
            for (int i = 0; i < _matchingTags.Length; i++)
            {
                _matchingTags[i] = "";
            }

            for (int i = 0; i < _last; i++)
            {
                if (!Utilities.IsValid(_matchingTagsArray[i])) continue;

                for (int j = 0; j < _matchingTagsArray[i].Length; j++)
                {
                    int index = Array.IndexOf(_matchingTags, _matchingTagsArray[i][j]);
                    if (index > -1) continue;

                    index = Array.IndexOf(_matchingTags, "");
                    if (index < 0) continue;
                    _matchingTags[index] = _matchingTagsArray[i][j];
                }
            }
        }
        
        private void AddSender(ContactEnterInfo contactInfo)
        {
            _last = ValidateStayedSenders();

            int index = Array.IndexOf(_senders, null);
            if (index > -1)
            {
                _senders[index] = contactInfo.contactSender;
                _matchingTagsArray[index] = contactInfo.matchingTags;
                _last = Mathf.Max(_last, index);
            }
        }

        private void RemoveSender(ContactExitInfo contactInfo)
        {
            int index;
            while ((index = Array.LastIndexOf(_senders, contactInfo.contactSender)) > -1)
            {
                _senders[index] = null;
                _matchingTagsArray[index] = null;
                if (_last == index)
                {
                    _last--;
                }
            }
        }

        private int ValidateStayedSenders()
        {
            var last = -1;

            for (int i = 0; i < _senders.Length; i++)
            {
                if (!Utilities.IsValid(_senders[i])) continue;

                if (!_senders[i].isValid)
                {
                    _senders[i] = null;
                    _matchingTagsArray[i] = null;
                    continue;
                }

                last = i;
            }

            return last;
        }
    }
}