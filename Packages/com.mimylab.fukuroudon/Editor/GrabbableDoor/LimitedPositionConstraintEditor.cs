/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UnityEngine;

namespace MimyLab.FukuroUdon
{
    [CustomEditor(typeof(LimitedPositionConstraint))]
    public class LimitedPositionConstraintEditor : Editor
    {
        private void OnSceneGUI()
        {
            var targetComponent = target as LimitedPositionConstraint;
            if (!targetComponent) return;

            var transform = serializedObject.FindProperty("_targetTransform").objectReferenceValue as Transform;
            if (!transform)
            {
                transform = targetComponent.transform;
            }

            Vector3 center = transform.position;

            Transform parent = transform.parent;
            var relativeTo = (Space)serializedObject.FindProperty("_relativeTo").enumValueIndex;
            if (relativeTo == Space.Self && parent)
            {
                Handles.matrix = parent.localToWorldMatrix;
                center = transform.localPosition;
            }

            Vector3 minPosition = serializedObject.FindProperty("_minPosition").vector3Value;
            Vector3 maxPosition = serializedObject.FindProperty("_maxPosition").vector3Value;

            center += (minPosition + maxPosition) * 0.5f;
            Vector3 size = maxPosition - minPosition;

            Handles.color = Color.white;
            Handles.DrawWireCube(center, size);
        }
    }
}