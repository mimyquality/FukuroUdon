/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    public enum PlayerAudioRegulatorListControllerSwitchMode
    {
        Assign,
        Release,
        ReleaseAll
    }

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PAR List Controller")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PARListController : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerAudioRegulatorList targetList;
        [SerializeField]
        private PlayerAudioRegulatorListControllerSwitchMode switchMode;

        private VRCPlayerApi _localPlayer;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _localPlayer = Networking.LocalPlayer;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void Interact()
        {
            switch (switchMode)
            {
                case PlayerAudioRegulatorListControllerSwitchMode.Assign: Assign(); break;
                case PlayerAudioRegulatorListControllerSwitchMode.Release: Release(); break;
                case PlayerAudioRegulatorListControllerSwitchMode.ReleaseAll: ReleaseAll(); break;
            }
        }

        private void Assign()
        {
            Networking.SetOwner(_localPlayer, targetList.gameObject);

            targetList.AssignPlayer(_localPlayer);
        }

        private void Release()
        {
            Networking.SetOwner(_localPlayer, targetList.gameObject);

            targetList.ReleasePlayer(_localPlayer);
        }

        private void ReleaseAll()
        {
            Networking.SetOwner(_localPlayer, targetList.gameObject);

            targetList.ReleaseAllPlayer();
        }
    }
}
