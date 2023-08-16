/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
//using VRC.SDKBase;
//using VRC.Udon;

namespace MimyLab
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class SCKeyInputManager : UdonSharpBehaviour
    {
        private SwivelChair _swivelChair;

        void Start()
        {
            _swivelChair = GetComponent<SwivelChair>();
        }

        private void Update()
        {
            _swivelChair._OnUpdate();
        }
    }
}
