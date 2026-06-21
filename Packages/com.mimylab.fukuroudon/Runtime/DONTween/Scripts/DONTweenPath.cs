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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/DON-Tween#don-tween-path")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/DON Tween/DON Tween Path")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public partial class DONTweenPath : DONTween
    {
        [Header("Value Settings")]
        [SerializeField]
        private Transform[] waypoints = new Transform[0];
        public VRCTweenPathType pathType = VRCTweenPathType.Linear;
        [Range(0, 25)]
        public int pathResolution = 10;
        public bool closePath = false;
        [SerializeField]
        private Space _relativeTo = Space.World;

        private Vector3[] _waypointsPosition = new Vector3[0];
        private VRCTweenHandle _pathHandle;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            if (!_target) { _target = gameObject; }

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
            _pathHandle.Kill();
        }

        public override void Reconfigure()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.Kill();
            Configure();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.Play();
        }

        public override void Pause()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.Pause();
        }

        public override void Complete()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.Complete();
        }

        public override void Restart()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.Restart();
        }

        public override void Flip()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.Flip();
        }

        public override void PlayBackwards()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.PlayBackwards();
        }

        public override void PlayForwards()
        {
            if (!isActiveAndEnabled) { return; }

            _pathHandle.PlayForwards();
        }

        private void Configure()
        {
            if (_waypointsPosition.Length != waypoints.Length)
            {
                _waypointsPosition = new Vector3[waypoints.Length];
            }
            for (int i = 0; i < _waypointsPosition.Length; i++)
            {
                if (waypoints[i])
                {
                    _waypointsPosition[i] = _relativeTo == Space.World ? waypoints[i].position : waypoints[i].localPosition;
                }
                else
                {
                    _waypointsPosition[i] = Vector3.zero;
                }
            }

            _pathHandle = _relativeTo == Space.World ?
                _target.TweenPath(_waypointsPosition, duration, pathType, closePath, pathResolution, easeType) :
                _target.TweenLocalPath(_waypointsPosition, duration, pathType, closePath, pathResolution, easeType);
            _pathHandle.SetDelay(delay).SetLoops(loops, loopType).Pause();
            if (!fixedDuration)
            {
                _pathHandle.SetSpeedBased();
            }
            if (easeType == VRCTweenEase.None)
            {
                _pathHandle.SetEase(customEase);
            }
            if (tweenDirection == DONTweenTweenDirection.From)
            {
                _pathHandle.From();
            }
            if (_callback && !string.IsNullOrEmpty(_callbackNameOnComplete))
            {
                _pathHandle.OnComplete(_callback, nameof(_callbackNameOnComplete));
            }
        }
    }
}
