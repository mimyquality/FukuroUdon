/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace MimyLab.FukuroUdon
{
    [CustomEditor(typeof(LimitedAimConstraint))]
    public class LimitedAimConstraintEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.UpdateIfRequiredOrScript();

            var excludes = new string[6];
            var excludeCount = 0;

            excludes[excludeCount++] = "m_Script";

            switch (serializedObject.FindProperty("_sourceAimType").enumValueIndex)
            {
                case (int)SourceAimType.ObjectAim:
                    excludes[excludeCount++] = "_sourceAimVector";
                    break;
            }

            switch (serializedObject.FindProperty("_worldUpType").enumValueIndex)
            {
                case (int)AimConstraint.WorldUpType.SceneUp:
                case (int)AimConstraint.WorldUpType.None:
                    excludes[excludeCount++] = "_worldUpObject";
                    excludes[excludeCount++] = "_worldUpVector";
                    break;
                case (int)AimConstraint.WorldUpType.ObjectUp:
                    excludes[excludeCount++] = "_worldUpVector";
                    break;
                case (int)AimConstraint.WorldUpType.Vector:
                    excludes[excludeCount++] = "_worldUpObject";
                    break;
            }

            switch (serializedObject.FindProperty("_limitType").enumValueIndex)
            {
                case (int)AimLimitType.Angle:
                    excludes[excludeCount++] = "_yawRange";
                    excludes[excludeCount++] = "_pitchRange";
                    break;
                case (int)AimLimitType.Polar:
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
            var targetComponent = target as LimitedAimConstraint;
            if (!targetComponent) return;

            var transform = serializedObject.FindProperty("_targetTransform").objectReferenceValue as Transform;
            if (!transform)
            {
                transform = targetComponent.transform;
            }

            Vector3 center = transform.position;
            float handleSize = HandleUtility.GetHandleSize(center) * 0.5f;

            Quaternion rotation = transform.rotation;
            Vector3 aim = rotation * Vector3.forward;
            var sourceTransform = serializedObject.FindProperty("_sourceTransform").objectReferenceValue as Transform;
            if (sourceTransform)
            {
                switch (serializedObject.FindProperty("_sourceAimType").enumValueIndex)
                {
                    case (int)SourceAimType.ObjectAim:
                        aim = sourceTransform.position - center;
                        break;
                    case (int)SourceAimType.ObjectRotationAim:
                        aim =
                            sourceTransform.rotation * serializedObject.FindProperty("_sourceAimVector").vector3Value;
                        break;
                }

                aim.Normalize();
            }

            Handles.color = Color.cyan;
            Handles.DrawLine(center, HandleUtility.GetHandleSize(center) * aim + center);

            Vector3 up = rotation * serializedObject.FindProperty("_upVector").vector3Value;
            up.Normalize();

            Handles.color = Color.yellow;
            Handles.DrawLine(center, HandleUtility.GetHandleSize(center) * up + center);

            switch (serializedObject.FindProperty("_limitType").enumValueIndex)
            {
                case (int)AimLimitType.Angle:
                    float maxAngle = serializedObject.FindProperty("_maxAngle").floatValue;
                    Vector3 angledAim = Quaternion.AngleAxis(maxAngle, Vector3.Cross(aim, up)) * aim;
                    Handles.color = Color.cyan;
                    Handles.color *= Alpha;
                    Handles.DrawSolidArc(center, aim, angledAim, 360f, handleSize);
                    break;

                case (int)AimLimitType.Polar:
                    DrawPolarRange(center, aim, up, handleSize);
                    break;
            }
        }

        private void DrawPolarRange(Vector3 center, Vector3 aim, Vector3 up, float handleSize)
        {
            Handles.color = Color.cyan * Alpha;
            
            Vector3 right = Vector3.Cross(up, aim);
            Vector3 yaw = Vector3.Cross(aim, right);
            Vector2 pitchRange = serializedObject.FindProperty("_pitchRange").vector2Value;
            Vector2 yawRange = serializedObject.FindProperty("_yawRange").vector2Value;
            float pitchAngle = pitchRange.y - pitchRange.x;
            float yawAngle = yawRange.y - yawRange.x;

            Quaternion minPitch = Quaternion.AngleAxis(pitchRange.x, right);
            Quaternion maxPitch = Quaternion.AngleAxis(pitchRange.y, right);
            Quaternion minYaw = Quaternion.AngleAxis(yawRange.x, yaw);
            Quaternion maxYaw = Quaternion.AngleAxis(yawRange.y, yaw);

            Vector3 corner;
            
            corner = minYaw * minPitch * aim;

            Handles.DrawSolidArc(center, yaw, corner, yawAngle, handleSize);

            if (0f < pitchAngle && pitchAngle < 180f)
            {
                corner = minYaw * maxPitch * aim;

                Handles.DrawSolidArc(center, yaw, corner, yawAngle, handleSize);
            }
            
            if(yawAngle < 360f)
            {
                corner = minYaw * minPitch * aim;

                Handles.DrawSolidArc(center, minYaw * right, corner, pitchAngle, handleSize);

                if (0f < yawAngle)
                {
                    corner = maxYaw * minPitch * aim;

                    Handles.DrawSolidArc(center, maxYaw * right, corner, pitchAngle, handleSize);
                }
            }
        }
    }
}