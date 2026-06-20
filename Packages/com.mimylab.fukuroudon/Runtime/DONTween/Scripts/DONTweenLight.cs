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

    public enum DONTweenLightProperties
    {
        //None = 0,
        Color = 1 << 0,
        Intensity = 1 << 1,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-light")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween Light")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenLight : DONTween
    {
        [Header("Value Settings")]
        [SerializeField, EnumFlag]
        private DONTweenLightProperties _changeProperty;
        public float intensity = 1.0f;
        public Color color = Color.white;
        [Min(0.0f)]

        private Light _targetLight;
        private VRCTweenHandle _colorHandle;
        private VRCTweenHandle _intensityHandle;
        private bool _isChangeColor;
        private bool _isChangeIntensity;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _targetLight = _target ? _target.GetComponent<Light>() : GetComponent<Light>();

            _isChangeColor = ((int)_changeProperty & (int)DONTweenLightProperties.Color) > 0 && _targetLight;
            _isChangeIntensity = ((int)_changeProperty & (int)DONTweenLightProperties.Intensity) > 0 && _targetLight;

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
            _colorHandle.Kill();
            _intensityHandle.Kill();
        }

        public override void Reconfigure()
        {
            if (!isActiveAndEnabled) { return; }

            _colorHandle.Kill();
            _intensityHandle.Kill();
            Configure();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Play(); }
            if (_isChangeIntensity) { _intensityHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Pause(); }
            if (_isChangeIntensity) { _intensityHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Complete(); }
            if (_isChangeIntensity) { _intensityHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Restart(); }
            if (_isChangeIntensity) { _intensityHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Flip(); }
            if (_isChangeIntensity) { _intensityHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.PlayBackwards(); }
            if (_isChangeIntensity) { _intensityHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.PlayForwards(); }
            if (_isChangeIntensity) { _intensityHandle.PlayForwards(); }
        }

        private void Configure()
        {
            if (_isChangeColor)
            {
                _colorHandle = _targetLight.TweenColor(color, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType).Pause();
                if (!fixedDuration)
                {
                    _colorHandle.SetSpeedBased();
                }
                if (easeType == VRCTweenEase.None)
                {
                    _colorHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _colorHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    // Fade が無効＝後で実行されない
                    if (!_isChangeIntensity)
                    {
                        _colorHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                    }
                }
            }

            if (_isChangeIntensity)
            {
                _intensityHandle = _targetLight.TweenIntensity(intensity, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType).Pause();
                if (!fixedDuration)
                {
                    _intensityHandle.SetSpeedBased();
                }
                if (easeType == VRCTweenEase.None)
                {
                    _intensityHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _intensityHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    _intensityHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                }
            }
        }
    }
}
