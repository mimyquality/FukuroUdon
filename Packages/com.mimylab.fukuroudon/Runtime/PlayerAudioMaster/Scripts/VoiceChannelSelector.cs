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
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceChannelSelector : UdonSharpBehaviour
    {
        internal VoiceChannelPlayerStates localPlayerStates;

        [SerializeField]
        private PlayerAudioRegulatorList[] _targetRegulator = new PlayerAudioRegulatorList[0];
        [SerializeField]
        private GameObject[] _buttonChannelOFF = new GameObject[0];
        [SerializeField]
        private GameObject[] _buttonChannelON = new GameObject[0];
        [SerializeField]
        private Transform[] _channelSlot = new Transform[0];

        public void _OnPlayerStatesChange(VoiceChannelPlayerStates states)
        {
            var owner = Networking.GetOwner(states.gameObject);
            var channel = states.VoiceChannel;
            for (int i = 0; i < _targetRegulator.Length; i++)
            {
                if (i == channel)
                {
                    _targetRegulator[i].AssignPlayer(owner);
                }
                else
                {
                    _targetRegulator[i].ReleasePlayer(owner);
                }
                
                _buttonChannelOFF[i].SetActive(i != channel);
                _buttonChannelON[i].SetActive(i == channel);
            }

            if (-1 < channel && channel < _channelSlot.Length)
            {
                states.transform.SetParent(_channelSlot[channel], false);
            }
            else
            {
                states.ResetParent();
            }
        }

        /******************************
         uGUIイベント受け取り用メソッド
         ******************************/
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
        public void Assign(int channel)
        {
            localPlayerStates.VoiceChannel = channel;
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
        public void Release(int channel)
        {
            localPlayerStates.VoiceChannel = -1;
        }
    }
}
