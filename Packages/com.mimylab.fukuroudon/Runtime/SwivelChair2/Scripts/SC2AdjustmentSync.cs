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
    using VRC.SDK3.Components;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Swivel Chair 2/SC2 Adjustment Sync")]
    [RequireComponent(typeof(VRCPlayerObject), typeof(VRCEnablePersistence))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SC2AdjustmentSync : UdonSharpBehaviour
    {
        public bool enableLink = false;
        public int linkNumber = 0;

        [UdonSynced]
        internal bool hasSaved = false;
        [UdonSynced]
        private Vector3 _localOffset;

        private SC2SeatAdjuster _adjuster;
        private SC2AdjustmentSync[] _linkedAdjustmentSyncs = null;

        // SC2SeatAdjusterとのやりとり用、FieldChangeCallbackで連携しない
        public Vector3 LocalOffset
        {
            get => _localOffset;
            set
            {
                SetLocalOffset(value);

                if (enableLink)
                {
                    if (_linkedAdjustmentSyncs == null) { _linkedAdjustmentSyncs = GetLinkedAdjustmentSyncs(); }

                    foreach (var linkee in _linkedAdjustmentSyncs)
                    {
                        linkee.LinkLocalOffset(this);
                    }
                }
            }
        }

        private void Start()
        {
            if (!Networking.IsOwner(this.gameObject))
            {
                this.gameObject.SetActive(false);
                return;
            }

            _adjuster = GetComponentInParent<SwivelChair2>(true).seatAdjuster;
            _adjuster.adjustmentSync = this;
        }

        public void LinkLocalOffset(SC2AdjustmentSync linker)
        {
            SetLocalOffset(linker.LocalOffset);
        }

        private void SetLocalOffset(Vector3 value)
        {
            _localOffset = value;
            hasSaved = true;
            RequestSerialization();
        }

        private SC2AdjustmentSync[] GetLinkedAdjustmentSyncs()
        {
            var playerObjects = Networking.LocalPlayer.GetPlayerObjects();
            var linkedAdjustmentSyncs = new SC2AdjustmentSync[playerObjects.Length];
            var count = 0;
            foreach (var playerObject in playerObjects)
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
