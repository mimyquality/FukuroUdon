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

    [Icon(ComponentIconPath.FukuroUdon)]
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
        private int _playerCount = 1;
        private VRCPlayerApi[] _players = new VRCPlayerApi[1];

        private void Update()
        {
            var chosenPlayer = _players[Time.frameCount % _playerCount];
            if (!Utilities.IsValid(chosenPlayer)) { return; }

            var channel = TagIsEmpty;
            var overrideNumber = TagIsEmpty;
            IPlayerAudioRegulator overrideRegulator = null;
            for (int i = 0; i < playerAudioRegulators.Length; i++)
            {
                if (!playerAudioRegulators[i]) { continue; }
                if (!playerAudioRegulators[i].CheckApplicable(chosenPlayer)) { continue; }

                if (!playerAudioRegulators[i].enableChannelMode)
                {
                    overrideNumber = i.ToString();
                    overrideRegulator = playerAudioRegulators[i];
                    break;
                }
                // ここからチャンネル処理

                channel = playerAudioRegulators[i].channel.ToString();

                if (Networking.LocalPlayer.GetPlayerTag(PlayerAudioChannelTagName) == channel)
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
                    case PlayerAudioRegulatorChannelUncmatchMode.Pretend:
                        overrideNumber = i.ToString();
                        overrideRegulator = playerAudioRegulators[i];
                        break;
                    default:
                        break;
                }
                break;
            }

            if (chosenPlayer.GetPlayerTag(PlayerAudioChannelTagName) != channel)
            {
                chosenPlayer.SetPlayerTag(PlayerAudioChannelTagName, channel);
            }

            var executeOverride = overrideRegulator && overrideRegulator.NeedRealtimeOverride;
            if (chosenPlayer.GetPlayerTag(PlayerAudioOverrideTagName) != overrideNumber)
            {
                chosenPlayer.SetPlayerTag(PlayerAudioOverrideTagName, overrideNumber);
                executeOverride = true;
            }

            if (executeOverride)
            {
                if (overrideNumber == TagIsEmpty)
                {
                    SetDefaultPlayerVoice(chosenPlayer);
                    SetDefaultAvatarAudio(chosenPlayer);
                    return;
                }

                if (!overrideRegulator.OverridePlayerVoice(chosenPlayer))
                {
                    SetDefaultPlayerVoice(chosenPlayer);
                }

                if (!overrideRegulator.OverrideAvatarAudio(chosenPlayer))
                {
                    SetDefaultAvatarAudio(chosenPlayer);
                }
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            player.SetPlayerTag(PlayerAudioChannelTagName, TagIsEmpty);
            player.SetPlayerTag(PlayerAudioOverrideTagName, TagIsEmpty);
            SetDefaultPlayerVoice(player);
            SetDefaultAvatarAudio(player);

            SendCustomEventDelayedFrames(nameof(_RefreshPlayerList), 1);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_RefreshPlayerList), 1);
        }

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

        public void _RefreshPlayerList()
        {
            _playerCount = Mathf.Max(VRCPlayerApi.GetPlayerCount(), 1);
            _players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[_playerCount]);
        }
    }
}
