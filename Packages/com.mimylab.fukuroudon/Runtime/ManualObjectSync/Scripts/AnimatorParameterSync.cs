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
    using VRC.Udon.Common;

    public enum AnimatorParameterSyncSmoothingMode
    {
        None,
        Linear,
        Smooth
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#animator-parameter-sync")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Manual ObjectSync/Animator Parameter Sync")]
    [RequireComponent(typeof(Animator))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AnimatorParameterSync : UdonSharpBehaviour
    {
        private const float ParameterCheckTickRate = 0.1f;  // 10Hz
        private const float SmoothingDuration = 0.2f;

        [SerializeField]
        private string[] _parameterNames = new string[0];
        [SerializeField, Tooltip("smoothing is only float parameters")]
        private AnimatorParameterSyncSmoothingMode _smoothingMode = AnimatorParameterSyncSmoothingMode.None;

        [SerializeField, HideInInspector]
        private int[] _parameterHashes = new int[0];

        [UdonSynced]
        private int[] sync_boolParameterHashes = new int[0];
        [UdonSynced]
        private bool[] sync_boolParameterValues = new bool[0];
        [UdonSynced]
        private int[] sync_intParameterHashes = new int[0];
        [UdonSynced]
        private int[] sync_intParameterValues = new int[0];
        [UdonSynced]
        private int[] sync_floatParameterHashes = new int[0];
        [UdonSynced]
        private float[] sync_floatParameterValues = new float[0];

        private Animator _animator;
        private int[] _boolParameterHashes = new int[0];
        private bool[] _boolParameterValues = new bool[0];
        private int[] _intParameterHashes = new int[0];
        private int[] _intParameterValues = new int[0];
        private int[] _floatParameterHashes = new int[0];
        private float[] _floatParameterValues = new float[0];
        private float _elapsedTime = SmoothingDuration;

        private float _checkTiming;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            if (_parameterHashes.Length != _parameterNames.Length)
            {
                _parameterHashes = new int[_parameterNames.Length];
            }
            for (int i = 0; i < _parameterNames.Length; i++)
            {
                _parameterHashes[i] = Animator.StringToHash(_parameterNames[i]);
            }
        }
#endif

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _animator = GetComponent<Animator>();

            var parameters = _animator.parameters;
            var parameterCount = _animator.parameterCount;

            var tmp_boolParameterHashes = new int[parameterCount];
            var boolCount = 0;
            var tmp_intParameterHashes = new int[parameterCount];
            var intCount = 0;
            var tmp_floatParameterHashes = new int[parameterCount];
            var floatCount = 0;
            for (int i = 0; i < parameterCount; i++)
            {
                var hash = parameters[i].nameHash;
                if (System.Array.IndexOf(_parameterHashes, hash) < 0) { continue; }

                switch (parameters[i].type)
                {
                    case AnimatorControllerParameterType.Bool: tmp_boolParameterHashes[boolCount++] = hash; break;
                    case AnimatorControllerParameterType.Int: tmp_intParameterHashes[intCount++] = hash; break;
                    case AnimatorControllerParameterType.Float: tmp_floatParameterHashes[floatCount++] = hash; break;
                }
            }
            _boolParameterHashes = new int[boolCount];
            _boolParameterValues = new bool[boolCount];
            System.Array.Copy(tmp_boolParameterHashes, _boolParameterHashes, boolCount);
            _intParameterHashes = new int[intCount];
            _intParameterValues = new int[intCount];
            System.Array.Copy(tmp_intParameterHashes, _intParameterHashes, intCount);
            _floatParameterHashes = new int[floatCount];
            _floatParameterValues = new float[floatCount];
            System.Array.Copy(tmp_floatParameterHashes, _floatParameterHashes, floatCount);

            _checkTiming = Time.time + ParameterCheckTickRate + (Random.value * ParameterCheckTickRate);

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (Networking.IsOwner(this.gameObject))
            {
                if (_checkTiming > Time.time) { return; }
                _checkTiming = Time.time + ParameterCheckTickRate;

                if (Networking.IsClogged) { return; }

                if (CheckAnimatorParameterChange())
                {
                    RequestSerialization();
                }
            }
            else
            {
                SmoothingFloat();
            }
        }

        public override void OnPreSerialization()
        {
            if (sync_boolParameterHashes.Length != _boolParameterHashes.Length)
            {
                sync_boolParameterHashes = new int[_boolParameterHashes.Length];
                sync_boolParameterValues = new bool[_boolParameterValues.Length];
            }
            if (_boolParameterHashes.Length > 0)
            {
                _boolParameterHashes.CopyTo(sync_boolParameterHashes, 0);
                _boolParameterValues.CopyTo(sync_boolParameterValues, 0);
            }

            if (sync_intParameterHashes.Length != _intParameterHashes.Length)
            {
                sync_intParameterHashes = new int[_intParameterHashes.Length];
                sync_intParameterValues = new int[_intParameterValues.Length];
            }
            if (_intParameterHashes.Length > 0)
            {
                _intParameterHashes.CopyTo(sync_intParameterHashes, 0);
                _intParameterValues.CopyTo(sync_intParameterValues, 0);
            }

            if (sync_floatParameterHashes.Length != _floatParameterHashes.Length)
            {
                sync_floatParameterHashes = new int[_floatParameterHashes.Length];
                sync_floatParameterValues = new float[_floatParameterValues.Length];
            }
            if (_floatParameterHashes.Length > 0)
            {
                _floatParameterHashes.CopyTo(sync_floatParameterHashes, 0);
                _floatParameterValues.CopyTo(sync_floatParameterValues, 0);
            }
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            Initialize();

            for (int i = 0; i < _boolParameterHashes.Length; i++)
            {
                var index = System.Array.IndexOf(sync_boolParameterHashes, _boolParameterHashes[i]);
                if (index < 0) { continue; }

                if (_animator.GetBool(_boolParameterHashes[i]) != sync_boolParameterValues[index])
                {
                    _boolParameterValues[i] = sync_boolParameterValues[index];
                    _animator.SetBool(_boolParameterHashes[i], sync_boolParameterValues[index]);
                }
            }

            for (int i = 0; i < _intParameterHashes.Length; i++)
            {
                var index = System.Array.IndexOf(sync_intParameterHashes, _intParameterHashes[i]);
                if (index < 0) { continue; }

                if (_animator.GetInteger(_intParameterHashes[i]) != sync_intParameterValues[index])
                {
                    _intParameterValues[i] = sync_intParameterValues[index];
                    _animator.SetInteger(_intParameterHashes[i], sync_intParameterValues[index]);
                }
            }

            for (int i = 0; i < _floatParameterHashes.Length; i++)
            {
                var index = System.Array.IndexOf(sync_floatParameterHashes, _floatParameterHashes[i]);
                if (index < 0) { continue; }

                var animatorFloatParameter = _animator.GetFloat(_floatParameterHashes[i]);
                if (Mathf.Approximately(animatorFloatParameter, sync_floatParameterValues[index])) { continue; }

                // スムージング処理ありなら Update() でやるので、ここでは行わない
                if (_smoothingMode == AnimatorParameterSyncSmoothingMode.None)
                {
                    _floatParameterValues[i] = sync_floatParameterValues[index];
                    _animator.SetFloat(_floatParameterHashes[i], sync_floatParameterValues[index]);
                }
                else
                {
                    _floatParameterValues[i] = animatorFloatParameter;
                    _elapsedTime = 0.0f;
                }
            }
        }

        private bool CheckAnimatorParameterChange()
        {
            var result = false;

            for (int i = 0; i < _boolParameterHashes.Length; i++)
            {
                var boolValue = _animator.GetBool(_boolParameterHashes[i]);
                if (_boolParameterValues[i] == boolValue) { continue; }

                _boolParameterValues[i] = boolValue;
                result = true;
            }

            for (int i = 0; i < _intParameterHashes.Length; i++)
            {
                var intValue = _animator.GetInteger(_intParameterHashes[i]);
                if (_intParameterValues[i] == intValue) { continue; }

                _intParameterValues[i] = intValue;
                result = true;
            }

            for (int i = 0; i < _floatParameterHashes.Length; i++)
            {
                var floatValue = _animator.GetFloat(_floatParameterHashes[i]);
                if (Mathf.Approximately(_floatParameterValues[i], floatValue)) { continue; }

                _floatParameterValues[i] = floatValue;
                result = true;
            }

            return result;
        }

        private void SmoothingFloat()
        {
            if (_elapsedTime >= SmoothingDuration) { return; }
            _elapsedTime += Time.deltaTime;

            var t = _elapsedTime / SmoothingDuration;
            for (int i = 0; i < _floatParameterHashes.Length; i++)
            {
                var index = System.Array.IndexOf(sync_floatParameterHashes, _floatParameterHashes[i]);
                if (index < 0) { continue; }

                var currentFloatValue = _floatParameterValues[i];
                switch (_smoothingMode)
                {
                    case AnimatorParameterSyncSmoothingMode.Linear:
                        currentFloatValue = Mathf.Lerp(_floatParameterValues[i], sync_floatParameterValues[index], t);
                        break;
                    case AnimatorParameterSyncSmoothingMode.Smooth:
                        currentFloatValue = Mathf.SmoothStep(_floatParameterValues[i], sync_floatParameterValues[index], t);
                        break;
                }
                _animator.SetFloat(_floatParameterHashes[i], currentFloatValue);
            }
        }
    }
}
