/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;

namespace MimyLab.FukuroUdon
{
    public partial class DONTweenPath
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            var tmp_waypoints = new Transform[waypoints.Length];
            var waypointsCount = 0;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i])
                {
                    tmp_waypoints[waypointsCount++] = waypoints[i];
                }
            }
            if (waypointsCount != waypoints.Length)
            {
                System.Array.Resize(ref tmp_waypoints, waypointsCount);
                waypoints = tmp_waypoints;
            }
        }

        private void OnDrawGizmos()
        {
            if (_waypointsPosition.Length != waypoints.Length)
            {
                _waypointsPosition = new Vector3[waypoints.Length];
            }
            for (int i = 0; i < waypoints.Length; i++)
            {
                _waypointsPosition[i] = _relativeTo == Space.World ? waypoints[i].position : waypoints[i].localPosition;
            }

            if (_waypointsPosition.Length > 1)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLineStrip(_waypointsPosition, closePath);
            }
        }
#endif
    }
}
