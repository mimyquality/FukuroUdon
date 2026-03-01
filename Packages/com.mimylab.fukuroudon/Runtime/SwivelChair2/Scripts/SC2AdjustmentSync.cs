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
    using VRC.SDK3.Components;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Swivel-Chair-2#%E5%BA%A7%E4%BD%8D%E7%BD%AE%E4%BF%9D%E5%AD%98%E6%A9%9F%E8%83%BD-ver300-%E4%BB%A5%E9%99%8D")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/SC2 Adjustment Sync")]
    [RequireComponent(typeof(VRCPlayerObject), typeof(VRCEnablePersistence))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SC2AdjustmentSync : UdonSharpBehaviour
    {
        public bool enableLink = false;
        public int linkNumber = 0;

        [UdonSynced]
        internal bool _hasSaved = false;
        [UdonSynced]
        internal Vector3 _localOffset = Vector3.zero;
        [UdonSynced]
        internal float _avatarEyeHeight = 0.0f;

        private SC2SeatAdjuster _adjuster;
        private SC2AdjustmentSync[] _linkedAdjustmentSyncs = null;

        private void Start()
        {
            if (!Networking.IsOwner(this.gameObject))
            {
                this.gameObject.SetActive(false);
                return;
            }

            _adjuster = GetComponentInParent<SwivelChair2>(true)._seatAdjuster;
            _adjuster._adjustmentSync = this;
        }

        public void Save(Vector3 offset, float avatarEyeHeight)
        {
            SetParameters(offset, avatarEyeHeight);

            if (enableLink)
            {
                if (_linkedAdjustmentSyncs == null) { _linkedAdjustmentSyncs = GetLinkedAdjustmentSyncs(); }

                foreach (SC2AdjustmentSync linkee in _linkedAdjustmentSyncs)
                {
                    linkee.LinkLocalOffset(this);
                }
            }
        }

        public void LinkLocalOffset(SC2AdjustmentSync linker)
        {
            SetParameters(linker._localOffset, linker._avatarEyeHeight);
        }

        private void SetParameters(Vector3 localOffset, float avatarEyeHeight)
        {
            _localOffset = localOffset;
            _avatarEyeHeight = avatarEyeHeight;
            _hasSaved = true;
            RequestSerialization();
        }

        private SC2AdjustmentSync[] GetLinkedAdjustmentSyncs()
        {
            GameObject[] playerObjects = Networking.LocalPlayer.GetPlayerObjects();
            var linkedAdjustmentSyncs = new SC2AdjustmentSync[playerObjects.Length];
            var count = 0;
            foreach (GameObject playerObject in playerObjects)
            {
                var adjustmentSync = playerObject.GetComponent<SC2AdjustmentSync>();
                if (adjustmentSync &&
                    adjustmentSync != this &&
                    adjustmentSync.enableLink &&
                    adjustmentSync.linkNumber == linkNumber)
                {
                    linkedAdjustmentSyncs[count++] = adjustmentSync;
                }
            }

            var result = new SC2AdjustmentSync[count];
            System.Array.Copy(linkedAdjustmentSyncs, result, count);

            return result;
        }
    }
}
