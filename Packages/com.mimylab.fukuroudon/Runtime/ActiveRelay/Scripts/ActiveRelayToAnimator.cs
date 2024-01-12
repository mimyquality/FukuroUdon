/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;

    public enum ActiveRelayToAnimatorEventType
    {
        Active,
        Inactive,
        ActiveAndInactive,
    }

    [AddComponentMenu("Fukuro Udon/Active Relay/Active Relay to Animator")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayToAnimator : UdonSharpBehaviour
    {
        [SerializeField]
        ActiveRelayToAnimatorEventType _eventType = default;
        [SerializeField]
        Animator _animator = null;

        [Header("Set Animator parameters by event")]
        [SerializeField]
        string _triggerName = "";
        [SerializeField]
        bool _resetTrigger = false;

        [Space]
        [SerializeField]
        string _boolName = "";
        [SerializeField]
        bool _boolValue = false;

        [Space]
        [SerializeField]
        string _intName = "";
        [SerializeField]
        int _intValue = 0;

        [Space]
        [SerializeField]
        string _floatName = "";
        [SerializeField]
        float _floatValue = 0.0f;

        // パラメーター名ハッシュのキャッシュ用
        int _triggerNameHash, _boolNameHash, _intNameHash, _floatNameHash;

        bool _initialized = false;
        void Initialize()
        {
            if (_initialized) { return; }

            _triggerNameHash = Animator.StringToHash(_triggerName);
            _boolNameHash = Animator.StringToHash(_boolName);
            _intNameHash = Animator.StringToHash(_intName);
            _floatNameHash = Animator.StringToHash(_floatName);

            _initialized = true;
        }

        void OnEnable()
        {
            Initialize();

            if (_eventType == ActiveRelayToAnimatorEventType.ActiveAndInactive
             || _eventType == ActiveRelayToAnimatorEventType.Active)
            {
                TrySetValue();
            }
        }

        void OnDisable()
        {
            if (_eventType == ActiveRelayToAnimatorEventType.ActiveAndInactive
             || _eventType == ActiveRelayToAnimatorEventType.Inactive)
            {
                TrySetValue();
            }
        }

        void TrySetValue()
        {
            if (_animator)
            {
                if (_triggerName != "")
                {
                    if (_resetTrigger) { _animator.ResetTrigger(_triggerNameHash); }
                    else { _animator.SetTrigger(_triggerNameHash); }
                }
                if (_boolName != "") { _animator.SetBool(_boolNameHash, _boolValue); }
                if (_intName != "") { _animator.SetInteger(_intNameHash, _intValue); }
                if (_floatName != "") { _animator.SetFloat(_floatNameHash, _floatValue); }
            }
        }
    }
}
