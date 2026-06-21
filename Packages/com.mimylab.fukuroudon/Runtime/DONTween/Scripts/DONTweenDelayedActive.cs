/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-delayed-active")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween Delayed Active")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenDelayedActive : DONTween
    {
        [Header("Value Settings")]
        public bool objectActive = false;

        private VRCTweenHandle _activeHandle;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            Configure();
            if (playOnAwake) { Restart(); }
        }

        private void OnDisable()
        {
            _activeHandle.Kill();
        }

        public override void Reconfigure()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.Kill();
            Configure();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.Play();
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.Pause();
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.Complete();
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.Restart();
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.Flip();
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.PlayBackwards();
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            _activeHandle.PlayForwards();
        }

        private void Configure()
        {
            _activeHandle = VRCTween.DelayedSetActive(_target, objectActive, delay)
                .SetLoops(loops, loopType).Pause();
            if (tweenDirection == DONTweenTweenDirection.From)
            {
                _activeHandle.From();
            }
            if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
            {
                _activeHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
            }
        }
    }
}
