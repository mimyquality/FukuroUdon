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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#voicechannel-selector")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceChannelSelector : UdonSharpBehaviour
    {
        [SerializeField]
        private PlayerAudioRegulatorRegister[] _targetRegulator = new PlayerAudioRegulatorRegister[0];
        [SerializeField]
        private GameObject[] _buttonChannelOFF = new GameObject[0];
        [SerializeField]
        private GameObject[] _buttonChannelON = new GameObject[0];
        [SerializeField]
        private Transform[] _channelSlot = new Transform[0];
        [SerializeField]
        private VoiceChannelPlayerStates _playerStates;

        [Header("Options")]
        [SerializeField]
        private AudioSource _speaker;
        [SerializeField]
        private AudioClip _channelJoinSound;
        [SerializeField]
        private AudioClip _channelLeaveSound;

        private int _playerCount = 1;
        private VRCPlayerApi[] _players = new VRCPlayerApi[1];
        private VoiceChannelPlayerStates _localPlayerStates;

        private VoiceChannelPlayerStates _LocalPlayerStates { get => _localPlayerStates ? _localPlayerStates : _localPlayerStates = (VoiceChannelPlayerStates)Networking.LocalPlayer.FindComponentInPlayerObjects(_playerStates); }

        private void OnValidate()
        {
            if (_targetRegulator.Length > 10)
            {
                var overcutTargetRegulator = new PlayerAudioRegulatorRegister[10];
                System.Array.Copy(_targetRegulator, overcutTargetRegulator, 10);
                _targetRegulator = overcutTargetRegulator;
            }
        }

        private void OnEnable()
        {
            _RefreshPlayers();
        }

        private void Update()
        {
            VRCPlayerApi selectedPlayer = _players[Time.frameCount % _playerCount];
            if (!Utilities.IsValid(selectedPlayer)) { return; }

            var channel = -1;
            for (int i = 0; i < _targetRegulator.Length; i++)
            {
                if (!_targetRegulator[i]) { continue; }

                if (System.Array.IndexOf(_targetRegulator[i].PlayerIds, selectedPlayer.playerId) > -1)
                {
                    channel = i;
                    break;
                }
            }
            var playerStates = (VoiceChannelPlayerStates)selectedPlayer.FindComponentInPlayerObjects(_playerStates);
            if (Utilities.IsValid(playerStates))
            {
                playerStates.VoiceChannel = channel;
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            _RefreshPlayers();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayers), 1);
        }

        public void _RefreshPlayers()
        {
            _playerCount = Mathf.Max(VRCPlayerApi.GetPlayerCount(), 1);
            _players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[_playerCount]);
        }

        public void _OnPlayerStatesChange(VoiceChannelPlayerStates states)
        {
            VRCPlayerApi changedPlayer = Networking.GetOwner(states.gameObject);
            int channel = states.VoiceChannel;

            // 誰かが同じチャンネルに入ってきたか、同じチャンネルから抜けた
            if (!changedPlayer.isLocal && _LocalPlayerStates.VoiceChannel > -1)
            {
                if (channel == _LocalPlayerStates.VoiceChannel)
                {
                    if (_speaker && _channelJoinSound)
                    {
                        _speaker.PlayOneShot(_channelJoinSound);
                    }
                }
                else if (states.transform.parent == _LocalPlayerStates.transform.parent)
                {
                    if (_speaker && _channelLeaveSound)
                    {
                        _speaker.PlayOneShot(_channelLeaveSound);
                    }
                }
            }

            // 表示更新
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
            if (channel < 0 || channel >= _targetRegulator.Length) { return; }
            if (!_targetRegulator[channel]) { return; }

            // 対応するRegulatorRegisterに登録処理
            for (int i = 0; i < _targetRegulator.Length; i++)
            {
                if (!_targetRegulator[i]) { continue; }

                if (i == channel)
                {
                    _targetRegulator[i].AssignPlayer();
                }
                else
                {
                    _targetRegulator[i].ReleasePlayer();
                }

                _buttonChannelOFF[i].SetActive(i != channel);
                _buttonChannelON[i].SetActive(i == channel);
            }

            _LocalPlayerStates.VoiceChannel = channel;

            if (_speaker && _channelJoinSound)
            {
                _speaker.PlayOneShot(_channelJoinSound);
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
        public void Release(int channel)
        {
            if (channel < 0 || channel >= _targetRegulator.Length) { return; }
            if (!_targetRegulator[channel]) { return; }

            // 対応するRegulatorRegisterから除名処理
            _targetRegulator[channel].ReleasePlayer();
            _buttonChannelOFF[channel].SetActive(true);
            _buttonChannelON[channel].SetActive(false);

            _LocalPlayerStates.VoiceChannel = -1;

            if (_speaker && _channelLeaveSound)
            {
                _speaker.PlayOneShot(_channelLeaveSound);
            }
        }
    }
}
