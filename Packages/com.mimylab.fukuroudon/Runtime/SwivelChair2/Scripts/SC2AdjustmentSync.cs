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
    using VRC.Udon;
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
        private Vector3 _localOffset;

        private SC2SeatAdjuster _adjuster;
        private SC2AdjustmentSync[] _linkedAdjustmentSyncs = null;

        // SC2SeatAdjusterとのやりとり用、FieldChangeCallbackで連携しない
        public Vector3 LocalOffset
        {
            get => _localOffset;
            set
            {
                _localOffset = value;
                RequestSerialization();

                if (enableLink)
                {
                    if (_linkedAdjustmentSyncs == null) { _linkedAdjustmentSyncs = GetLinkedAdjustmentSyncs(); }

                    foreach (var linkedAS in _linkedAdjustmentSyncs)
                    {
                        linkedAS.LinkLocalOffset(value);
                    }
                }
            }
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }
            if (!player.IsOwner(this.gameObject)) { return; }

            _adjuster = GetComponentInParent<SC2SeatAdjuster>(true);
            _adjuster.adjustmentSync = this;
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

        public void LinkLocalOffset(Vector3 offset)
        {
            _localOffset = offset;
            RequestSerialization();
        }
    }
}
