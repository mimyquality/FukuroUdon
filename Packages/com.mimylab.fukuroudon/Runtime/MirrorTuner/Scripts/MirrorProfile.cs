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

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Mirror-Tuner#mirror-profile")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Mirror Tuner/Mirror Profile")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorProfile : UdonSharpBehaviour
    {
        public bool disablePixelLights = true;
        public bool turnOffMirrorOcclusion = true;
        public LayerMask reflectLayers = 0b01111111110001100010101100001001;
        public Material customMaterial = null;
        public MirrorClearFlags cameraClearFlags = default;
        public Material customSkybox = null;
        public Color customCrearColor = Color.black;
    }
}
