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
    using VRC.SDK3.Dynamics.PhysBone.Components;
    using VRC.SDK3.Dynamics.Contact.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Better-AvatarPedestal#dynamics-parameter-transfer")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Better AvatarPedestal/Dynamics Parameter Transfer")]
    [RequireComponent(typeof(Animator))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DynamicsParameterTransfer : UdonSharpBehaviour
    {
        private Animator _animator;
        private VRCPhysBone[] _physbones = new VRCPhysBone[0];
        //private VRCContactReceiver[] _receivers = new VRCContactReceiver[0];

        private VRCPhysBone[] _pbs_IsGrabbed = new VRCPhysBone[0];
        private int[] _pbs_IsGrabbedHash = new int[0];
        private VRCPhysBone[] _pbs_IsPosed = new VRCPhysBone[0];
        private int[] _pbs_IsPosedHash = new int[0];
        private VRCPhysBone[] _pbs_Angle = new VRCPhysBone[0];
        private int[] _pbs_AngleHash = new int[0];
        private VRCPhysBone[] _pbs_Stretch = new VRCPhysBone[0];
        private int[] _pbs_StretchHash = new int[0];
        private VRCPhysBone[] _pbs_Squish = new VRCPhysBone[0];
        private int[] _pbs_SquishHash = new int[0];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _animator = GetComponent<Animator>();
            _physbones = GetComponentsInChildren<VRCPhysBone>(true);
            //_receivers = GetComponentsInChildren<VRCContactReceiver>(true);

            _pbs_IsGrabbed = GetPhysbonesInAnimatorParameter("_IsGrabbed", out _pbs_IsGrabbedHash);
            _pbs_IsPosed = GetPhysbonesInAnimatorParameter("_IsPosed", out _pbs_IsPosedHash);
            _pbs_Angle = GetPhysbonesInAnimatorParameter("_Angle", out _pbs_AngleHash);
            _pbs_Stretch = GetPhysbonesInAnimatorParameter("_Angle", out _pbs_StretchHash);
            _pbs_Squish = GetPhysbonesInAnimatorParameter("_Angle", out _pbs_SquishHash);


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
                var isGrabbed = _pbs_IsGrabbed[i].IsGrabbed;
                if (_animator.GetBool(_pbs_IsGrabbedHash[i]) != isGrabbed)
                {
                    _animator.SetBool(_pbs_IsGrabbedHash[i], isGrabbed);
                }
            }
            for (int i = 0; i < _pbs_IsPosed.Length; i++)
            {
                var isPosed = _pbs_IsPosed[i].IsPosed;
                if (_animator.GetBool(_pbs_IsPosedHash[i]) != isPosed)
                {
                    _animator.SetBool(_pbs_IsPosedHash[i], isPosed);
                }
            }
            for (int i = 0; i < _pbs_Angle.Length; i++)
            {
                var angle = _pbs_Angle[i].Angle;
                if (Mathf.Approximately(_animator.GetFloat(_pbs_AngleHash[i]), angle))
                {
                    _animator.SetFloat(_pbs_AngleHash[i], angle);
                }
            }
            for (int i = 0; i < _pbs_Stretch.Length; i++)
            {
                var stretch = _pbs_Stretch[i].Stretch;
                if (Mathf.Approximately(_animator.GetFloat(_pbs_StretchHash[i]), stretch))
                {
                    _animator.SetFloat(_pbs_StretchHash[i], stretch);
                }
            }
            for (int i = 0; i < _pbs_Squish.Length; i++)
            {
                var squish = _pbs_Squish[i].Squish;
                if (Mathf.Approximately(_animator.GetFloat(_pbs_SquishHash[i]), squish))
                {
                    _animator.SetFloat(_pbs_SquishHash[i], squish);
                }
            }

            // Contacts
        }

        private VRCPhysBone[] GetPhysbonesInAnimatorParameter(string suffix, out int[] physboneHashes)
        {
            var referencePhysboneHashes = new int[_physbones.Length];
            for (int i = 0; i < _physbones.Length; i++)
            {
                var pbName = _physbones[i] ? (_physbones[i].gameObject.name + suffix) : "";
                referencePhysboneHashes[i] = Animator.StringToHash(pbName);
            }

            var parameters = _animator.parameters;
            var parameterCount = _animator.parameterCount;

            var tmp_physbones = new VRCPhysBone[parameterCount];
            var tmp_physboneHashes = new int[parameterCount];
            var physboneCount = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                var hash = parameters[i].nameHash;
                var index = System.Array.IndexOf(referencePhysboneHashes, hash);
                if (index < 0) { continue; }

                tmp_physboneHashes[physboneCount] = hash;
                tmp_physbones[physboneCount] = _physbones[index];
                physboneCount++;
            }
            var physbones = new VRCPhysBone[physboneCount];
            physboneHashes = new int[physboneCount];
            tmp_physbones.CopyTo(physbones, 0);
            tmp_physboneHashes.CopyTo(physboneHashes, 0);

            return physbones;
        }
    }
}
