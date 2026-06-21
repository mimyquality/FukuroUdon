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

            this.serializedObject.UpdateIfRequiredOrScript();
            var iterator = this.serializedObject.GetIterator();
            for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (iterator.propertyPath == "m_Script")
                {
                    continue;
                }
                if (iterator.propertyPath == nameof(DONTween.fixedDuration))
                {
                    continue;
                }
                if (iterator.propertyPath == nameof(DONTween.duration))
                {
                    continue;
                }
                if (iterator.propertyPath == nameof(DONTween.easeType))
                {
                    continue;
                }
                if (iterator.propertyPath == nameof(DONTween.customEase))
                {
                    continue;
                }if (iterator.propertyPath == nameof(DONTween.delay))
                {
                    EditorGUILayout.Space();
                }


                EditorGUILayout.PropertyField(iterator, true);
            }
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
