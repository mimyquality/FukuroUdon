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

    [System.Flags]
    public enum DONTweenTransformProperties
    {
        //None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-transform")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween Transform")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenTransform : DONTween
    {
        [Header("Value Settings")]
        [SerializeField, EnumFlag]
        private DONTweenTransformProperties _changeProperty;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;
        [SerializeField]
        private Space _relativeTo = Space.Self;
        [Space]
        [SerializeField]
        private Transform _referenceTransform = null;

        private VRCTweenHandle _positionHandle;
        private VRCTweenHandle _rotationHandle;
        private VRCTweenHandle _scaleHandle;
        private bool _isChangePosition;
        private bool _isChangeRotation;
        private bool _isChangeScale;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_target) { _target = gameObject; }

            _isChangePosition = ((int)_changeProperty & (int)DONTweenTransformProperties.Position) > 0;
            _isChangeRotation = ((int)_changeProperty & (int)DONTweenTransformProperties.Rotation) > 0;
            _isChangeScale = ((int)_changeProperty & (int)DONTweenTransformProperties.Scale) > 0;

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            if (_referenceTransform)
            {
                position = _relativeTo == Space.World ? _referenceTransform.position : _referenceTransform.localPosition;
                rotation = _relativeTo == Space.World ? _referenceTransform.rotation : _referenceTransform.localRotation;
                scale = _referenceTransform.localScale;
            }

            if (_isChangePosition)
            {
                _positionHandle = _relativeTo == Space.World ?
                    _target.TweenPosition(position, duration, easeType) :
                    _target.TweenLocalPosition(position, duration, easeType);
                _positionHandle.SetDelay(delay).SetLoops(loops, loopType);
                if (easeType == VRCTweenEase.None)
                {
                    _positionHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _positionHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    // Position のみ有効＝他で実行されない
                    if (!_isChangeRotation && !_isChangeScale)
                    {
                        _positionHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                    }
                }
                if (!playOnAwake)
                {
                    _positionHandle.Pause();
                }
            }

            if (_isChangeRotation)
            {
                _rotationHandle = _relativeTo == Space.World ?
                    _target.TweenRotation(rotation.eulerAngles, duration, easeType) :
                    _target.TweenLocalRotation(rotation.eulerAngles, duration, easeType);
                _rotationHandle.SetDelay(delay).SetLoops(loops, loopType);
                if (easeType == VRCTweenEase.None)
                {
                    _rotationHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _rotationHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    // Scale が無効＝後で実行されない
                    if (!_isChangeScale)
                    {
                        _positionHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                    }
                }
                if (!playOnAwake)
                {
                    _rotationHandle.Pause();
                }
            }

            if (_isChangeScale)
            {
                _scaleHandle = _target.TweenScale(position, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType);
                if (easeType == VRCTweenEase.None)
                {
                    _scaleHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _scaleHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    _scaleHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                }
                if (!playOnAwake)
                {
                    _scaleHandle.Pause();
                }
            }
        }

        private void OnDisable()
        {
            _positionHandle.Kill();
            _rotationHandle.Kill();
            _scaleHandle.Kill();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangePosition) { _positionHandle.Play(); }
            if (_isChangeRotation) { _rotationHandle.Play(); }
            if (_isChangeScale) { _scaleHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangePosition) { _positionHandle.Pause(); }
            if (_isChangeRotation) { _rotationHandle.Pause(); }
            if (_isChangeScale) { _scaleHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangePosition) { _positionHandle.Complete(); }
            if (_isChangeRotation) { _rotationHandle.Complete(); }
            if (_isChangeScale) { _scaleHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangePosition) { _positionHandle.Restart(); }
            if (_isChangeRotation) { _rotationHandle.Restart(); }
            if (_isChangeScale) { _scaleHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangePosition) { _positionHandle.Flip(); }
            if (_isChangeRotation) { _rotationHandle.Flip(); }
            if (_isChangeScale) { _scaleHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangePosition) { _positionHandle.PlayBackwards(); }
            if (_isChangeRotation) { _rotationHandle.PlayBackwards(); }
            if (_isChangeScale) { _scaleHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangePosition) { _positionHandle.PlayForwards(); }
            if (_isChangeRotation) { _rotationHandle.PlayForwards(); }
            if (_isChangeScale) { _scaleHandle.PlayForwards(); }
        }
    }
}
