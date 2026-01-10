/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.SDK3.Rendering;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Ambient-Effect-Assistant#canvas-distance-fade")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Canvas Distance Fade")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CanvasDistanceFade : UdonSharpBehaviour
    {
        [SerializeField]
        private CanvasGroup[] _canvasGroups = new CanvasGroup[0];

        [Space]
        [SerializeField, Min(0.0f)]
        private float _fadeStart = 5.0f;
        [SerializeField, Min(0.0f)]
        private float _fadeEnd = 7.0f;

        private bool _enableFade;
        private AnimationCurve _fadeCurve;
        private VRCCameraSettings _camera;

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _enableFade = !Mathf.Approximately(_fadeEnd - _fadeStart, 0.0f);
            _fadeCurve = AnimationCurve.Linear(_fadeStart, 1.0f, _fadeEnd, 0.0f);
            _camera = VRCCameraSettings.ScreenCamera;

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(_camera)) { return; }
            Vector3 position = _camera.Position;

            foreach (CanvasGroup canvasGroup in _canvasGroups)
            {
                if (!canvasGroup) { continue; }

                Vector3 targetPoint = canvasGroup.transform.position;
                float distance = Vector3.Distance(targetPoint, position);

                float calculateFade = _enableFade ?
                    _fadeCurve.Evaluate(distance) :
                    Mathf.Sign(_fadeEnd - distance);

                canvasGroup.alpha = Mathf.Clamp01(calculateFade);
                canvasGroup.interactable = calculateFade > 0.0f;
            }
        }
    }
}
