/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [RequireComponent(typeof(Collider))]
    public class ImpactEffect : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject _effectPrefab;
        [SerializeField, Min(0.0f), Tooltip("m/s")]
        private float _impactSpeed = 1.0f;

        [Header("Option")]
        [SerializeField]
        private GameObject _highEffectPrefab;
        [SerializeField, Min(0.0f), Tooltip("m/s")]
        private float _highImpactSpeed = 3.0f;

        [Header("Settings")]
        [SerializeField, Range(1, 32)]
        private int _poolSize = 10;
        [SerializeField]
        private Vector3 _normal = Vector3.up;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _effectTime = 3.0f;

        private Collider _collider;
        private GameObject[] _effectPool, _highEffectPool;
        private Transform[] _effectsTransform, _highEffectsTransform;
        private bool[] _effectsActive;

        private bool _isNormalCheck;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _collider = GetComponent<Collider>();

            _effectPool = new GameObject[_poolSize];
            _highEffectPool = new GameObject[_poolSize];
            _effectsTransform = new Transform[_poolSize];
            _highEffectsTransform = new Transform[_poolSize];
            _effectsActive = new bool[_poolSize];
            for (int i = 0; i < _effectPool.Length; i++)
            {
                _effectsActive[i] = false;
                _effectPool[i] = Instantiate(_effectPrefab);
                _effectPool[i].SetActive(_effectsActive[i]);
                _effectsTransform[i] = _effectPool[i].transform;
            }

            if (_highEffectPrefab)
            {
                for (int i = 0; i < _highEffectPool.Length; i++)
                {
                    _highEffectPool[i] = Instantiate(_highEffectPrefab);
                    _highEffectPool[i].SetActive(_effectsActive[i]);
                    _highEffectsTransform[i] = _highEffectPool[i].transform;
                }
            }

            _isNormalCheck = _normal != Vector3.zero;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        protected virtual void PlayEffect(Vector3 collideePosition, Vector3 collideeVelocity)
        {
            Initialize();

            // 進入方向と速度のバリデーション
            var localNormal = this.transform.TransformDirection(_normal);
            if (_isNormalCheck && Vector3.Angle(localNormal, collideeVelocity) <= 90.0f) { return; }

            var collideeSpeed = collideeVelocity.sqrMagnitude;
            if (_highEffectPrefab && _highImpactSpeed * _highImpactSpeed <= collideeSpeed)
            {
                SpawnHighEffect(_collider.ClosestPoint(collideePosition));
                return;
            }

            if (_impactSpeed * _impactSpeed <= collideeSpeed)
            {
                SpawnEffect(_collider.ClosestPoint(collideePosition));
                return;
            }
        }

        private bool SpawnEffect(Vector3 position)
        {
            for (int i = 0; i < _effectPool.Length; i++)
            {
                if (!_effectsActive[i])
                {
                    _effectsTransform[i].position = position;
                    _effectsActive[i] = true;
                    _effectPool[i].SetActive(_effectsActive[i]);
                    SendCustomEventDelayedSeconds("_ReturnEffect" + i.ToString(), _effectTime);
                    return true;
                }
            }

            return false;
        }

        private bool SpawnHighEffect(Vector3 position)
        {
            for (int i = 0; i < _highEffectPool.Length; i++)
            {
                if (!_effectsActive[i])
                {
                    _highEffectsTransform[i].position = position;
                    _effectsActive[i] = true;
                    _highEffectPool[i].SetActive(_effectsActive[i]);
                    SendCustomEventDelayedSeconds("_ReturnEffect" + i.ToString(), _effectTime);
                    return true;
                }
            }

            return false;
        }

        private void ReturnEffect(int index)
        {
            _effectsActive[index] = false;
            _effectPool[index].SetActive(_effectsActive[index]);
            if (_highEffectPrefab) { _highEffectPool[index].SetActive(_effectsActive[index]); }
        }

        public void _ReturnEffect0() { ReturnEffect(0); }
        public void _ReturnEffect1() { ReturnEffect(1); }
        public void _ReturnEffect2() { ReturnEffect(2); }
        public void _ReturnEffect3() { ReturnEffect(3); }
        public void _ReturnEffect4() { ReturnEffect(4); }
        public void _ReturnEffect5() { ReturnEffect(5); }
        public void _ReturnEffect6() { ReturnEffect(6); }
        public void _ReturnEffect7() { ReturnEffect(7); }
        public void _ReturnEffect8() { ReturnEffect(8); }
        public void _ReturnEffect9() { ReturnEffect(9); }
        public void _ReturnEffect10() { ReturnEffect(10); }
        public void _ReturnEffect11() { ReturnEffect(11); }
        public void _ReturnEffect12() { ReturnEffect(12); }
        public void _ReturnEffect13() { ReturnEffect(13); }
        public void _ReturnEffect14() { ReturnEffect(14); }
        public void _ReturnEffect15() { ReturnEffect(15); }
        public void _ReturnEffect16() { ReturnEffect(16); }
        public void _ReturnEffect17() { ReturnEffect(17); }
        public void _ReturnEffect18() { ReturnEffect(18); }
        public void _ReturnEffect19() { ReturnEffect(19); }
        public void _ReturnEffect20() { ReturnEffect(20); }
        public void _ReturnEffect21() { ReturnEffect(21); }
        public void _ReturnEffect22() { ReturnEffect(22); }
        public void _ReturnEffect23() { ReturnEffect(23); }
        public void _ReturnEffect24() { ReturnEffect(24); }
        public void _ReturnEffect25() { ReturnEffect(25); }
        public void _ReturnEffect26() { ReturnEffect(26); }
        public void _ReturnEffect27() { ReturnEffect(27); }
        public void _ReturnEffect28() { ReturnEffect(28); }
        public void _ReturnEffect29() { ReturnEffect(29); }
        public void _ReturnEffect30() { ReturnEffect(30); }
        public void _ReturnEffect31() { ReturnEffect(31); }
    }
}
