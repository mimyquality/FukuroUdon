/*
Copyright (c) 2026 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UdonSharpEditor;

namespace MimyLab.FukuroUdon
{
    [CustomEditor(typeof(DONTweenDelayedActive))]
    public class DONTweenDelayedActiveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.UpdateIfRequiredOrScript();

            string[] excludes =
            {
                "m_Script",
                nameof(DONTween.fixedDuration),
                nameof(DONTween.duration),
                nameof(DONTween.easeType),
                nameof(DONTween.customEase),
            };

            DrawPropertiesExcluding(serializedObject, excludes);

            serializedObject.ApplyModifiedProperties();
        }
    }
}