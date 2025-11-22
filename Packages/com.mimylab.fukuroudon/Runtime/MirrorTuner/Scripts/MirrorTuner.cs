/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Mirror-Tuner#mirror-tuner")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Mirror Tuner/Mirror Tuner")]
    [RequireComponent(typeof(VRCMirrorReflection))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorTuner : UdonSharpBehaviour
    {
        [SerializeField]
        private MirrorProfile[] _profileList;

        private VRCMirrorReflection _mirror;
        private Renderer _renderer;

        private Material _defaultMaterial;
        private Material _customMaterial = null;


        /******************************
         VRCMirrorReflection Alies Property
         ******************************/
        public bool DisablePixelLights
        {
            get { Initialize(); return _mirror.m_DisablePixelLights; }
            set { Initialize(); _mirror.m_DisablePixelLights = value; }
        }

        public bool TurnOffMirrorOcclusion
        {
            get { Initialize(); return _mirror.TurnOffMirrorOcclusion; }
            set { Initialize(); _mirror.TurnOffMirrorOcclusion = value; }
        }

        public LayerMask ReflectLayers
        {
            get { Initialize(); return _mirror.m_ReflectLayers; }
            set { Initialize(); _mirror.m_ReflectLayers = value; }
        }

        public MirrorClearFlags CameraClearFlags
        {
            get { Initialize(); return _mirror.cameraClearFlags; }
            set { Initialize(); _mirror.cameraClearFlags = value; }
        }

        public Material CustomSkybox
        {
            get { Initialize(); return _mirror.customSkybox; }
            set { Initialize(); _mirror.customSkybox = value; }
        }

        public Color CustomClearColor
        {
            get { Initialize(); return _mirror.customClearColor; }
            set { Initialize(); _mirror.customClearColor = value; }
        }

        /******************************
         Extend Property
         ******************************/
        public Material CustomMaterial
        {
            get { Initialize(); return _customMaterial; }
            set
            {
                Initialize();

                _customMaterial = value;

                if (!_isStarted) { return; }
                if (!_renderer) { return; }

                _renderer.sharedMaterial = value ? value : _defaultMaterial;
            }
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _mirror = GetComponent<VRCMirrorReflection>();
            _renderer = _mirror.GetComponent<MeshRenderer>();
            _defaultMaterial = _renderer.sharedMaterial;

            _initialized = true;
        }
        private void Start()
        {
            // セッター内で Initialize() されるので省略
            //Initialize();

            _isStarted = true;
            CustomMaterial = _customMaterial;
        }

        private bool _isStarted = false;

        public void SetProfile0() { SetProfile(0); }
        public void SetProfile1() { SetProfile(1); }
        public void SetProfile2() { SetProfile(2); }
        public void SetProfile3() { SetProfile(3); }
        public void SetProfile4() { SetProfile(4); }
        public void SetProfile5() { SetProfile(5); }
        public void SetProfile6() { SetProfile(6); }
        public void SetProfile7() { SetProfile(7); }
        public void SetProfile8() { SetProfile(8); }
        public void SetProfile9() { SetProfile(9); }
        public void SetProfile(int number)
        {
            if (number < 0) { return; }
            if (number >= _profileList.Length) { return; }
            if (!_profileList[number]) { return; }

            // セッター内で Initialize() されるので省略
            //Initialize();

            DisablePixelLights = _profileList[number].disablePixelLights;
            TurnOffMirrorOcclusion = _profileList[number].turnOffMirrorOcclusion;
            ReflectLayers = _profileList[number].reflectLayers;
            CameraClearFlags = _profileList[number].cameraClearFlags;
            CustomSkybox = _profileList[number].customSkybox;
            CustomClearColor = _profileList[number].customCrearColor;
            CustomMaterial = _profileList[number].customMaterial;
        }
    }
}
