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

    public enum DONTweenGraphicProperties
    {
        //None = 0,
        Color = 1 << 0,
        Fade = 1 << 1,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-ui-graphic")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween UI Graphic")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenUIGraphic : DONTween
    {
        [Header("Value Settings")]
        [SerializeField, EnumFlag]
        private DONTweenGraphicProperties _changeProperty;
        public Color color = Color.white;
        [Range(0.0f, 1.0f)]
        public float fade = 0.0f;

        private MaskableGraphic _targetGraphic;
        private CanvasGroup _targetCanvasGroup;
        private VRCTweenHandle _colorHandle;
        private VRCTweenHandle _fadeHandle;
        private bool _isChangeColor;
        private bool _isChangeFade;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _targetGraphic = _target ? _target.GetComponent<MaskableGraphic>() : GetComponent<MaskableGraphic>();
            _targetCanvasGroup = _target ? _target.GetComponent<CanvasGroup>() : GetComponent<CanvasGroup>();

            _isChangeColor = ((int)_changeProperty & (int)DONTweenGraphicProperties.Color) > 0 && _targetGraphic;
            _isChangeFade = ((int)_changeProperty & (int)DONTweenGraphicProperties.Fade) > 0 && (_targetGraphic || _targetCanvasGroup);

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            if (_isChangeColor)
            {
                _colorHandle = _targetGraphic.TweenColor(color, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType);
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
                if (!playOnAwake)
                {
                    _colorHandle.Pause();
                }
            }

            if (_isChangeFade)
            {
                _fadeHandle = _targetGraphic ?
                    _targetGraphic.TweenFade(fade, duration, easeType) :
                    _targetCanvasGroup.TweenFade(fade, duration, easeType);
                _fadeHandle.SetDelay(delay).SetLoops(loops, loopType);
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
                if (!playOnAwake)
                {
                    _fadeHandle.Pause();
                }
            }
        }

        private void OnDisable()
        {
            _colorHandle.Kill();
            _fadeHandle.Kill();
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
    }
}
