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
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Grabbable-Door#limit-angle-constraint")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Limit Constraint/Limit Angle Constraint")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LimitAngleConstraint : LimitConstraint
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