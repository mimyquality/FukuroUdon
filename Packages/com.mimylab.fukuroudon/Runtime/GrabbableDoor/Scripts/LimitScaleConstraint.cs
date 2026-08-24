/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using MimyLab.FukuroUdon;

namespace MimyLab.CombatAssemblyToolit
{
    using UdonSharp;
    using UnityEngine;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limit-scale-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limit Constraint/Limit Scale Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitScaleConstraint : LimitConstraint
    {


        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) return;



           _initialized = true;
        }
        private void Start()
        {
            Initialize();


        }
    }
}