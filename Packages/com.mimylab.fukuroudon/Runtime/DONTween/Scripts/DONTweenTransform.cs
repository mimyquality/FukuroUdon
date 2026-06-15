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
    public enum DONTweenTransformChangeProperties
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
        private DONTweenTransformChangeProperties _changeProperty;
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
        private bool _changePosition;
        private bool _changeRotation;
        private bool _changeScale;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_target) { _target = gameObject; }

            _changePosition = ((int)_changeProperty & (int)DONTweenTransformChangeProperties.Position) > 0;
            _changeRotation = ((int)_changeProperty & (int)DONTweenTransformChangeProperties.Rotation) > 0;
            _changeScale = ((int)_changeProperty & (int)DONTweenTransformChangeProperties.Scale) > 0;

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

            if (_changePosition)
            {
                _positionHandle = _relativeTo == Space.World ?
                    _target.TweenPosition(position, duration, easeType) :
                    _target.TweenLocalPosition(position, duration, easeType);
                _positionHandle.SetDelay(delay).SetLoops(loops, loopType)
                    .OnComplete(_callback, nameof(_callbackEventName));
                if (easeType == VRCTweenEase.None)
                {
                    _positionHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _positionHandle.From();
                }
            }

            if (_changeRotation)
            {
                _rotationHandle = _relativeTo == Space.World ?
                    _target.TweenRotation(rotation.eulerAngles, duration, easeType) :
                    _target.TweenLocalRotation(rotation.eulerAngles, duration, easeType);
                _rotationHandle.SetDelay(delay).SetLoops(loops, loopType)
                    .OnComplete(_callback, nameof(_callbackEventName));
                if (easeType == VRCTweenEase.None)
                {
                    _rotationHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _rotationHandle.From();
                }
            }

            if (_changeScale)
            {
                _scaleHandle = _target.TweenScale(position, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType)
                    .OnComplete(_callback, nameof(_callbackEventName));
                if (easeType == VRCTweenEase.None)
                {
                    _scaleHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _scaleHandle.From();
                }
            }

            if (!playOnAwake)
            {
                _positionHandle.Pause();
                _rotationHandle.Pause();
                _scaleHandle.Pause();
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

            if (_changePosition) { _positionHandle.Play(); }
            if (_changeRotation) { _rotationHandle.Play(); }
            if (_changeScale) { _scaleHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_changePosition) { _positionHandle.Pause(); }
            if (_changeRotation) { _rotationHandle.Pause(); }
            if (_changeScale) { _scaleHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_changePosition) { _positionHandle.Complete(); }
            if (_changeRotation) { _rotationHandle.Complete(); }
            if (_changeScale) { _scaleHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_changePosition) { _positionHandle.Restart(); }
            if (_changeRotation) { _rotationHandle.Restart(); }
            if (_changeScale) { _scaleHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_changePosition) { _positionHandle.Flip(); }
            if (_changeRotation) { _rotationHandle.Flip(); }
            if (_changeScale) { _scaleHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_changePosition) { _positionHandle.PlayBackwards(); }
            if (_changeRotation) { _rotationHandle.PlayBackwards(); }
            if (_changeScale) { _scaleHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_changePosition) { _positionHandle.PlayForwards(); }
            if (_changeRotation) { _rotationHandle.PlayForwards(); }
            if (_changeScale) { _scaleHandle.PlayForwards(); }
        }
    }
}
