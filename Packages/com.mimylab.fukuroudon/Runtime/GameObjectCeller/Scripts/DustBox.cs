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
    //using VRC.Udon;
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/GameObject Celler/Dust Box")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DustBox : UdonSharpBehaviour
    {
        [SerializeField]
        private ObjectPoolManager target = null;

        private void OnTriggerEnter(Collider other)
        {
            if (!target) { return; }
            if (!Utilities.IsValid(other)) { return; }

            var incommingObject = other.gameObject;
            if (!Networking.IsOwner(incommingObject)) { return; }

            Networking.SetOwner(Networking.LocalPlayer, target.gameObject);
            GameObject pooledObject = _FindInParentPools(incommingObject);
            if (!Utilities.IsValid(pooledObject)) { return; }
            target.Return(pooledObject);
        }
        
        // 自身や先祖がPoolにあればそれを返す
	    private GameObject _FindInParentPools(GameObject current)
	    {
	        while (current != null)
	        {
	            foreach (GameObject pooledObject in target.Pool)
	            {
	                if (current == pooledObject)
	                {
	                    return pooledObject;
	                }
	            }
	            Transform parentTransform = current.transform.parent;
	            if (parentTransform == null) { return null; }
	            current = parentTransform.gameObject;
	        }
	        return null;
	    }
    }
}
