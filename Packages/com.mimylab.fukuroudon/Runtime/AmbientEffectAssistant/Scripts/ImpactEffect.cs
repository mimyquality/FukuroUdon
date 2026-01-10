/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;

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
        [SerializeField, Range(1, 256)]
        private int _poolSize = 10;
        [SerializeField]
        private Vector3 _normal = Vector3.up;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _effectTime = 3.0f;

        private Collider _collider;
        private int _spawnCount = 0;
        private int _returnCount = 0;
        private int[] _spawnedEffects = new int[0];
        private GameObject[] _effectPool = new GameObject[0];
        private GameObject[] _highEffectPool = new GameObject[0];
        private Transform[] _effectTransforms = new Transform[0];
        private Transform[] _highEffectTransforms = new Transform[0];

        private bool _isNormalCheck;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _collider = GetComponent<Collider>();

            _spawnedEffects = new int[_poolSize];
            _effectPool = new GameObject[_poolSize];
            _effectTransforms = new Transform[_poolSize];
            for (int i = 0; i < _effectPool.Length; i++)
            {
                _effectPool[i] = Instantiate(_effectPrefab);
                _effectPool[i].SetActive(false);
                _effectTransforms[i] = _effectPool[i].transform;
            }

            if (_highEffectPrefab)
            {
                _highEffectPool = new GameObject[_poolSize];
                _highEffectTransforms = new Transform[_poolSize];
                for (int i = 0; i < _highEffectPool.Length; i++)
                {
                    _highEffectPool[i] = Instantiate(_highEffectPrefab);
                    _highEffectPool[i].SetActive(false);
                    _highEffectTransforms[i] = _highEffectPool[i].transform;
                }
            }

            _isNormalCheck = _normal != Vector3.zero;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        protected void PlayEffect(Vector3 collideePosition, Vector3 collideeVelocity)
        {
            Initialize();

            // 進入方向と速度のバリデーション
            if (_isNormalCheck)
            {
                Vector3 collideNormal = this.transform.TransformDirection(_normal);
                if (Vector3.Dot(collideNormal, collideeVelocity) >= 0.0f) { return; }
            }

            // SpawnEffect() か SpawnHighEffect() のどちらかだけ実行
            float collideeSpeed = collideeVelocity.sqrMagnitude;
            if (_highEffectPrefab)
            {
                if (collideeSpeed > _highImpactSpeed * _highImpactSpeed)
                {
                    SpawnHighEffect(_collider.ClosestPoint(collideePosition));
                    return;
                }
            }

            if (collideeSpeed > _impactSpeed * _impactSpeed)
            {
                SpawnEffect(_collider.ClosestPoint(collideePosition));
                return;
            }
        }

        private void SpawnEffect(Vector3 position)
        {
            int index = System.Array.IndexOf(_spawnedEffects, 0);
            if (index < 0) { return; }

            _spawnCount = (_spawnCount < _poolSize) ? _spawnCount + 1 : 1;
            _spawnedEffects[index] = _spawnCount;

            _effectTransforms[index].position = position;
            _effectPool[index].SetActive(true);

            SendCustomEventDelayedSeconds(nameof(_ReturnEffect), _effectTime);
        }

        private void SpawnHighEffect(Vector3 position)
        {
            int index = System.Array.IndexOf(_spawnedEffects, 0);
            if (index < 0) { return; }

            _spawnCount = (_spawnCount < _poolSize) ? _spawnCount + 1 : 1;
            _spawnedEffects[index] = _spawnCount;

            _highEffectTransforms[index].position = position;
            _highEffectPool[index].SetActive(true);

            SendCustomEventDelayedSeconds(nameof(_ReturnEffect), _effectTime);
        }

        public void _ReturnEffect()
        {
            _returnCount = (_returnCount < _poolSize) ? _returnCount + 1 : 1;
            int index = System.Array.IndexOf(_spawnedEffects, _returnCount);
            if (index < 0) { return; }

            _spawnedEffects[index] = 0;
            _effectPool[index].SetActive(false);
            if (_highEffectPrefab) { _highEffectPool[index].SetActive(false); }
        }
    }
}
