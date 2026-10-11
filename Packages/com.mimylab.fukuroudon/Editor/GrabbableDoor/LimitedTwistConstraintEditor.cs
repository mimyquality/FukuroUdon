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
    [CustomEditor(typeof(LimitedTwistConstraint))]
    public class LimitedTwistConstraintEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.UpdateIfRequiredOrScript();

            var excludes = new string[2];
            var excludeCount = 0;

            excludes[excludeCount++] = "m_Script";

            switch (serializedObject.FindProperty("_sourceAimType").enumValueIndex)
            {
                case (int)SourceAimType.ObjectAim:
                    excludes[excludeCount++] = "_sourceAimVector";
                    break;
            }

            //System.Array.Resize(ref excludes, excludeCount);

            DrawPropertiesExcluding(serializedObject, excludes);

            serializedObject.ApplyModifiedProperties();
        }

        private static readonly Color Alpha = new(1f, 1f, 1f, 0.4f);

        private void OnSceneGUI()
        {
            var targetComponent = target as LimitedTwistConstraint;
            if (!targetComponent) return;

            var transform = serializedObject.FindProperty("_targetTransform").objectReferenceValue as Transform;
            if (!transform)
            {
                transform = targetComponent.transform;
            }

            Vector3 center = transform.position;
            float handleSize = HandleUtility.GetHandleSize(center) * 0.5f;

            Quaternion rotation = transform.rotation;
            Vector3 normal = rotation * serializedObject.FindProperty("_twistAxis").vector3Value;
            normal.Normalize();

            Handles.color = Color.cyan;
            Handles.DrawLine(center, HandleUtility.GetHandleSize(center) * normal + center);

            Vector2 range = serializedObject.FindProperty("_angleRange").vector2Value;
            Vector3 from = rotation * Vector3.forward;
            var sourceTransform = serializedObject.FindProperty("_sourceTransform").objectReferenceValue as Transform;
            if (sourceTransform)
            {
                switch (serializedObject.FindProperty("_sourceAimType").enumValueIndex)
                {
                    case (int)SourceAimType.ObjectAim:
                        from = sourceTransform.position - center;
                        break;
                    case (int)SourceAimType.ObjectRotationAim:
                        from =
                            sourceTransform.rotation * serializedObject.FindProperty("_sourceAimVector").vector3Value;
                        break;
                }
            }
            
            from = Quaternion.AngleAxis(range.x, normal) * Quaternion.LookRotation(normal, from) * Vector3.up;

            DrawAxisRange(center, normal, from, range, handleSize);
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