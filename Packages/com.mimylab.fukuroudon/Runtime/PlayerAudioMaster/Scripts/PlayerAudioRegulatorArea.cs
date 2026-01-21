/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/PlayerAudio-Master#pa-regulator-area")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/PlayerAudio Master/PA Regulator Area")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerAudioRegulatorArea : PlayerAudioRegulator
    {
        private Collider _collider;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            this.gameObject.layer = 5;
            if (!GetComponent<Collider>())
            {
                _collider = this.gameObject.AddComponent<BoxCollider>();
                _collider.isTrigger = true;
            }
        }
#endif

        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized) { return; }

            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;

            _initialized = true;
        }

        protected override bool CheckUniqueApplicable(VRCPlayerApi target)
        {
            Initialize();

            if (!_collider.enabled) { return false; }

            Vector3 pos = target.GetPosition();

            return _collider.bounds.Contains(pos) && (pos == _collider.ClosestPoint(pos));
        }
    }
}
