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
    //using VRC.SDK3.Components;

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorProfileChangeSwitch : UdonSharpBehaviour
    {
        [SerializeField]
        private MirrorTuner _mirrorTuner;

        [SerializeField]
        private int _targetNumber = 0;

        public override void Interact()
        {
            if (!_mirrorTuner) { return; }

            _mirrorTuner.SetProfile(_targetNumber);
        }
    }
}
