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

    public enum DONTweenAudioProperties
    {
        //None = 0,
        Volume = 1 << 0,
        Pitch = 1 << 1,
    }

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-audio")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween Audio")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DONTweenAudio : DONTween
    {
        [Header("Value Settings")]
        [SerializeField, EnumFlag]
        private DONTweenGraphicProperties _changeProperty;
        [Range(0.0f, 1.0f)]
        public float volume = 1.0f;
        [Range(-3.0f, 3.0f)]
        public float pitch = 1.0f;

        private AudioSource _targetAudio;
        private VRCTweenHandle _volumeHandle;
        private VRCTweenHandle _pitchHandle;
        private bool _isChangeVolume;
        private bool _isChangePitch;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _targetAudio = _target ? _target.GetComponent<AudioSource>() : GetComponent<AudioSource>();

            _isChangeVolume = ((int)_changeProperty & (int)DONTweenAudioProperties.Volume) > 0 && _targetAudio;
            _isChangePitch = ((int)_changeProperty & (int)DONTweenAudioProperties.Pitch) > 0 && _targetAudio;

            _initialized = true;
        }
        private void OnEnable()
        {
            Initialize();

            if (_isChangeVolume)
            {
                _volumeHandle = _targetAudio.TweenVolume(volume, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType);
                if (easeType == VRCTweenEase.None)
                {
                    _volumeHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _volumeHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    // Pitch が無効＝後で実行されない
                    if (!_isChangePitch)
                    {
                        _volumeHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                    }
                }
                if (!playOnAwake)
                {
                    _volumeHandle.Pause();
                }
            }

            if (_isChangePitch)
            {
                _pitchHandle = _targetAudio.TweenPitch(pitch, duration, easeType)
                    .SetDelay(delay).SetLoops(loops, loopType);
                if (easeType == VRCTweenEase.None)
                {
                    _pitchHandle.SetEase(customEase);
                }
                if (tweenDirection == DONTweenTweenDirection.From)
                {
                    _pitchHandle.From();
                }
                if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
                {
                    _pitchHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
                }
                if (!playOnAwake)
                {
                    _pitchHandle.Pause();
                }
            }
        }

        private void OnDisable()
        {
            _volumeHandle.Kill();
            _pitchHandle.Kill();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeVolume) { _volumeHandle.Play(); }
            if (_isChangePitch) { _pitchHandle.Play(); }
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeVolume) { _volumeHandle.Pause(); }
            if (_isChangePitch) { _pitchHandle.Pause(); }
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeVolume) { _volumeHandle.Complete(); }
            if (_isChangePitch) { _pitchHandle.Complete(); }
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeVolume) { _volumeHandle.Restart(); }
            if (_isChangePitch) { _pitchHandle.Restart(); }
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeVolume) { _volumeHandle.Flip(); }
            if (_isChangePitch) { _pitchHandle.Flip(); }
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeVolume) { _volumeHandle.PlayBackwards(); }
            if (_isChangePitch) { _pitchHandle.PlayBackwards(); }
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            if (_isChangeVolume) { _volumeHandle.PlayForwards(); }
            if (_isChangePitch) { _pitchHandle.PlayForwards(); }
        }
    }
}
