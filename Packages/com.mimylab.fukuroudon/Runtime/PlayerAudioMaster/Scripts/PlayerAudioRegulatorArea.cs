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
        [Header("Bounds Settings")]
        [SerializeField, Tooltip("Only Sphere, Capsule, Box, and Convexed Mesh Colliders")]
        private Collider[] _area = new Collider[0];
        [SerializeField]
        private bool _areaIsStatic = true;

        private Bounds _areaBounds;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            var collider = GetComponent<Collider>();
            if (collider)
            {
                if (System.Array.IndexOf(_area, collider) < 0)
                {
                    Collider[] tmp_area = new Collider[_area.Length + 1];
                    _area.CopyTo(tmp_area, 0);
                    tmp_area[_area.Length] = collider;
                    _area = tmp_area;
                }
            }
        }
#endif

        private void OnEnable()
        {
            RecalculateAreaBounds();
        }

        public void RecalculateAreaBounds()
        {
            var compoundMin = Vector3.positiveInfinity;
            var compoundMax = Vector3.negativeInfinity;
            foreach (Collider collider in _area)
            {
                if (!collider) { continue; }

                Bounds bounds = collider.bounds;
                if (bounds.extents.Equals(Vector3.zero)) { continue; }

                compoundMin = Vector3.Min(compoundMin, bounds.min);
                compoundMax = Vector3.Max(compoundMax, bounds.max);
            }

            if (compoundMin.Equals(Vector3.positiveInfinity))
            {
                _areaBounds = new Bounds();
                return;
            }

            Vector3 center = (compoundMin + compoundMax) / 2f;
            Vector3 size = compoundMax - compoundMin;
            _areaBounds = new Bounds(center, size);
        }

        protected override bool CheckUniqueApplicable(VRCPlayerApi target)
        {
            Vector3 position = target.GetPosition();
            bool isIn = _areaIsStatic ?
                        _areaBounds.Contains(position) && CheckInArea(position) :
                        CheckInArea(position);

            return isIn;
        }

        private bool CheckInArea(Vector3 position)
        {
            foreach (Collider collider in _area)
            {
                if (!collider) { continue; }
                if (!collider.enabled) { continue; }
                if (!collider.gameObject.activeInHierarchy) { continue; }

                Vector3 point = collider.ClosestPoint(position);
                if (point == position)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
