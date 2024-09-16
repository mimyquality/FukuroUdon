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
    using TMPro;

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceChannelPlayerList : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject _playerPrefab;
        [SerializeField]
        private Transform[] _channelSlot;
        [SerializeField]
        private int[] _channelList;

        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi[] _players = new VRCPlayerApi[PlayerAudioSupervisor.HardCap];
        private GameObject[] _playersNameSlot = new GameObject[PlayerAudioSupervisor.HardCap];
        private Transform[] _playersNameSlotTf = new Transform[PlayerAudioSupervisor.HardCap];
        private TextMeshProUGUI[] _playersNameText = new TextMeshProUGUI[PlayerAudioSupervisor.HardCap];

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _localPlayer = Networking.LocalPlayer;

            for (int i = 0; i < _playersNameSlot.Length; i++)
            {
                _playersNameSlot[i] = Instantiate(_playerPrefab, transform);
                _playersNameSlotTf[i] = _playersNameSlot[i].transform;
                _playersNameText[i] = _playersNameSlot[i].GetComponentInChildren<TextMeshProUGUI>(true);
                _playersNameSlot[i].SetActive(false);
            }

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            RefreshPlayers();
        }

        private void Update()
        {
            var target = Time.frameCount % PlayerAudioSupervisor.HardCap;
            if (!Utilities.IsValid(_players[target]))
            {
                _playersNameSlot[target].SetActive(false);
                return;
            }

            var channelTag = _players[target].GetPlayerTag(PlayerAudioSupervisor.PlayerAudioChannelTagName);
            if (int.TryParse(channelTag, out int channel))
            {
                SetPlayerNameSlot(target, channel);
            }
            else
            {
                _playersNameSlot[target].SetActive(false);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            Initialize();
            if (player.playerId < _localPlayer.playerId) { return; }

            RefreshPlayers();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            Initialize();

            RefreshPlayers();
        }

        private void RefreshPlayers()
        {
            _players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[PlayerAudioSupervisor.HardCap]);
            bool valid;
            for (int i = 0; i < _players.Length; i++)
            {
                valid = Utilities.IsValid(_players[i]);
                _playersNameText[i].text = valid ? _players[i].displayName : "";
                _playersNameSlot[i].SetActive(valid);
            }
        }

        private void SetPlayerNameSlot(int target, int channel)
        {
            for (int i = 0; i < _channelList.Length; i++)
            {
                if (_channelList[i] == channel)
                {
                    _playersNameSlotTf[target].SetParent(_channelSlot[i], false);
                    _playersNameSlot[target].SetActive(true);
                    return;
                }
            }
            _playersNameSlot[target].SetActive(false);
        }
    }
}
