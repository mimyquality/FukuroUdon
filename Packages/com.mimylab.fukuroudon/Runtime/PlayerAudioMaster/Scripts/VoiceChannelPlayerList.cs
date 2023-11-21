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
    using VRC.Udon;
    using VRC.SDK3.Components;
    using TMPro;

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VoiceChannelPlayerList : UdonSharpBehaviour
    {
        private const int HardCap = 90;

        [SerializeField]
        private GameObject _playerPrefab;
        [SerializeField]
        private Transform[] _channelSlot;
        [SerializeField]
        private int[] _channelList;

        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi[] _players = new VRCPlayerApi[HardCap];
        private GameObject[] _playersNameSlot = new GameObject[HardCap];
        private Transform[] _playersNameSlotTf = new Transform[HardCap];
        private TextMeshProUGUI[] _playersNameText = new TextMeshProUGUI[HardCap];

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
            var target = Time.frameCount % HardCap;
            if (!Utilities.IsValid(_players[target])) { return; }

            var channelTag = _players[target].GetPlayerTag(PlayerAudioSupervisor.PlayerAudioChannelTagName);
            int channel;
            if (int.TryParse(channelTag, out channel))
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
            VRCPlayerApi.GetPlayers(_players);
        }

        private void SetPlayerNameSlot(int target, int channel)
        {
            for (int i = 0; i < _channelList.Length; i++)
            {
                if (_channelList[i] == channel)
                {
                    _playersNameText[target].text = _players[target].displayName;
                    _playersNameSlotTf[target].parent = _channelSlot[i];
                    _playersNameSlot[target].SetActive(true);
                    return;
                }
            }
            _playersNameSlot[target].SetActive(false);
        }
    }
}
