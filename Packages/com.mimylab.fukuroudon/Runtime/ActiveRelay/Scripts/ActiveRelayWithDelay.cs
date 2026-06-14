/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase.Editor.Attributes;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-with-delay")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay with/ActiveRelay with Delay")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayWithDelay : UdonSharpBehaviour
    {
        [HelpBox("Disable delay if Dlelay Time is 0", HelpBoxAttribute.MessageType.Info)]
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _delayTimeToInactive = 0.0f;
        [SerializeField, Min(0.0f), Tooltip("sec")]
        private float _delayTimeToActive = 0.0f;

        private VRCTweenHandle _activateTweenHandle;
        private VRCTweenHandle _deactivateTweenHandle;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _activateTweenHandle = VRCTween.DelayedSetActive(gameObject, true, _delayTimeToActive)
                .Pause();
            _deactivateTweenHandle = VRCTween.DelayedSetActive(gameObject, false, _delayTimeToInactive)
                .Pause();

            _initialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            if (_delayTimeToInactive > 0.0f)
            {
                _deactivateTweenHandle.SetDuration(_delayTimeToInactive).Restart();
            }
        }

        private void OnDisable()
        {
            Initialize();

            if (_delayTimeToActive > 0.0f)
            {
                _activateTweenHandle.SetDuration(_delayTimeToActive).Restart();
            }
        }

        private void OnDestroy()
        {
            gameObject.KillAllTweens();
        }
    }
}
