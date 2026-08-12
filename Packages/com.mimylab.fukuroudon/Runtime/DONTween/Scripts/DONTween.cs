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
    using VRC.Udon;

    public enum DONTweenTweenDirection
    {
        To,
        From
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class DONTween : UdonSharpBehaviour
    {
        [Header("Tween Settings")]
        [SerializeField]
        private protected GameObject _target = null;
        [Space]
        public bool fixedDuration = true;
        [Min(0.0f)]
        public float duration = 1.0f;
        [Min(0.0f)]
        public float delay = 0.0f;
        public VRCTweenEase easeType = VRCTweenEase.Linear;
        public AnimationCurve customEase = AnimationCurve.Linear(0f, 0f, 1f, 1);
        [Min(-1)]
        public int loops = 1;
        public VRCTweenLoopType loopType = VRCTweenLoopType.Restart;
        public DONTweenTweenDirection tweenDirection = DONTweenTweenDirection.To;
        public bool playOnAwake = true;
        [Space]
        [SerializeField]
        private protected UdonBehaviour _callback = null;
        [SerializeField]
        private protected string _callbackNameOnComplete = "";

        public abstract void Reconfigure();
        public abstract void Play();
        public abstract void Pause();
        public abstract void Complete();
        public abstract void Restart();
        public abstract void Flip();
        public abstract void PlayBackwards();
        public abstract void PlayForwards();
    }
}