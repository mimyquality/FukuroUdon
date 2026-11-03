/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using System;
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Dynamics.PhysBone.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Better-AvatarPedestal#dynamics-parameter-transfer")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Better AvatarPedestal/Dynamics Parameter Transfer")]
    [RequireComponent(typeof(Animator))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DynamicsParameterTransfer : UdonSharpBehaviour
    {
        private Animator _animator;
        private VRCPhysBone[] _physbones = Array.Empty<VRCPhysBone>();
        private ContactReceiverInformation[] _contacts = Array.Empty<ContactReceiverInformation>();
        private ContactReceiverAdvancedInformation[] _advancedContacts = Array.Empty<ContactReceiverAdvancedInformation>();
        private UdonRaycast[] _raycasts = Array.Empty<UdonRaycast>();

        private VRCPhysBone[] _pbs_IsGrabbed = Array.Empty<VRCPhysBone>();
        private int[] _pbs_IsGrabbedType = Array.Empty<int>();
        private int[] _pbs_IsGrabbedHash = Array.Empty<int>();
        private VRCPhysBone[] _pbs_IsPosed = Array.Empty<VRCPhysBone>();
        private int[] _pbs_IsPosedType = Array.Empty<int>();
        private int[] _pbs_IsPosedHash = Array.Empty<int>();
        private VRCPhysBone[] _pbs_Angle = Array.Empty<VRCPhysBone>();
        private int[] _pbs_AngleType = Array.Empty<int>();
        private int[] _pbs_AngleHash = Array.Empty<int>();
        private VRCPhysBone[] _pbs_Stretch = Array.Empty<VRCPhysBone>();
        private int[] _pbs_StretchType = Array.Empty<int>();
        private int[] _pbs_StretchHash = Array.Empty<int>();
        private VRCPhysBone[] _pbs_Squish = Array.Empty<VRCPhysBone>();
        private int[] _pbs_SquishType = Array.Empty<int>();
        private int[] _pbs_SquishHash = Array.Empty<int>();

        private ContactReceiverInformation[] _crs_Constant = Array.Empty<ContactReceiverInformation>();
        private int[] _crs_ConstantType = Array.Empty<int>();
        private int[] _crs_ConstantHash = Array.Empty<int>();
        private ContactReceiverInformation[] _crs_OnEnter = Array.Empty<ContactReceiverInformation>();
        private int[] _crs_OnEnterType = Array.Empty<int>();
        private int[] _crs_OnEnterHash = Array.Empty<int>();
        private ContactReceiverInformation[] _crs_Proximity = Array.Empty<ContactReceiverInformation>();
        private int[] _crs_ProximityType = Array.Empty<int>();
        private int[] _crs_ProximityHash = Array.Empty<int>();
        
        private ContactReceiverAdvancedInformation[] _crs_SenderType = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderTypeType = Array.Empty<int>();
        private int[] _crs_SenderTypeHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderLocalPositionX = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderLocalPositionXType = Array.Empty<int>();
        private int[] _crs_SenderLocalPositionXHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderLocalPositionY = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderLocalPositionYType = Array.Empty<int>();
        private int[] _crs_SenderLocalPositionYHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderLocalPositionZ = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderLocalPositionZType = Array.Empty<int>();
        private int[] _crs_SenderLocalPositionZHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderLossyScaleX = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderLossyScaleXType = Array.Empty<int>();
        private int[] _crs_SenderLossyScaleXHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderLossyScaleY = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderLossyScaleYType = Array.Empty<int>();
        private int[] _crs_SenderLossyScaleYHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderLossyScaleZ = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderLossyScaleZType = Array.Empty<int>();
        private int[] _crs_SenderLossyScaleZHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderVelocityX = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderVelocityXType = Array.Empty<int>();
        private int[] _crs_SenderVelocityXHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderVelocityY = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderVelocityYType = Array.Empty<int>();
        private int[] _crs_SenderVelocityYHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderVelocityZ = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderVelocityZType = Array.Empty<int>();
        private int[] _crs_SenderVelocityZHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_SenderVelocityMagnitude = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_SenderVelocityMagnitudeType = Array.Empty<int>();
        private int[] _crs_SenderVelocityMagnitudeHash = Array.Empty<int>();
        // private ContactReceiverAdvancedInformation[] _crs_PointSeparation = Array.Empty<ContactReceiverAdvancedInformation>();
        // private int[] _crs_PointSeparationType = Array.Empty<int>();
        // private int[] _crs_PointSeparationHash = Array.Empty<int>();
        private ContactReceiverAdvancedInformation[] _crs_ProximitySpeed = Array.Empty<ContactReceiverAdvancedInformation>();
        private int[] _crs_ProximitySpeedType = Array.Empty<int>();
        private int[] _crs_ProximitySpeedHash = Array.Empty<int>();
        // private ContactReceiverAdvancedInformation[] _crs_CollisionTag = Array.Empty<ContactReceiverAdvancedInformation>();
        // private int[] _crs_CollisionTagType = Array.Empty<int>();
        // private int[] _crs_CollisionTagHash = Array.Empty<int>();

        private UdonRaycast[] _urcs_Hit = Array.Empty<UdonRaycast>();
        private int[] _urcs_HitType = Array.Empty<int>();
        private int[] _urcs_HitHash = Array.Empty<int>();
        private UdonRaycast[] _urcs_Distance = Array.Empty<UdonRaycast>();
        private int[] _urcs_DistanceType = Array.Empty<int>();
        private int[] _urcs_DistanceHash = Array.Empty<int>();
        private UdonRaycast[] _urcs_Ratio = Array.Empty<UdonRaycast>();
        private int[] _urcs_RatioType = Array.Empty<int>();
        private int[] _urcs_RatioHash = Array.Empty<int>();

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _animator = GetComponent<Animator>();
            _physbones = GetComponentsInChildren<VRCPhysBone>(true);
            _contacts = GetComponentsInChildren<ContactReceiverInformation>(true);
            _raycasts = GetComponentsInChildren<UdonRaycast>(true);

            _pbs_IsGrabbed = GetPhysbonesInAnimatorParameter("_IsGrabbed", out _pbs_IsGrabbedType, out _pbs_IsGrabbedHash);
            _pbs_IsPosed = GetPhysbonesInAnimatorParameter("_IsPosed", out _pbs_IsPosedType, out _pbs_IsPosedHash);
            _pbs_Angle = GetPhysbonesInAnimatorParameter("_Angle", out _pbs_AngleType, out _pbs_AngleHash);
            _pbs_Stretch = GetPhysbonesInAnimatorParameter("_Stretch", out _pbs_StretchType, out _pbs_StretchHash);
            _pbs_Squish = GetPhysbonesInAnimatorParameter("_Squish", out _pbs_SquishType, out _pbs_SquishHash);

            _crs_Constant = GetContactsInAnimatorParameter("_Constant", out _crs_ConstantType, out _crs_ConstantHash);
            _crs_OnEnter = GetContactsInAnimatorParameter("_OnEnter", out _crs_OnEnterType, out _crs_OnEnterHash);
            _crs_Proximity = GetContactsInAnimatorParameter("_Proximity", out _crs_ProximityType, out _crs_ProximityHash);
            
            _crs_SenderType = GetAdvancedContactsInAnimatorParameter("_SenderType", out _crs_SenderTypeType, out _crs_SenderTypeHash);
            _crs_SenderLocalPositionX = GetAdvancedContactsInAnimatorParameter("_SenderLocalPositionX",  out _crs_SenderLocalPositionXType, out _crs_SenderLocalPositionXHash);
            _crs_SenderLocalPositionY = GetAdvancedContactsInAnimatorParameter("_SenderLocalPositionY", out  _crs_SenderLocalPositionYType, out _crs_SenderLocalPositionYHash);
            _crs_SenderLocalPositionZ = GetAdvancedContactsInAnimatorParameter("_SenderLocalPositionZ", out  _crs_SenderLocalPositionZType, out _crs_SenderLocalPositionZHash);
            _crs_SenderLossyScaleX = GetAdvancedContactsInAnimatorParameter("_SenderLossyScaleX", out _crs_SenderLossyScaleXType, out _crs_SenderLossyScaleXHash);
            _crs_SenderLossyScaleY = GetAdvancedContactsInAnimatorParameter("_SenderLossyScaleY", out _crs_SenderLossyScaleYType, out _crs_SenderLossyScaleYHash);
            _crs_SenderLossyScaleZ = GetAdvancedContactsInAnimatorParameter("_SenderLossyScaleZ", out _crs_SenderLossyScaleZType, out _crs_SenderLossyScaleZHash);
            _crs_SenderVelocityX = GetAdvancedContactsInAnimatorParameter("_SenderVelocityX", out _crs_SenderVelocityXType, out _crs_SenderVelocityXHash);
            _crs_SenderVelocityY = GetAdvancedContactsInAnimatorParameter("_SenderVelocityY", out _crs_SenderVelocityYType, out _crs_SenderVelocityYHash);
            _crs_SenderVelocityZ = GetAdvancedContactsInAnimatorParameter("_SenderVelocityZ", out _crs_SenderVelocityZType, out _crs_SenderVelocityZHash);
            _crs_SenderVelocityMagnitude = GetAdvancedContactsInAnimatorParameter("_SenderVelocityMagnitude", out _crs_SenderVelocityMagnitudeType, out _crs_SenderVelocityMagnitudeHash);
            // _crs_PointSeparation = GetAdvancedContactsInAnimatorParameter("_PointSeparation",  out _crs_PointSeparationType, out _crs_PointSeparationHash);
            _crs_ProximitySpeed = GetAdvancedContactsInAnimatorParameter("_ProximitySpeed",  out _crs_ProximitySpeedType, out _crs_ProximitySpeedHash);
            // _crs_CollisionTag = GetAdvancedContactsInAnimatorParameter("_CollisionTag",   out _crs_CollisionTagType, out _crs_CollisionTagHash);

            _urcs_Hit = GetRaycastsInAnimatorParameter("_Hit", out _urcs_HitType, out _urcs_HitHash);
            _urcs_Distance = GetRaycastsInAnimatorParameter("_Distance", out _urcs_DistanceType, out _urcs_DistanceHash);
            _urcs_Ratio = GetRaycastsInAnimatorParameter("_Ratio", out _urcs_RatioType, out _urcs_RatioHash);

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            // Physbones
            for (int i = 0; i < _pbs_IsGrabbed.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_pbs_IsGrabbedType[i], _pbs_IsGrabbedHash[i], _pbs_IsGrabbed[i].IsGrabbed);
            }
            for (int i = 0; i < _pbs_IsPosed.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_pbs_IsPosedType[i], _pbs_IsPosedHash[i], _pbs_IsPosed[i].IsPosed);
            }
            for (int i = 0; i < _pbs_Angle.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_pbs_AngleType[i], _pbs_AngleHash[i], _pbs_Angle[i].Angle);
            }
            for (int i = 0; i < _pbs_Stretch.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_pbs_StretchType[i], _pbs_StretchHash[i], _pbs_Stretch[i].Stretch);
            }
            for (int i = 0; i < _pbs_Squish.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_pbs_SquishType[i], _pbs_SquishHash[i], _pbs_Squish[i].Squish);
            }

            // Contacts
            for (int i = 0; i < _crs_Constant.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_ConstantType[i], _crs_ConstantHash[i], _crs_Constant[i].HasStayedSender());
            }
            for (int i = 0; i < _crs_OnEnter.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_OnEnterType[i], _crs_OnEnterHash[i], _crs_OnEnter[i].OnEnter);
            }
            for (int i = 0; i < _crs_Proximity.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_ProximityType[i], _crs_ProximityHash[i], _crs_Proximity[i].CalculateProximity());
            }
            
            // Advanced Contacts
            for (int i = 0; i < _crs_SenderType.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderTypeType[i], _crs_SenderTypeHash[i], (int)_crs_SenderType[i].SenderType);
            }
            for (int i = 0; i < _crs_SenderLocalPositionX.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderLocalPositionXType[i], _crs_SenderLocalPositionXHash[i], _crs_SenderLocalPositionX[i].SenderLocalPosition.x);
            }
            for (int i = 0; i < _crs_SenderLocalPositionY.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderLocalPositionYType[i], _crs_SenderLocalPositionYHash[i], _crs_SenderLocalPositionY[i].SenderLocalPosition.y);
            }
            for (int i = 0; i < _crs_SenderLocalPositionZ.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderLocalPositionZType[i], _crs_SenderLocalPositionZHash[i], _crs_SenderLocalPositionZ[i].SenderLocalPosition.z);
            }
            for (int i = 0; i < _crs_SenderLossyScaleX.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderLossyScaleXType[i], _crs_SenderLossyScaleXHash[i], _crs_SenderLossyScaleX[i].SenderLossyScale.x);                
            }
            for (int i = 0; i < _crs_SenderLossyScaleY.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderLossyScaleYType[i], _crs_SenderLossyScaleYHash[i], _crs_SenderLossyScaleY[i].SenderLossyScale.y);                
            }
            for (int i = 0; i < _crs_SenderLossyScaleZ.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderLossyScaleZType[i], _crs_SenderLossyScaleZHash[i], _crs_SenderLossyScaleZ[i].SenderLossyScale.z);                
            }
            for (int i = 0; i < _crs_SenderVelocityX.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderVelocityXType[i], _crs_SenderVelocityXHash[i], _crs_SenderVelocityX[i].SenderVelocity.x);
            }
            for (int i = 0; i < _crs_SenderVelocityY.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderVelocityYType[i], _crs_SenderVelocityYHash[i], _crs_SenderVelocityY[i].SenderVelocity.y);
            }
            for (int i = 0; i < _crs_SenderVelocityZ.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderVelocityZType[i], _crs_SenderVelocityZHash[i], _crs_SenderVelocityZ[i].SenderVelocity.z);
            }
            for (int i = 0; i < _crs_SenderVelocityMagnitude.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_SenderVelocityMagnitudeType[i], _crs_SenderVelocityMagnitudeHash[i], _crs_SenderVelocityMagnitude[i].SenderVelocityMagnitude);
            }
            // for (int i = 0; i < _crs_PointSeparation.Length; i++)
            // {
            //     SetAnimatorParameter((AnimatorControllerParameterType)_crs_PointSeparationType[i], _crs_PointSeparationHash[i], _crs_PointSeparation[i].PointSeparation);
            // }
            for (int i = 0; i < _crs_ProximitySpeed.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_crs_ProximitySpeedType[i], _crs_ProximitySpeedHash[i], _crs_ProximitySpeed[i].ProximitySpeed);
            }
            // for (int i = 0; i < _crs_CollisionTag.Length; i++)
            // {
            //     SetAnimatorParameter((AnimatorControllerParameterType)_crs_CollisionTagType[i], _crs_CollisionTagHash[i], _crs_CollisionTag[i].MatchingTags);
            // }
            
            // Raycasts
            for (int i = 0; i < _urcs_Hit.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_urcs_HitType[i], _urcs_HitHash[i], _urcs_Hit[i].Hit);
            }
            for (int i = 0; i < _urcs_Distance.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_urcs_DistanceType[i], _urcs_DistanceHash[i], _urcs_Distance[i].Distance);
            }
            for (int i = 0; i < _urcs_Ratio.Length; i++)
            {
                SetAnimatorParameter((AnimatorControllerParameterType)_urcs_RatioType[i], _urcs_RatioHash[i], _urcs_Ratio[i].Ratio);
            }
        }

        private VRCPhysBone[] GetPhysbonesInAnimatorParameter(string suffix, out int[] physboneTypes, out int[] physboneHashes)
        {
            var referencePhysboneHashes = new int[_physbones.Length];
            for (int i = 0; i < _physbones.Length; i++)
            {
                string pbName = _physbones[i] ? (_physbones[i].gameObject.name + suffix) : "";
                referencePhysboneHashes[i] = Animator.StringToHash(pbName);
            }

            AnimatorControllerParameter[] parameters = _animator.parameters;
            int parameterCount = _animator.parameterCount;

            var tmp_physbones = new VRCPhysBone[parameterCount];
            var tmp_physboneTypes = new int[parameterCount];
            var tmp_physboneHashes = new int[parameterCount];
            var physboneCount = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                int hash = parameters[i].nameHash;
                int index = Array.IndexOf(referencePhysboneHashes, hash);
                if (index < 0) { continue; }

                tmp_physbones[physboneCount] = _physbones[index];
                tmp_physboneTypes[physboneCount] = (int)parameters[i].type;
                tmp_physboneHashes[physboneCount] = hash;
                physboneCount++;
            }
            var physbones = new VRCPhysBone[physboneCount];
            physboneTypes = new int[physboneCount];
            physboneHashes = new int[physboneCount];
            Array.Copy(tmp_physbones, physbones, physboneCount);
            Array.Copy(tmp_physboneTypes, physboneTypes, physboneCount);
            Array.Copy(tmp_physboneHashes, physboneHashes, physboneCount);

            return physbones;
        }

        private ContactReceiverInformation[] GetContactsInAnimatorParameter(string suffix, out int[] contactTypes, out int[] contactHashes)
        {
            var referenceContactHashes = new int[_contacts.Length];
            for (int i = 0; i < _contacts.Length; i++)
            {
                string crName = _contacts[i] ? (_contacts[i].gameObject.name + suffix) : "";
                referenceContactHashes[i] = Animator.StringToHash(crName);
            }

            AnimatorControllerParameter[] parameters = _animator.parameters;
            int parameterCount = _animator.parameterCount;

            var tmp_contacts = new ContactReceiverInformation[parameterCount];
            var tmp_contactTypes = new int[parameterCount];
            var tmp_contactHashes = new int[parameterCount];
            var contactCount = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                int hash = parameters[i].nameHash;
                int index = Array.IndexOf(referenceContactHashes, hash);
                if (index < 0) { continue; }

                tmp_contacts[contactCount] = _contacts[index];
                tmp_contactTypes[contactCount] = (int)parameters[i].type;
                tmp_contactHashes[contactCount] = hash;
                contactCount++;
            }
            var contacts = new ContactReceiverInformation[contactCount];
            contactTypes = new int[contactCount];
            contactHashes = new int[contactCount];
            Array.Copy(tmp_contacts, contacts, contactCount);
            Array.Copy(tmp_contactTypes, contactTypes, contactCount);
            Array.Copy(tmp_contactHashes, contactHashes, contactCount);

            return contacts;
        }
        
        private ContactReceiverAdvancedInformation[] GetAdvancedContactsInAnimatorParameter(string suffix, out int[] contactTypes, out int[] contactHashes)
        {
            var referenceContactHashes = new int[_advancedContacts.Length];
            for (int i = 0; i < _advancedContacts.Length; i++)
            {
                string crName = _advancedContacts[i] ? (_advancedContacts[i].gameObject.name + suffix) : "";
                referenceContactHashes[i] = Animator.StringToHash(crName);
            }

            AnimatorControllerParameter[] parameters = _animator.parameters;
            int parameterCount = _animator.parameterCount;

            var tmp_contacts = new ContactReceiverAdvancedInformation[parameterCount];
            var tmp_contactTypes = new int[parameterCount];
            var tmp_contactHashes = new int[parameterCount];
            var contactCount = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                int hash = parameters[i].nameHash;
                int index = Array.IndexOf(referenceContactHashes, hash);
                if (index < 0) { continue; }

                tmp_contacts[contactCount] = _advancedContacts[index];
                tmp_contactTypes[contactCount] = (int)parameters[i].type;
                tmp_contactHashes[contactCount] = hash;
                contactCount++;
            }
            var contacts = new ContactReceiverAdvancedInformation[contactCount];
            contactTypes = new int[contactCount];
            contactHashes = new int[contactCount];
            Array.Copy(tmp_contacts, contacts, contactCount);
            Array.Copy(tmp_contactTypes, contactTypes, contactCount);
            Array.Copy(tmp_contactHashes, contactHashes, contactCount);

            return contacts;
        }

        private UdonRaycast[] GetRaycastsInAnimatorParameter(string suffix, out int[] raycastTypes, out int[] raycastHashes)
        {
            var referenceRaycastHashes = new int[_raycasts.Length];
            for (int i = 0; i < _raycasts.Length; i++)
            {
                string urcName = _raycasts[i] ? (_raycasts[i].gameObject.name + suffix) : "";
                referenceRaycastHashes[i] = Animator.StringToHash(urcName);
            }

            AnimatorControllerParameter[] parameters = _animator.parameters;
            int parameterCount = _animator.parameterCount;

            var tmp_raycasts = new UdonRaycast[parameterCount];
            var tmp_raycastTypes = new int[parameterCount];
            var tmp_raycastHashes = new int[parameterCount];
            var raycastCount = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                int hash = parameters[i].nameHash;
                int index = Array.IndexOf(referenceRaycastHashes, hash);
                if (index < 0) { continue; }

                tmp_raycasts[raycastCount] = _raycasts[index];
                tmp_raycastTypes[raycastCount] = (int)parameters[i].type;
                tmp_raycastHashes[raycastCount] = hash;
                raycastCount++;
            }
            var raycasts = new UdonRaycast[raycastCount];
            raycastTypes = new int[raycastCount];
            raycastHashes = new int[raycastCount];
            Array.Copy(tmp_raycasts, raycasts, raycastCount);
            Array.Copy(tmp_raycastTypes, raycastTypes, raycastCount);
            Array.Copy(tmp_raycastHashes, raycastHashes, raycastCount);

            return raycasts;
        }

        private void SetAnimatorParameter(AnimatorControllerParameterType type, int hash, bool value)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    var floatValue = value ? 1.0f : 0.0f;
                    if (!Mathf.Approximately(_animator.GetFloat(hash), floatValue))
                    {
                        _animator.SetFloat(hash, floatValue);
                    }
                    break;
                case AnimatorControllerParameterType.Int:
                    var intValue = value ? 1 : 0;
                    if (_animator.GetInteger(hash) != intValue)
                    {
                        _animator.SetInteger(hash, intValue);
                    }
                    break;
                case AnimatorControllerParameterType.Bool:
                    if (_animator.GetBool(hash) != value)
                    {
                        _animator.SetBool(hash, value);
                    }
                    break;
            }
        }

        private void SetAnimatorParameter(AnimatorControllerParameterType type, int hash, int value)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    var floatValue = (float)value;
                    if (!Mathf.Approximately(_animator.GetFloat(hash), floatValue))
                    {
                        _animator.SetFloat(hash, floatValue);
                    }
                    break;
                case AnimatorControllerParameterType.Int:
                    if (_animator.GetInteger(hash) != value)
                    {
                        _animator.SetInteger(hash, value);
                    }
                    break;
                case AnimatorControllerParameterType.Bool:
                    var boolValue = value > 0;
                    if (_animator.GetBool(hash) != boolValue)
                    {
                        _animator.SetBool(hash, boolValue);
                    }
                    break;
            }
        }

        private void SetAnimatorParameter(AnimatorControllerParameterType type, int hash, float value)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    if (!Mathf.Approximately(_animator.GetFloat(hash), value))
                    {
                        _animator.SetFloat(hash, value);
                    }
                    break;
                case AnimatorControllerParameterType.Int:
                    var intValue = (int)value;
                    if (_animator.GetInteger(hash) != intValue)
                    {
                        _animator.SetInteger(hash, intValue);
                    }
                    break;
                case AnimatorControllerParameterType.Bool:
                    var boolValue = !Mathf.Approximately(value, 0.0f);
                    if (_animator.GetBool(hash) != boolValue)
                    {
                        _animator.SetBool(hash, boolValue);
                    }
                    break;
            }
        }
    }
}
