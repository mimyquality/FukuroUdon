/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UdonSharpEditor;
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
    }
}