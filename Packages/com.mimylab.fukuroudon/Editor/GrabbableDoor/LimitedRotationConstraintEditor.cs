/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UdonSharpEditor;

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
    }
}