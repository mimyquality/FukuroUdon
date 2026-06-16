/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.UI;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-ui-slider")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween UI Slider")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenUISlider : DONTween
    {
        [Header("Value Settings")]
        public float tweenValue = 0.0f;
        [Space]
        [SerializeField]
        private Slider _referenceSlider = null;

        private Slider _targetSlider;
        private VRCTweenHandle _tweenHandle;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _targetSlider = _target ? _target.GetComponent<Slider>() : GetComponent<Slider>();

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            if (!_targetSlider) { return; }

            if (_referenceSlider) { tweenValue = _referenceSlider.value; }

            _tweenHandle = _targetSlider.TweenValue(tweenValue, duration, easeType)
                .SetDelay(delay).SetLoops(loops, loopType)
                .OnComplete(_callback, nameof(_callbackEventName));
            if (easeType == VRCTweenEase.None)
            {
                _tweenHandle.SetEase(customEase);
            }
            if (tweenDirection == DONTweenTweenDirection.From)
            {
                _tweenHandle.From();
            }
            if (!playOnAwake)
            {
                _tweenHandle.Pause();
            }
        }

        private void OnDisable()
        {
            _tweenHandle.Kill();
        }


        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            if (_targetSlider) { _tweenHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_targetSlider) { _tweenHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_targetSlider) { _tweenHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_targetSlider) { _tweenHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_targetSlider) { _tweenHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_targetSlider) { _tweenHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_targetSlider) { _tweenHandle.PlayForwards(); }
        }
    }
}
