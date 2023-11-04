/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace MimyLab
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorToggleSwitch : UdonSharpBehaviour
    {
        [SerializeField]
        GameObject[] _targetList;

        public override void Interact()
        {
            for (int i = 0; i < _targetList.Length; i++)
            {
                if (_targetList[i])
                {
                    _targetList[i].SetActive(!_targetList[i].activeSelf);
                }
            }
        }
    }
}
