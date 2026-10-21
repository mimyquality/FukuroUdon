/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace MimyLab.FukuroUdon
{
    [CustomEditor(typeof(LimitedRotationConstraint))]
    public class LimitedRotationConstraintEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.UpdateIfRequiredOrScript();

            var excludes = new string[4];
            var excludeCount = 0;

            excludes[excludeCount++] = "m_Script";

            switch (serializedObject.FindProperty("_limitType").enumValueIndex)
            {
                case (int)RotationLimitType.Rotate:
                    excludes[excludeCount++] = "_xAxisRange";
                    excludes[excludeCount++] = "_yAxisRange";
                    excludes[excludeCount++] = "_zAxisRange";
                    break;
                case (int)RotationLimitType.EulerAngles:
                    excludes[excludeCount++] = "_maxAngle";
                    break;
            }

            //System.Array.Resize(ref excludes, excludeCount);

            DrawPropertiesExcluding(serializedObject, excludes);

            serializedObject.ApplyModifiedProperties();
        }

        private static readonly Color Alpha = new(1f, 1f, 1f, 0.4f);

        private void OnSceneGUI()
        {
            var targetComponent = target as LimitedRotationConstraint;
            if (!targetComponent) return;

            if (serializedObject.FindProperty("_limitType").enumValueIndex == (int)RotationLimitType.EulerAngles)
            {
                var transform = serializedObject.FindProperty("_targetTransform").objectReferenceValue as Transform;
                if (!transform)
                {
                    transform = targetComponent.transform;
                }

                Vector3 center = transform.position;
                float handleSize = HandleUtility.GetHandleSize(center) * 0.5f;

                var relativeTo = (Space)serializedObject.FindProperty("_relativeTo").enumValueIndex;
                Transform parent = transform.parent;
                Quaternion rotation = relativeTo == Space.Self && parent ? parent.rotation : Quaternion.identity;

                // X軸
                Vector3 normal = rotation * Vector3.right;
                Vector2 range = serializedObject.FindProperty("_xAxisRange").vector2Value;
                Vector3 from = Quaternion.AngleAxis(range.x, normal) * rotation * Vector3.forward;

                Handles.color = Color.red;
                DrawAxisRange(center, normal, from, range, handleSize);

                // Y軸
                normal = rotation * Vector3.up;
                range = serializedObject.FindProperty("_yAxisRange").vector2Value;
                from = Quaternion.AngleAxis(range.x, normal) * rotation * Vector3.forward;

                Handles.color = Color.green;
                DrawAxisRange(center, normal, from, range, handleSize);

                // Z軸
                normal = rotation * Vector3.forward;
                range = serializedObject.FindProperty("_zAxisRange").vector2Value;
                from = Quaternion.AngleAxis(range.x, normal) * rotation * Vector3.up;

                Handles.color = Color.blue;
                DrawAxisRange(center, normal, from, range, handleSize);
            }
        }

        private static void DrawAxisRange(Vector3 center, Vector3 normal, Vector3 from, Vector2 range, float handleSize)
        {
            float angle = range.y - range.x;
            if (angle < 360f)
            {
                Handles.DrawWireDisc(center, normal, handleSize);
                Handles.color *= Alpha;
                Handles.DrawSolidArc(center, normal, from, angle, handleSize);
            }
            else
            {
                Handles.color *= Alpha;
                Handles.DrawSolidDisc(center, normal, handleSize);
            }
        }
    }
}