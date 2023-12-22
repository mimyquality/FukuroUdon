/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceChannelSelecter : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerAudioRegulatorList[] _targetRegulator;
        [SerializeField]
        private GameObject[] _buttonChannelOFF;
        [SerializeField]
        private GameObject[] _buttonChannelON;

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

        public void Assign0() { Assign(0); }
        public void Assign1() { Assign(1); }
        public void Assign2() { Assign(2); }
        public void Assign3() { Assign(3); }
        public void Assign4() { Assign(4); }
        public void Assign5() { Assign(5); }
        public void Assign6() { Assign(6); }
        public void Assign7() { Assign(7); }
        public void Assign8() { Assign(8); }
        public void Assign9() { Assign(9); }
        public void Assign(int num)
        {
            for (int i = 0; i < _targetRegulator.Length; i++)
            {
                Networking.SetOwner(_localPlayer, _targetRegulator[i].gameObject);
                if (i == num)
                {
                    AssignSelectChannel(i);
                }
                else
                {
                    ReleaseSelectChannel(i);
                }
            }
        }

        public void Release0() { Release(0); }
        public void Release1() { Release(1); }
        public void Release2() { Release(2); }
        public void Release3() { Release(3); }
        public void Release4() { Release(4); }
        public void Release5() { Release(5); }
        public void Release6() { Release(6); }
        public void Release7() { Release(7); }
        public void Release8() { Release(8); }
        public void Release9() { Release(9); }
        public void Release(int num)
        {
            Networking.SetOwner(_localPlayer, _targetRegulator[num].gameObject);
            ReleaseSelectChannel(num);
        }

        private void AssignSelectChannel(int num)
        {
            _targetRegulator[num].AssignPlayer(_localPlayer);
            _buttonChannelOFF[num].SetActive(false);
            _buttonChannelON[num].SetActive(true);
        }

        private void ReleaseSelectChannel(int num)
        {
            _targetRegulator[num].ReleasePlayer(_localPlayer);
            _buttonChannelOFF[num].SetActive(true);
            _buttonChannelON[num].SetActive(false);
        }
    }
}
