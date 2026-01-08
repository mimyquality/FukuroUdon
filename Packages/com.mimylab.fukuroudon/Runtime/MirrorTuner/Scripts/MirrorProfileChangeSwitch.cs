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
    //using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Mirror-Tuner#mirrorprofile-change-switch")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Mirror Tuner/MirrorProfile Change Switch")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorProfileChangeSwitch : UdonSharpBehaviour
    {
        [SerializeField]
        private MirrorTuner _mirrorTuner;

        [SerializeField]
        private int _targetNumber = 0;

        public override void Interact()
        {
            SetProfile();
        }

        public void SetProfile()
        {
            if (_mirrorTuner)
            {
                _mirrorTuner.SetProfile(_targetNumber);
            }
        }
    }
}
