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

    public enum DONTweenRectCProperties
    {
        //None = 0,
        AnchorPos = 1 << 0,
        SizeDelta = 1 << 1,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-ui-rect")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween UI Rect")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenUIRect : DONTween
    {
        [Header("Value Settings")]
        [SerializeField, EnumFlag]
        private DONTweenRectCProperties _changeProperty;
        public Vector2 anchorPos = Vector2.zero;
        public Vector2 sizeDelta = Vector2.zero;
        [Space]
        [SerializeField]
        private RectTransform _referenceTransform = null;

        private RectTransform _targetRect;
        private VRCTweenHandle _anchorPosHandle;
        private VRCTweenHandle _sizeDeltaHandle;
        private bool _isChangeAnchorPos;
        private bool _isChangeSizeDelta;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _targetRect = (RectTransform)(_target ? _target.transform : transform);

            _isChangeAnchorPos = ((int)_changeProperty & (int)DONTweenRectCProperties.AnchorPos) > 0;
            _isChangeSizeDelta = ((int)_changeProperty & (int)DONTweenRectCProperties.SizeDelta) > 0;

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
            _anchorPosHandle.Kill();
            _sizeDeltaHandle.Kill();
        }

        public override void Reconfigure()
        {
            if (!isActiveAndEnabled) { return; }

            _anchorPosHandle.Kill();
            _sizeDeltaHandle.Kill();
            Configure();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeAnchorPos) { _anchorPosHandle.Play(); }
            if (_isChangeSizeDelta) { _sizeDeltaHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeAnchorPos) { _anchorPosHandle.Pause(); }
            if (_isChangeSizeDelta) { _sizeDeltaHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeAnchorPos) { _anchorPosHandle.Complete(); }
            if (_isChangeSizeDelta) { _sizeDeltaHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeAnchorPos) { _anchorPosHandle.Restart(); }
            if (_isChangeSizeDelta) { _sizeDeltaHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeAnchorPos) { _anchorPosHandle.Flip(); }
            if (_isChangeSizeDelta) { _sizeDeltaHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeAnchorPos) { _anchorPosHandle.PlayBackwards(); }
            if (_isChangeSizeDelta) { _sizeDeltaHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeAnchorPos) { _anchorPosHandle.PlayForwards(); }
            if (_isChangeSizeDelta) { _sizeDeltaHandle.PlayForwards(); }
        }

        private void Configure()
        {
            if (_referenceTransform)
            {
                anchorPos = _referenceTransform.anchoredPosition;
                sizeDelta = _referenceTransform.sizeDelta;
            }

            if (_isChangeAnchorPos)
            {
                _anchorPosHandle = _targetRect.TweenAnchorPos(anchorPos, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType).Pause();
                if (!fixedDuration)
                {
                    _anchorPosHandle.SetSpeedBased();
                }
                if (easeType == VRCTweenEase.None)
                {
                    _anchorPosHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _anchorPosHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    // SizeDelta が無効＝後で実行されない
                    if (!_isChangeSizeDelta)
                    {
                        _anchorPosHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                    }
                }
            }

            if (_isChangeSizeDelta)
            {
                _sizeDeltaHandle = _targetRect.TweenSizeDelta(sizeDelta, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType).Pause();
                if (!fixedDuration)
                {
                    _sizeDeltaHandle.SetSpeedBased();
                }
                if (easeType == VRCTweenEase.None)
                {
                    _sizeDeltaHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _sizeDeltaHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    _sizeDeltaHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                }
            }
        }
    }
}
