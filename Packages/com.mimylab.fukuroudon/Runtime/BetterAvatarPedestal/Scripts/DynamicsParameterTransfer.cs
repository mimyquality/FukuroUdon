/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    using VRC.SDK3.Dynamics.PhysBone.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Better-AvatarPedestal#dynamics-parameter-transfer")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Better AvatarPedestal/Dynamics Parameter Transfer")]
    [RequireComponent(typeof(Animator))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DynamicsParameterTransfer : UdonSharpBehaviour
    {
        private Animator _animator;
        private VRCPhysBone[] _physbones = new VRCPhysBone[0];
        private ContactReceiverInfomation[] _contacts = new ContactReceiverInfomation[0];
        private UdonRaycast[] _raycasts = new UdonRaycast[0];

        private VRCPhysBone[] _pbs_IsGrabbed = new VRCPhysBone[0];
        private int[] _pbs_IsGrabbedType = new int[0];
        private int[] _pbs_IsGrabbedHash = new int[0];
        private VRCPhysBone[] _pbs_IsPosed = new VRCPhysBone[0];
        private int[] _pbs_IsPosedType = new int[0];
        private int[] _pbs_IsPosedHash = new int[0];
        private VRCPhysBone[] _pbs_Angle = new VRCPhysBone[0];
        private int[] _pbs_AngleType = new int[0];
        private int[] _pbs_AngleHash = new int[0];
        private VRCPhysBone[] _pbs_Stretch = new VRCPhysBone[0];
        private int[] _pbs_StretchType = new int[0];
        private int[] _pbs_StretchHash = new int[0];
        private VRCPhysBone[] _pbs_Squish = new VRCPhysBone[0];
        private int[] _pbs_SquishType = new int[0];
        private int[] _pbs_SquishHash = new int[0];

        private ContactReceiverInfomation[] _crs_Constant = new ContactReceiverInfomation[0];
        private int[] _crs_ConstantType = new int[0];
        private int[] _crs_ConstantHash = new int[0];
        private ContactReceiverInfomation[] _crs_OnEnter = new ContactReceiverInfomation[0];
        private int[] _crs_OnEnterType = new int[0];
        private int[] _crs_OnEnterHash = new int[0];
        private ContactReceiverInfomation[] _crs_Proximity = new ContactReceiverInfomation[0];
        private int[] _crs_ProximityType = new int[0];
        private int[] _crs_ProximityHash = new int[0];

        private UdonRaycast[] _urcs_Hit = new UdonRaycast[0];
        private int[] _urcs_HitType = new int[0];
        private int[] _urcs_HitHash = new int[0];
        private UdonRaycast[] _urcs_Distance = new UdonRaycast[0];
        private int[] _urcs_DistanceType = new int[0];
        private int[] _urcs_DistanceHash = new int[0];
        private UdonRaycast[] _urcs_Ratio = new UdonRaycast[0];
        private int[] _urcs_RatioType = new int[0];
        private int[] _urcs_RatioHash = new int[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _animator = GetComponent<Animator>();
            _physbones = GetComponentsInChildren<VRCPhysBone>(true);
            _contacts = GetComponentsInChildren<ContactReceiverInfomation>(true);
            _raycasts = GetComponentsInChildren<UdonRaycast>(true);

            _pbs_IsGrabbed = GetPhysbonesInAnimatorParameter("_IsGrabbed", out _pbs_IsGrabbedType, out _pbs_IsGrabbedHash);
            _pbs_IsPosed = GetPhysbonesInAnimatorParameter("_IsPosed", out _pbs_IsPosedType, out _pbs_IsPosedHash);
            _pbs_Angle = GetPhysbonesInAnimatorParameter("_Angle", out _pbs_AngleType, out _pbs_AngleHash);
            _pbs_Stretch = GetPhysbonesInAnimatorParameter("_Stretch", out _pbs_StretchType, out _pbs_StretchHash);
            _pbs_Squish = GetPhysbonesInAnimatorParameter("_Squish", out _pbs_SquishType, out _pbs_SquishHash);

            _crs_Constant = GetContactsInAnimatorParameter("_Constant", out _crs_ConstantType, out _crs_ConstantHash);
            _crs_OnEnter = GetContactsInAnimatorParameter("_OnEnter", out _crs_OnEnterType, out _crs_OnEnterHash);
            _crs_Proximity = GetContactsInAnimatorParameter("_Proximity", out _crs_ProximityType, out _crs_ProximityHash);

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
                int index = System.Array.IndexOf(referencePhysboneHashes, hash);
                if (index < 0) { continue; }

                tmp_physbones[physboneCount] = _physbones[index];
                tmp_physboneTypes[physboneCount] = (int)parameters[i].type;
                tmp_physboneHashes[physboneCount] = hash;
                physboneCount++;
            }
            var physbones = new VRCPhysBone[physboneCount];
            physboneTypes = new int[physboneCount];
            physboneHashes = new int[physboneCount];
            System.Array.Copy(tmp_physbones, physbones, physboneCount);
            System.Array.Copy(tmp_physboneTypes, physboneTypes, physboneCount);
            System.Array.Copy(tmp_physboneHashes, physboneHashes, physboneCount);

            return physbones;
        }

        private ContactReceiverInfomation[] GetContactsInAnimatorParameter(string suffix, out int[] contactTypes, out int[] contactHashes)
        {
            var referenceContactHashes = new int[_contacts.Length];
            for (int i = 0; i < _contacts.Length; i++)
            {
                string crName = _contacts[i] ? (_contacts[i].gameObject.name + suffix) : "";
                referenceContactHashes[i] = Animator.StringToHash(crName);
            }

            AnimatorControllerParameter[] parameters = _animator.parameters;
            int parameterCount = _animator.parameterCount;

            var tmp_contacts = new ContactReceiverInfomation[parameterCount];
            var tmp_contactTypes = new int[parameterCount];
            var tmp_contactHashes = new int[parameterCount];
            var contactCount = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                int hash = parameters[i].nameHash;
                int index = System.Array.IndexOf(referenceContactHashes, hash);
                if (index < 0) { continue; }

                tmp_contacts[contactCount] = _contacts[index];
                tmp_contactTypes[contactCount] = (int)parameters[i].type;
                tmp_contactHashes[contactCount] = hash;
                contactCount++;
            }
            var contacts = new ContactReceiverInfomation[contactCount];
            contactTypes = new int[contactCount];
            contactHashes = new int[contactCount];
            System.Array.Copy(tmp_contacts, contacts, contactCount);
            System.Array.Copy(tmp_contactTypes, contactTypes, contactCount);
            System.Array.Copy(tmp_contactHashes, contactHashes, contactCount);

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
                int index = System.Array.IndexOf(referenceRaycastHashes, hash);
                if (index < 0) { continue; }

                tmp_raycasts[raycastCount] = _raycasts[index];
                tmp_raycastTypes[raycastCount] = (int)parameters[i].type;
                tmp_raycastHashes[raycastCount] = hash;
                raycastCount++;
            }
            var raycasts = new UdonRaycast[raycastCount];
            raycastTypes = new int[raycastCount];
            raycastHashes = new int[raycastCount];
            System.Array.Copy(tmp_raycasts, raycasts, raycastCount);
            System.Array.Copy(tmp_raycastTypes, raycastTypes, raycastCount);
            System.Array.Copy(tmp_raycastHashes, raycastHashes, raycastCount);

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
