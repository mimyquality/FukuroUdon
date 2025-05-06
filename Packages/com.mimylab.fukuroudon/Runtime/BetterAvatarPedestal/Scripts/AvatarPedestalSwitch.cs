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
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/General/AvatarPedestal Switch")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarPedestalSwitch : UdonSharpBehaviour
    {
        [SerializeField]
        private VRCAvatarPedestal avatarPedestal;

        private void Reset()
        {
            avatarPedestal = GetComponentInChildren<VRCAvatarPedestal>();
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!avatarPedestal) { avatarPedestal = GetComponentInChildren<VRCAvatarPedestal>(); }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void Interact()
        {
            Initialize();

            if (avatarPedestal)
            {
                avatarPedestal.SetAvatarUse(Networking.LocalPlayer);
            }
        }
    }
}
