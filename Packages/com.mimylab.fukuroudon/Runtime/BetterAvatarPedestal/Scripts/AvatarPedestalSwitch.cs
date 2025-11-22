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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Better-AvatarPedestal#%E4%BD%BF%E3%81%84%E6%96%B9")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Better AvatarPedestal/AvatarPedestal Switch")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarPedestalSwitch : UdonSharpBehaviour
    {
        [SerializeField]
        private VRCAvatarPedestal avatarPedestal;

        private void Reset()
        {
            avatarPedestal = GetComponentInChildren<VRCAvatarPedestal>(true);
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!avatarPedestal) { avatarPedestal = GetComponentInChildren<VRCAvatarPedestal>(true); }

            _initialized = true;
        }

        public override void Interact()
        {
            SetAvatarUse();
        }

        public void SetAvatarUse()
        {
            Initialize();

            if (avatarPedestal)
            {
                avatarPedestal.SetAvatarUse(Networking.LocalPlayer);
            }
        }
    }
}
