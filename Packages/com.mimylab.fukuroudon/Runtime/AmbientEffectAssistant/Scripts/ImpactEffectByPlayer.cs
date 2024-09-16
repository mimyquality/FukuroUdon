/*
Copyright (c) 2024 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    //using VRC.Udon;
    //using VRC.SDK3.Components;

    [AddComponentMenu("Fukuro Udon/Ambient Effect Assistant/Impact Effect by Player")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ImpactEffectByPlayer : ImpactEffect
    {
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            Debug.Log("On Impact Player");
            if (!Utilities.IsValid(player)) { return; }

            PlayEffect(player.GetPosition(), player.GetVelocity());
        }
    }
}
