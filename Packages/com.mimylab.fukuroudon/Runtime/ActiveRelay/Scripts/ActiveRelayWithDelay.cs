/*
Copyright (c) 2025 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-with-delay")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay with Delay")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayWithDelay : UdonSharpBehaviour
    {
        [SerializeField, Min(0.0f), Tooltip("sec, No delay if 0")]
        private float _delayTimeToInactive = 0.0f;
        [SerializeField, Min(0.0f), Tooltip("sec, No delay if 0")]
        private float _delayTimeToActive = 0.0f;

        private int _activateDelayedCount = 0;
        private int _deactivateDelayedCount = 0;

        private void OnEnable()
        {
            if (_delayTimeToInactive > 0.0f)
            {
                _deactivateDelayedCount++;
                SendCustomEventDelayedSeconds(nameof(_DeactivateDelayed), _delayTimeToInactive);
            }
        }

        private void OnDisable()
        {

            if (_delayTimeToActive > 0.0f)
            {
                _activateDelayedCount++;
                SendCustomEventDelayedSeconds(nameof(_ActivateDelayed), _delayTimeToActive);
            }
        }

        public void _ActivateDelayed()
        {
            _activateDelayedCount--;

            if (_activateDelayedCount < 1)
            {
                _activateDelayedCount = 0;
                this.gameObject.SetActive(true);
            }
        }

        public void _DeactivateDelayed()
        {
            _deactivateDelayedCount--;

            if (_deactivateDelayedCount < 1)
            {
                _deactivateDelayedCount = 0;
                this.gameObject.SetActive(false);
            }
        }
    }
}
