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
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PAR Register Player")]
    [RequireComponent(typeof(VRCPlayerObject))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PARRegisterPlayer : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerAudioRegulatorRegister _parRegister;

        private VRCPlayerApi _owner;

        [UdonSynced, FieldChangeCallback(nameof(IsAssigned))]
        private bool _isAssigned = false;
        public bool IsAssigned
        {
            get => _isAssigned;
            set
            {
                if (_isAssigned == value) { return; }

                Initialize();

                _isAssigned = value;

                if (value)
                {
                    _parRegister._OnPlayerAssigned(_owner);
                }
                else
                {
                    _parRegister._OnPlayerReleased(_owner);
                }
            }
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            // PlayerObjectはオーナー不変なのでキャッシュ
            _owner = Networking.GetOwner(this.gameObject);
            
            if (_owner.isLocal)
            {
                _parRegister.localParRegisterPlayer = this;
            }

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }
    }
}
