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
    }
}