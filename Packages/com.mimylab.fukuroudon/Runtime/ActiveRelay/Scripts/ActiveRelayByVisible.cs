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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-by-visible")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay by Visible")]
    [RequireComponent(typeof(Renderer))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayByVisible : ActiveRelayBy
    {
        [SerializeField]
        private ActiveRelayVisibleType _eventType = default;

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
