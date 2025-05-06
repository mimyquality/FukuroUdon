/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    using VRC.SDK3.Components;
    //using VRC.Udon;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/ObjectPool Manager")]
    [RequireComponent(typeof(VRCObjectPool))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ObjectPoolManager : UdonSharpBehaviour
    {
        [Tooltip("Set a value upper than world Respawn Hight Y.")]
        public Vector3 respawnPoint = new Vector3(0f, -101f, 0f);
        
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

            _ReturnAndAllPickupObjectRespawn(gameObject);
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
                if (!resultObject) { break; }

                _lastSpawnedObject = resultObject;
                spawnObjects[spawnCount++] = resultObject;
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
                    _ReturnAndAllPickupObjectRespawn(_pool[i]);
                    break;
                }
            }
        }

        public void Return(int index)
        {
            Initialize();

            _ReturnAndAllPickupObjectRespawn(_pool[index]);
        }

        public void ReturnAll()
        {
            Initialize();

            for (int i = 0; i < _pool.Length; i++)
            {
                _ReturnAndAllPickupObjectRespawn(_pool[i]);
            }
        }
        
        private void _ReturnAndAllPickupObjectRespawn(GameObject obj)
        {
            if (!Utilities.IsValid(obj)) { return; }

            // 子孫オブジェクト(VRCPickup)の位置をリセットする
            VRCPickup[] childrenPickup = obj.GetComponentsInChildren<VRCPickup>(true);
            foreach (VRCPickup childPickup in childrenPickup)
            {
                _Respawn(childPickup.gameObject);
            }

            // objの位置をリセットする
            _Respawn(obj);

            // objを非表示にする
            _objectPool.Return(obj);

        }

        private void _Respawn(GameObject obj)
        {
            var objPickup = obj.GetComponent<VRCPickup>();
            var objObjectSync = obj.GetComponent<VRCObjectSync>();
            var objRigidBody = obj.GetComponent<Rigidbody>();
            if (objPickup) { objPickup.Drop(); }
            if (objObjectSync) { objObjectSync.FlagDiscontinuity(); }
            if (objRigidBody)
            {
                objRigidBody.velocity = Vector3.zero;
                objRigidBody.angularVelocity = Vector3.zero;
            }
            obj.transform.position = respawnPoint;
        }
    }
}
