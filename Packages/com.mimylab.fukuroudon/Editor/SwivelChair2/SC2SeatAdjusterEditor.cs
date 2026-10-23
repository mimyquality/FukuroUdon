/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

namespace MimyLab.FukuroUdon
{
    [CustomEditor(typeof(SC2SeatAdjuster))]
    public class SC2SeatAdjusterEditor : Editor
    {
        private SC2SeatAdjuster _target;
        private VRCStation _station;

        private void OnEnable()
        {
            _target = target as SC2SeatAdjuster;
            if (!_target) return;

            _station = _target.GetComponent<VRCStation>();
        }

        private void OnSceneGUI()
        {
            if (!_target) return;
            if (!_station) return;

            Transform pointer = _station.stationEnterPlayerLocation
                ? _station.stationEnterPlayerLocation
                : _station.transform;
            Transform parent = pointer.parent;
            if (parent)
            {
                Handles.matrix = parent.localToWorldMatrix;
            }

            Vector3 point = pointer.localPosition;
            float handleSize = HandleUtility.GetHandleSize(pointer.position);
            Color handleColor = Color.cyan;
            
            Handles.color = handleColor;
            Handles.DrawLine(point, 0.25f * handleSize * Vector3.up + point);
            
            handleColor.a = 0.2f;
            Handles.color = handleColor;
            Handles.DrawSolidDisc(point, Vector3.up, 0.1f * handleSize);

            Vector3 adjustMinLimit = serializedObject.FindProperty("adjustMinLimit").vector3Value;
            Vector3 adjustMaxLimit = serializedObject.FindProperty("adjustMaxLimit").vector3Value;

            Vector3 center = (adjustMinLimit + adjustMaxLimit) * 0.5f;
            Vector3 size = adjustMaxLimit - adjustMinLimit;

            Handles.color = Color.white;
            Handles.DrawWireCube(center, size);
        }
    }
}