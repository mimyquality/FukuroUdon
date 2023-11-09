/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    //using VRC.SDKBase;
    //using VRC.Udon;
    using VRC.SDK3.Components;

    [RequireComponent(typeof(VRCMirrorReflection))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorTuner : UdonSharpBehaviour
    {
        [SerializeField]
        private VRCMirrorReflection _mirror;

        [SerializeField]
        private MirrorProfile[] _profileList;

        private MeshRenderer _mesh;

        /******************************
         VRCMirrorReflection Alies Property
         ******************************/
        public bool DisablePixelLights
        {
            get
            {
                Initialize();
                return _mirror.m_DisablePixelLights;
            }
            set
            {
                Initialize();
                _mirror.m_DisablePixelLights = value;
            }
        }
        public bool TurnOffMirrorOcclusion
        {
            get
            {
                Initialize();
                return _mirror.TurnOffMirrorOcclusion;
            }
            set
            {
                Initialize();
                _mirror.TurnOffMirrorOcclusion = value;
            }
        }
        public LayerMask ReflectLayers
        {
            get
            {
                Initialize();
                return _mirror.m_ReflectLayers;
            }
            set
            {
                Initialize();
                _mirror.m_ReflectLayers = value;
            }
        }
        public MirrorClearFlags CameraClearFlags
        {
            get
            {
                Initialize();
                return _mirror.cameraClearFlags;
            }
            set
            {
                Initialize();
                _mirror.cameraClearFlags = value;
            }
        }
        public Material CustomSkybox
        {
            get
            {
                Initialize();
                return _mirror.customSkybox;
            }
            set
            {
                Initialize();
                _mirror.customSkybox = value;
            }
        }
        public Color CustomClearColor
        {
            get
            {
                Initialize();
                return _mirror.customClearColor;
            }
            set
            {
                Initialize();
                _mirror.customClearColor = value;
            }
        }

        /******************************
         Extend Property
         ******************************/
        public Material CustomMaterial
        {
            get
            {
                Initialize();
                return _mesh.material;
            }
            set
            {
                Initialize();
                _mesh.material = value;
            }
        }

        private void Reset()
        {
            // ランタイムでGetComponentできないので
            _mirror = GetComponent<VRCMirrorReflection>();
        }

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _mesh = _mirror.GetComponent<MeshRenderer>();

            _initialized = true;
        }
        private void Start()
        {
            Initialize();
        }

        public void SetProfile0() { SetProfile(0); }
        public void SetProfile1() { SetProfile(1); }
        public void SetProfile2() { SetProfile(2); }
        public void SetProfile3() { SetProfile(3); }
        public void SetProfile4() { SetProfile(4); }
        public void SetProfile(int num)
        {
            if (0 > num || num >= _profileList.Length) { return; }
            Initialize();

            _mirror.m_DisablePixelLights = _profileList[num].disablePixelLights;
            _mirror.TurnOffMirrorOcclusion = _profileList[num].turnOffMirrorOcclusion;
            _mirror.m_ReflectLayers = _profileList[num].reflectLayers;
            _mirror.cameraClearFlags = _profileList[num].cameraClearFlags;
            _mirror.customSkybox = _profileList[num].customSkybox;
            _mirror.customClearColor = _profileList[num].customCrearColor;

            if (_profileList[num].customMaterial) { _mesh.material = _profileList[num].customMaterial; }
        }


    }
}
