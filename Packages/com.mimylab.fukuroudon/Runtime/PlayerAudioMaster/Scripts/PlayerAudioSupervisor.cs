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

    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Supervisor")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioSupervisor : UdonSharpBehaviour
    {
        public const int HardCap = 90;
        public const string PlayerAudioChannelTagName = "PlayerAudioChannel";
        public const string PlayerAudioOverrideTagName = "PlayerAudioOverride";

        private const string TagIsEmpty = "None";

        [Header("Reference Settings")]
        public IPlayerAudioRegulator[] playerAudioRegulators;

        [Header("Player Voice Settings")]
        [Range(0f, 24f)]
        public float defaultVoiceGain = 15f;
        [Range(0f, 999999.9f)]
        public float defaultVoiceDistanceNear = 0f;
        [Range(0f, 999999.9f)]
        public float defaultVoiceDistanceFar = 25f;

        [Header("Player Voice Advance Settings")]
        [Range(0f, 1000f)]
        public float defaultVoiceVolumetricRadius = 0f;
        public bool defaultVoiceLowpass = true;

        [Header("Avatar Audio Settings")]
        [Range(0f, 10f)]
        public float defaultAvatarAudioGain = 10f;
        [Min(0f)]
        public float defaultAvatarAudioDistanceNear = 0f;
        [Min(0f)]
        public float defaultAvatarAudioDistanceFar = 40f;

        [Header("Avatar Audio Advance Settings")]
        [Min(0f)]
        public float defaultAvatarAudioVolumetricRadius = 0f;
        public bool defaultAvatarAudioForceSpatial = false;
        public bool defaultAvatarAudioCustomCurve = false;

        // キャッシュ用
        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi[] _players = new VRCPlayerApi[HardCap];

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

        private void Update()
        {
            var selecter = _players[Time.frameCount % HardCap];
            if (!Utilities.IsValid(selecter)) { return; }

            var channel = TagIsEmpty;
            var overrideNumber = TagIsEmpty;
            IPlayerAudioRegulator overrideRegulator = null;
            for (int i = 0; i < playerAudioRegulators.Length; i++)
            {
                if (!playerAudioRegulators[i]) { continue; }
                if (!playerAudioRegulators[i].CheckApplicable(selecter)) { continue; }

                if (!playerAudioRegulators[i].enableChannelMode)
                {
                    overrideNumber = i.ToString();
                    overrideRegulator = playerAudioRegulators[i];
                    break;
                }
                // ここからチャンネル処理

                channel = playerAudioRegulators[i].channel.ToString();
                if (selecter.isLocal) { break; }

                if (_localPlayer.GetPlayerTag(PlayerAudioChannelTagName) == channel)
                {
                    overrideNumber = i.ToString();
                    overrideRegulator = playerAudioRegulators[i];
                    break;
                }

                switch (playerAudioRegulators[i].channelUnmatchMode)
                {
                    case PlayerAudioRegulatorChannelUncmatchMode.Fallback:
                        if (overrideRegulator = playerAudioRegulators[i].unmatchFallback)
                        {
                            overrideNumber = i.ToString() + "fb";
                        }
                        break;
                    case PlayerAudioRegulatorChannelUncmatchMode.Passthrough:
                        channel = TagIsEmpty;
                        continue;
                    default:
                        break;
                }
                break;
            }

            if (selecter.GetPlayerTag(PlayerAudioChannelTagName) != channel)
            {
                selecter.SetPlayerTag(PlayerAudioChannelTagName, channel);
            }

            if (selecter.GetPlayerTag(PlayerAudioOverrideTagName) != overrideNumber)
            {
                selecter.SetPlayerTag(PlayerAudioOverrideTagName, overrideNumber);

                if (overrideNumber == TagIsEmpty)
                {
                    SetDefaultPlayerVoice(selecter);
                    SetDefaultAvatarAudio(selecter);
                    return;
                }

                if (!overrideRegulator.OverridePlayerVoice(selecter))
                {
                    SetDefaultPlayerVoice(selecter);
                }

                if (!overrideRegulator.OverrideAvatarAudio(selecter))
                {
                    SetDefaultAvatarAudio(selecter);
                }
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            Initialize();

            player.SetPlayerTag(PlayerAudioChannelTagName, TagIsEmpty);
            player.SetPlayerTag(PlayerAudioOverrideTagName, TagIsEmpty);
            SetDefaultPlayerVoice(player);
            SetDefaultAvatarAudio(player);

            VRCPlayerApi.GetPlayers(_players);
        }
        // プレイヤー保持配列の数を固定化したので不要
        /* 
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            Initialize();

            var playerCount = Mathf.Max(VRCPlayerApi.GetPlayerCount(), 1);
            _players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[playerCount]);
        }
         */
        private void SetDefaultPlayerVoice(VRCPlayerApi selectPlayer)
        {
            selectPlayer.SetVoiceGain(defaultVoiceGain);
            selectPlayer.SetVoiceDistanceNear(defaultVoiceDistanceNear);
            selectPlayer.SetVoiceDistanceFar(defaultVoiceDistanceFar);
            selectPlayer.SetVoiceVolumetricRadius(defaultVoiceVolumetricRadius);
            selectPlayer.SetVoiceLowpass(defaultVoiceLowpass);
        }

        private void SetDefaultAvatarAudio(VRCPlayerApi selectPlayer)
        {
            selectPlayer.SetAvatarAudioGain(defaultAvatarAudioGain);
            selectPlayer.SetAvatarAudioNearRadius(defaultAvatarAudioDistanceNear);
            selectPlayer.SetAvatarAudioFarRadius(defaultAvatarAudioDistanceFar);
            selectPlayer.SetAvatarAudioVolumetricRadius(defaultAvatarAudioVolumetricRadius);
            selectPlayer.SetAvatarAudioForceSpatial(defaultAvatarAudioForceSpatial);
            selectPlayer.SetAvatarAudioCustomCurve(defaultAvatarAudioCustomCurve);
        }
    }
}
