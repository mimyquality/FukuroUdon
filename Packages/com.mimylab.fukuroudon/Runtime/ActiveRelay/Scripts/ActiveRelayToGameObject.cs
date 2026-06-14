/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase.Editor.Attributes;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-gameobject")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to GameObject")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToGameObject : ActiveRelayTo
    {
        [SerializeField]
        private ActiveRelayActiveEvent _eventType = default;
        [SerializeField]
        private GameObject[] _gameObjects = new GameObject[0];
        [SerializeField]
        private bool _invert = false;

        [Space]
        [HelpBox("Disable delay if Dlelay Time is 0", HelpBoxAttribute.MessageType.Info)]
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _delayTime = 0.0f;
        [SerializeField]
        private bool _delayLatestOnly = false;

        private VRCTweenHandle _activateTweenHandle;
        private VRCTweenHandle _deactivateTweenHandle;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _activateTweenHandle = VRCTween.DelayedCall(this, nameof(_ActivateDelayed), _delayTime)
                .Pause();
            _deactivateTweenHandle = VRCTween.DelayedCall(this, nameof(_DeactivateDelayed), _delayTime)
                .Pause();

            _initialized = true;
        }

        private protected override void OnEnable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Active)
            {
                if (_delayTime > 0.0f)
                {
                    ToggleActiveDelayed(!_invert);
                    return;
                }

                ToggleActive(!_invert);
            }
        }

        private protected override void OnDisable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Inactive)
            {
                if (_delayTime > 0.0f)
                {
                    ToggleActiveDelayed(_invert);
                    return;
                }

                ToggleActive(_invert);
            }
        }

        private void OnDestroy()
        {
            gameObject.KillAllTweens();
        }

        private void ToggleActiveDelayed(bool value)
        {
            if (value)
            {
                if (_delayLatestOnly)
                {
                    Initialize();
                    _activateTweenHandle.SetDuration(_delayTime).Restart();
                }
                else
                {
                    VRCTween.DelayedCall(this, nameof(_ActivateDelayed), _delayTime);
                }
            }
            else
            {
                if (_delayLatestOnly)
                {
                    Initialize();
                    _deactivateTweenHandle.SetDuration(_delayTime).Restart();
                }
                else
                {
                    VRCTween.DelayedCall(this, nameof(_DeactivateDelayed), _delayTime);
                }
            }
        }

        public void _ActivateDelayed()
        {
            ToggleActive(true);
        }

        public void _DeactivateDelayed()
        {
            ToggleActive(false);
        }

        private void ToggleActive(bool value)
        {
            foreach (GameObject item in _gameObjects)
            {
                if (!item) { continue; }

                item.SetActive(value);
            }
        }
    }
}
