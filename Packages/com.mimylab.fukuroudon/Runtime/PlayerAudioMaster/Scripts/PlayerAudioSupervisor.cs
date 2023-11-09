
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
        private const string PlayerAudioChannelTagName = "PlayerAudioChannel";
        private const string PlayerAudioOverrideTagName = "PlayerAudioOverride";

        [Header("Reference Settings")]
        public IPlayerAudioRegulator[] playerAudioRegulators;

        [Header("Player Voice Settings")]
        [Range(0f, 24f)]
        public float defaultVoiceGain = 15f;
        [Range(0f, 999999f)]
        public float defaultVoiceDistanceNear = 0f;
        [Range(0f, 999999f)]
        public float defaultVoiceDistanceFar = 25f;

        [Header("Player Voice Advance Settings")]
        [Range(0f, 1000f)]
        public float defaultVoiceVolumetricRadius = 0f;
        public bool defaultVoiceLowpass = true;

        [Header("Avatar Audio Settings")]
        [Range(0f, 10f)]
        public float defaultAvatarAudioGain = 10f;
        [Range(0f, 40f)]
        public float defaultAvatarAudioDistanceNear = 40f;
        [Range(0f, 40f)]
        public float defaultAvatarAudioDistanceFar = 40f;

        [Header("Avatar Audio Advance Settings")]
        [Range(0f, 40f)]
        public float defaultAvatarAudioVolumetricRadius = 40f;
        public bool defaultAvatarAudioForceSpatial = false;
        public bool defaultAvatarAudioCustomCurve = false;

        // キャッシュ用
        private VRCPlayerApi _localPlayer;
        private VRCPlayerApi[] _players = new VRCPlayerApi[1];
        private int[] _assignChannel;

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
            var selecter = Time.frameCount % Mathf.Clamp(_players.Length, 1, _players.Length);
            if (!Utilities.IsValid(_players[selecter])) { return; }

            var channel = "None";
            var overrideNumber = "None";
            IPlayerAudioRegulator overrideRegulator = null;
            for (int i = 0; i < playerAudioRegulators.Length; i++)
            {
                if (!playerAudioRegulators[i]) { continue; }

                if (playerAudioRegulators[i].CheckApplicable(_players[selecter]))
                {
                    if (playerAudioRegulators[i].enableChannelMode)
                    {
                        channel = playerAudioRegulators[i].channel.ToString();

                        if (_localPlayer.GetPlayerTag(PlayerAudioChannelTagName) == channel)
                        {
                            overrideNumber = i.ToString();
                            overrideRegulator = playerAudioRegulators[i];
                        }
                    }
                    else
                    {
                        overrideNumber = i.ToString();
                        overrideRegulator = playerAudioRegulators[i];
                    }

                    break;
                }
            }
            _players[selecter].SetPlayerTag(PlayerAudioChannelTagName, channel);

            if (_players[selecter].GetPlayerTag(PlayerAudioOverrideTagName) != overrideNumber)
            {
                _players[selecter].SetPlayerTag(PlayerAudioOverrideTagName, overrideNumber);

                if (overrideNumber == "None")
                {
                    SetDefaultPlayerVoice(_players[selecter]);
                    SetDefaultAvatarAudio(_players[selecter]);

                    return;
                }

                if (!overrideRegulator.OverridePlayerVoice(_players[selecter]))
                {
                    SetDefaultPlayerVoice(_players[selecter]);
                }

                if (!overrideRegulator.OverrideAvatarAudio(_players[selecter]))
                {
                    SetDefaultAvatarAudio(_players[selecter]);
                }
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            Initialize();

            player.SetPlayerTag(PlayerAudioChannelTagName, "None");
            player.SetPlayerTag(PlayerAudioOverrideTagName, "None");
            SetDefaultPlayerVoice(player);
            SetDefaultAvatarAudio(player);

            if (player.playerId >= _localPlayer.playerId)
            {
                VRCPlayerApi.GetPlayers(_players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            Initialize();

            var playerCount = Mathf.Clamp(VRCPlayerApi.GetPlayerCount(), 1, int.MaxValue);
            VRCPlayerApi.GetPlayers(_players = new VRCPlayerApi[playerCount]);
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
    }
}
