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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-sprite")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween Sprite")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenSprite : DONTween
    {
        [Header("Value Settings")]
        [SerializeField, EnumFlag]
        private DONTweenGraphicProperties _changeProperty;
        public Color color = Color.white;
        [Range(0.0f, 1.0f)]
        public float fade = 0.0f;

        private SpriteRenderer _targetSprite;
        private VRCTweenHandle _colorHandle;
        private VRCTweenHandle _fadeHandle;
        private bool _isChangeColor;
        private bool _isChangeFade;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _targetSprite = _target ? _target.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();

            _isChangeColor = ((int)_changeProperty & (int)DONTweenGraphicProperties.Color) > 0 && _targetSprite;
            _isChangeFade = ((int)_changeProperty & (int)DONTweenGraphicProperties.Fade) > 0 && _targetSprite;

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
            _fadeHandle.Kill();
        }

        public override void Reconfigure()
        {
            if (!isActiveAndEnabled) { return; }

            _colorHandle.Kill();
            _fadeHandle.Kill();
            Configure();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Play(); }
            if (_isChangeFade) { _fadeHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Pause(); }
            if (_isChangeFade) { _fadeHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Complete(); }
            if (_isChangeFade) { _fadeHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Restart(); }
            if (_isChangeFade) { _fadeHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Flip(); }
            if (_isChangeFade) { _fadeHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.PlayBackwards(); }
            if (_isChangeFade) { _fadeHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.PlayForwards(); }
            if (_isChangeFade) { _fadeHandle.PlayForwards(); }
        }

        private void Configure()
        {
            if (_isChangeColor)
            {
                _colorHandle = _targetSprite.TweenColor(color, duration, easeType)
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
                    if (!_isChangeFade)
                    {
                        _colorHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                    }
                }
            }

            if (_isChangeFade)
            {
                _fadeHandle = _targetSprite.TweenFade(fade, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType).Pause();
                if (!fixedDuration)
                {
                    _fadeHandle.SetSpeedBased();
                }
                if (easeType == VRCTweenEase.None)
                {
                    _fadeHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _fadeHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    _fadeHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                }
            }
        }
    }
}
