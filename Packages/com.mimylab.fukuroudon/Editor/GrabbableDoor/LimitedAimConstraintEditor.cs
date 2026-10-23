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

        private static readonly Color Alpha = new(1f, 1f, 1f, 0.2f);

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
            float handleSize = HandleUtility.GetHandleSize(center);

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

            Vector3 up = rotation * serializedObject.FindProperty("_upVector").vector3Value;
            up.Normalize();

            Handles.color = Color.yellow;
            Handles.DrawLine(center, handleSize * up + center);

            Handles.color = Color.cyan;
            Handles.DrawLine(center, handleSize * aim + center);

            switch (serializedObject.FindProperty("_limitType").enumValueIndex)
            {
                case (int)AimLimitType.Angle:
                    DrawAngleRange(center, aim, up, handleSize * 0.5f);
                    break;

                case (int)AimLimitType.Polar:
                    DrawPolarRange(center, aim, up, handleSize * 0.5f);
                    break;
            }
        }

        private void DrawAngleRange(Vector3 center, Vector3 aim, Vector3 up, float handleSize)
        {
            float maxAngle = serializedObject.FindProperty("_maxAngle").floatValue;
            float radian = maxAngle * Mathf.Deg2Rad;
            Vector3 angledAim = Quaternion.AngleAxis(maxAngle, Vector3.Cross(aim, up)) * aim;

            Handles.DrawWireDisc(center + Mathf.Cos(radian) * handleSize * aim, aim, Mathf.Sin(radian) * handleSize);

            Handles.color *= Alpha;
            Handles.DrawSolidArc(center, aim, angledAim, 360f, handleSize);
        }

        private void DrawPolarRange(Vector3 center, Vector3 aim, Vector3 up, float handleSize)
        {
            Vector3 pitch = Vector3.Cross(up, aim);
            Vector3 yaw = Vector3.Cross(aim, pitch);
            Vector2 pitchRange = serializedObject.FindProperty("_pitchRange").vector2Value;
            Vector2 yawRange = serializedObject.FindProperty("_yawRange").vector2Value;
            float pitchAngle = pitchRange.y - pitchRange.x;
            float yawAngle = yawRange.y - yawRange.x;

            Quaternion minPitch = Quaternion.AngleAxis(pitchRange.x, pitch);
            Quaternion maxPitch = Quaternion.AngleAxis(pitchRange.y, pitch);
            Quaternion minYaw = Quaternion.AngleAxis(yawRange.x, yaw);
            Quaternion maxYaw = Quaternion.AngleAxis(yawRange.y, yaw);

            Vector3 minYawMinPitchAim = minYaw * minPitch * aim;
            Vector3 minYawMaxPitchAim = minYaw * maxPitch * aim;
            Vector3 maxYawMinPitchAim = maxYaw * minPitch * aim;
            //Vector3 maxYawMaxPitchAim = maxYaw * maxPitch * aim;

            Color wireColor = Handles.color;
            Color solidColor = wireColor * Alpha;
            
            //Handles.color = wireColor;
            Handles.DrawWireArc(center, yaw, minYawMinPitchAim, yawAngle, handleSize);
            
            Handles.color = solidColor;
            Handles.DrawSolidArc(center, yaw, minYawMinPitchAim, yawAngle, handleSize);

            if (0f < pitchAngle && pitchAngle < 180f)
            {
                Handles.color = wireColor;
                Handles.DrawWireArc(center, yaw, minYawMaxPitchAim, yawAngle, handleSize);
                
                Handles.color = solidColor;
                Handles.DrawSolidArc(center, yaw, minYawMaxPitchAim, yawAngle, handleSize);
            }

            if (yawAngle < 360f)
            {
                Handles.color = wireColor;
                Handles.DrawWireArc(center, minYaw * pitch, minYawMinPitchAim, pitchAngle, handleSize);
                
                Handles.color = solidColor;
                Handles.DrawSolidArc(center, minYaw * pitch, minYawMinPitchAim, pitchAngle, handleSize);

                if (0f < yawAngle)
                {
                    Handles.color = wireColor;
                    Handles.DrawWireArc(center, maxYaw * pitch, maxYawMinPitchAim, pitchAngle, handleSize);
                    
                    Handles.color = solidColor;
                    Handles.DrawSolidArc(center, maxYaw * pitch, maxYawMinPitchAim, pitchAngle, handleSize);
                }
            }
        }
    }
}