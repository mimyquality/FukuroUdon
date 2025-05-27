/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    
    public enum ActiveRelayVisibleType
    {
        BecameVisibleAndInvisible,
        BecameVisible,
        BecameInvisible,
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Visible")]
    [RequireComponent(typeof(Renderer))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByVisible : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayVisibleType _eventType = default;

// OnBecameVisible/Invisible がクライアント上で動かないので代わりに叩く
// CliendSim上だと動くので無効
#if COMPILER_UDONSHARP && !UNITY_EDITOR
        public void _onBecameVisible()
        {
            OnBecameVisible();
        }

        public void _onBecameInvisible()
        {
            OnBecameInvisible();
        }
#endif

        private void OnBecameVisible()
        {
            switch (_eventType)
            {
                case ActiveRelayVisibleType.BecameVisibleAndInvisible:
                case ActiveRelayVisibleType.BecameVisible:
                    DoAction(Networking.LocalPlayer);
                    break;
            }
        }

        private void OnBecameInvisible()
        {
            switch (_eventType)
            {
                case ActiveRelayVisibleType.BecameVisibleAndInvisible:
                case ActiveRelayVisibleType.BecameInvisible:
                    DoAction(Networking.LocalPlayer);
                    break;
            }
        }
    }
}
