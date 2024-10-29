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
    using VRC.SDK3.Components;
    using TMPro;

    [Icon(ComponentIconPath.FukuroUdon)]
    [RequireComponent(typeof(VRCPlayerObject))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VoiceChannelPlayerStates : UdonSharpBehaviour
    {
        [SerializeField]
        private VoiceChannelSelector _selector;
        [SerializeField]
        private TextMeshProUGUI _playersNameText;

        private Transform _defaultParent;

        [UdonSynced, FieldChangeCallback(nameof(VoiceChannel))]
        private int _voiceChannel = -1;
        public int VoiceChannel
        {
            get => _voiceChannel;
            set
            {
                _voiceChannel = value;
                RequestSerialization();

                _selector._OnPlayerStatesChange(this);
            }
        }

        private void Start()
        {
            _defaultParent = this.transform.parent;

            if (!_playersNameText) { _playersNameText = GetComponentInChildren<TextMeshProUGUI>(true); }
            _playersNameText.text = Networking.GetOwner(this.gameObject).displayName;

            if (Networking.IsOwner(this.gameObject))
            {
                _selector.localPlayerStates = this;
            }
        }

        // プレイヤーが退室したら自然とリストから消えるので気にしなくて良い
        //private void OnDestroy() { }

        public void ResetParent()
        {
            this.transform.SetParent(_defaultParent, false);
        }
    }
}
