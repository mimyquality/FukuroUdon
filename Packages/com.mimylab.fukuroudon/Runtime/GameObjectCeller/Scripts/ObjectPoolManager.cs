/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
//using VRC.SDKBase;
using VRC.SDK3.Components;
//using VRC.Udon;

namespace MimyLab
{
    [AddComponentMenu("Fukuro Udon/GameObject Celler/ObjectPool Manager")]
    [RequireComponent(typeof(VRCObjectPool))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectPoolManager : UdonSharpBehaviour
    {
        VRCObjectPool _objectPool;
        GameObject[] _pool;
        GameObject _lastSpawnedObject = null;
        bool _initialized = false;

        void Initialize()
        {
            if (_initialized) { return; }

            _objectPool = GetComponent<VRCObjectPool>();
            _pool = _objectPool.Pool;

            _initialized = true;
        }
        void Start()
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
                return _objectPool.Pool;
            }
        }
        public Vector3[] StartPositions
        {
            get
            {
                Initialize();
                return _objectPool.StartPositions;
            }
        }
        public Quaternion[] StartRotations
        {
            get
            {
                Initialize();
                return _objectPool.StartRotations;
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

            GameObject resultObject;
            resultObject = _objectPool.TryToSpawn();
            if (resultObject) { _lastSpawnedObject = resultObject; }

            return resultObject;
        }

        public void Return(GameObject gameObject)
        {
            Initialize();

            _objectPool.Return(gameObject);
        }

        public void Shuffle()
        {
            Initialize();

            _objectPool.Shuffle();
        }

        /******************************
         Extend Method
         ******************************/
        public GameObject[] SpawnAll()
        {
            Initialize();

            GameObject resultObject;
            GameObject[] spawnObjects = new GameObject[_pool.Length];
            int spawnCount = 0;
            for (int i = 0; i < _pool.Length; i++)
            {
                resultObject = _objectPool.TryToSpawn();
                if (resultObject)
                {
                    _lastSpawnedObject = resultObject;
                    spawnObjects[spawnCount] = resultObject;
                    spawnCount++;
                }
                else
                {
                    break;
                }
            }

            // スポーンしたオブジェクトだけの配列を作る(色々と使えないので原始的にやる)
            GameObject[] returnObjects = new GameObject[spawnCount];
            System.Array.Copy(spawnObjects, returnObjects, spawnCount);

            return returnObjects;
        }

        // Poolの頭の方から戻す
        public void Return()
        {
            Initialize();

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

            _objectPool.Return(_pool[index]);
        }

        public void ReturnAll()
        {
            Initialize();

            for (int i = 0; i < _pool.Length; i++)
            {
                _objectPool.Return(_pool[i]);
            }
        }
    }
}
