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

    public enum DONTWeenShaderProperties
    {
        //None = 0,
        Color = 1 << 0,
        Float = 1 << 1,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-renderer")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween Renderer")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenRenderer : DONTween
    {
        [Header("Value Settings")]
        [SerializeField, EnumFlag]
        private DONTWeenShaderProperties _changeProperty;
        public string colorName = "_Color";
        public Color color = Color.white;
        public string floatName = "_Cutoff";
        public float floatValue = 0.0f;

        private Renderer _targetRenderer;
        private VRCTweenHandle _colorHandle;
        private VRCTweenHandle _floatHandle;
        private bool _isChangeColor;
        private bool _isChangeFloat;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _targetRenderer = _target ? _target.GetComponent<Renderer>() : GetComponent<Renderer>();

            _isChangeColor = ((int)_changeProperty & (int)DONTWeenShaderProperties.Color) > 0 && _targetRenderer;
            _isChangeFloat = ((int)_changeProperty & (int)DONTWeenShaderProperties.Float) > 0 && _targetRenderer;

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            if (_isChangeColor)
            {
                _colorHandle = _targetRenderer.TweenColor(colorName, color, duration, easeType)
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
                    // Float が無効＝後で実行されない
                    if (!_isChangeFloat)
                    {
                        _colorHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                    }
                }
                if (!playOnAwake)
                {
                    _colorHandle.Pause();
                }
            }

            if (_isChangeFloat)
            {
                _floatHandle = _targetRenderer.TweenFloat(floatName, floatValue, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType);
                if (easeType == VRCTweenEase.None)
                {
                    _floatHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _floatHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    _floatHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                }
                if (!playOnAwake)
                {
                    _floatHandle.Pause();
                }
            }
        }

        private void OnDisable()
        {
            _colorHandle.Kill();
            _floatHandle.Kill();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Play(); }
            if (_isChangeFloat) { _floatHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Pause(); }
            if (_isChangeFloat) { _floatHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Complete(); }
            if (_isChangeFloat) { _floatHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Restart(); }
            if (_isChangeFloat) { _floatHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.Flip(); }
            if (_isChangeFloat) { _floatHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.PlayBackwards(); }
            if (_isChangeFloat) { _floatHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeColor) { _colorHandle.PlayForwards(); }
            if (_isChangeFloat) { _floatHandle.PlayForwards(); }
        }
    }
}
