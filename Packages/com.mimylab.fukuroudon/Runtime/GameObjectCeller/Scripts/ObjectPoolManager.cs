/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Components;
    using VRC.SDK3.UdonNetworkCalling;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/ObjectPool Manager")]
    [RequireComponent(typeof(VRCObjectPool))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectPoolManager : UdonSharpBehaviour
    {
        private VRCObjectPool _objectPool;
        private GameObject[] _pool;
        private Vector3[] _startPositions;
        private Quaternion[] _startRotations;
        private GameObject _lastSpawnedObject = null;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _objectPool = GetComponent<VRCObjectPool>();
            _pool = _objectPool.Pool;
            // VRCObjectPoolのStartPositions, StartRotationsがバグってるので自前で初期化
            _startPositions = new Vector3[_pool.Length];
            _startRotations = new Quaternion[_pool.Length];
            for (int i = 0; i < _pool.Length; i++)
            {
                _startPositions[i] = _pool[i].transform.position;
                _startRotations[i] = _pool[i].transform.rotation;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        /******************************
         VRCObjectPool Alies Property
         ******************************/
        public GameObject[] Pool
        {
            get
            {
                Initialize();

                return _pool;
            }
        }
        public Vector3[] StartPositions
        {
            get
            {
                Initialize();

                return _startPositions;
            }
        }
        public Quaternion[] StartRotations
        {
            get
            {
                Initialize();

                return _startRotations;
            }
        }

        /******************************
         Extend Property
         ******************************/
        public GameObject LastSpawnedObject { get => _lastSpawnedObject; }

        public int CountAll
        {
            get
            {
                Initialize();

                int count = 0;
                for (int i = 0; i < _pool.Length; i++)
                {
                    if (_pool[i]) { count++; }
                }
                return count;
            }
        }

        public int CountActive
        {
            get
            {
                Initialize();

                int count = 0;
                for (int i = 0; i < _pool.Length; i++)
                {
                    if (_pool[i] && _pool[i].activeSelf) { count++; }
                }
                return count;
            }
        }

        public int CountInactive
        {
            get
            {
                Initialize();

                int count = 0;
                for (int i = 0; i < _pool.Length; i++)
                {
                    if (_pool[i] && !_pool[i].activeSelf) { count++; }
                }
                return count;
            }
        }

        /******************************
         VRCObjectPool Alies Method
         ******************************/
        public GameObject TryToSpawn()
        {
            Initialize();

            if (!Networking.IsOwner(this.gameObject)) { return null; }

            var resultObject = _objectPool.TryToSpawn();
            if (resultObject) { _lastSpawnedObject = resultObject; }

            return resultObject;
        }

        public void Return(GameObject gameObject)
        {
            Initialize();

            if (!Networking.IsOwner(this.gameObject)) { return; }

            _objectPool.Return(gameObject);
        }

        public void Shuffle()
        {
            Initialize();

            if (!Networking.IsOwner(this.gameObject)) { return; }

            _objectPool.Shuffle();
        }

        /******************************
         Extend Method
         ******************************/
        public GameObject[] SpawnAll()
        {
            Initialize();

            if (!Networking.IsOwner(this.gameObject)) { return new GameObject[0]; }

            var spawnObjects = new GameObject[_pool.Length];
            var spawnCount = 0;
            for (int i = 0; i < _pool.Length; i++)
            {
                var resultObject = _objectPool.TryToSpawn();
                if (!resultObject) { break; }

                _lastSpawnedObject = resultObject;
                spawnObjects[spawnCount++] = resultObject;
            }

            // スポーンしたオブジェクトだけの配列を作る(色々と使えないので原始的にやる)
            var returnObjects = new GameObject[spawnCount];
            System.Array.Copy(spawnObjects, returnObjects, spawnCount);

            return returnObjects;
        }

        // Poolの頭の方から戻す
        public void Return()
        {
            Initialize();

            if (!Networking.IsOwner(this.gameObject)) { return; }

            for (int i = 0; i < _pool.Length; i++)
            {
                if (_pool[i].activeSelf)
                {
                    _objectPool.Return(_pool[i]);
                    break;
                }
            }
        }

        public void Return(int index)
        {
            Initialize();

            if (!Networking.IsOwner(this.gameObject)) { return; }

            _objectPool.Return(_pool[index]);
        }

        public void ReturnAll()
        {
            Initialize();

            if (!Networking.IsOwner(this.gameObject)) { return; }

            for (int i = 0; i < _pool.Length; i++)
            {
                _objectPool.Return(_pool[i]);
            }
        }

        /******************************
         For SendCustomnetworkEvent Method
         ******************************/
        [NetworkCallable]
        public void CallTryToSpawn() { TryToSpawn(); }

        [NetworkCallable]
        public void CallReturnFirst() { Return(); }

        [NetworkCallable]
        public void CallReturn(int index) { Return(index); }

        [NetworkCallable]
        public void CallShuffle() { Shuffle(); }

        [NetworkCallable]
        public void CallSpawnAll() { SpawnAll(); }

        [NetworkCallable]
        public void CallReturnAll() { ReturnAll(); }
    }
}
