#if LIL_HARMONY
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static jp.lilxyzw.editortoolbox.AnimatorControllerEditorPatch;

namespace jp.lilxyzw.editortoolbox
{
    // TransitionのInterruption Source等がコピーされないUnityのバグを修正
    [HarmonyPatch]
    internal class AnimatorTransitionInspectorBaseCopyTransitionParametersPatch
    {
        public static readonly Type T_TransitionEditionContext = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.TransitionEditionContext");
        public static readonly FieldInfo FI_transition = T_TransitionEditionContext.GetField("transition", BindingFlags.Public | BindingFlags.Instance);
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_AnimatorTransitionInspectorBase, "CopyTransitionParameters");

        [HarmonyPostfix]
        private static void Postfix(object obj)
        {
            originalTransition = FI_transition.GetValue(obj) as AnimatorTransitionBase;
        }
    }

    [HarmonyPatch]
    internal class AnimatorTransitionInspectorBasePasteTransitionParametersPatch
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods() => new[]{
            AccessTools.Method(T_AnimatorTransitionInspectorBase, "PasteTransitionParameters"),
            AccessTools.Method(T_AnimatorTransitionInspectorBase, "PasteBoth")
        };

        [HarmonyPostfix]
        private static void Postfix(object obj)
        {
            if (!EditorToolboxSettings.instance.fixCopyInterruptionSettings) return;
            var transition = AnimatorTransitionInspectorBaseCopyTransitionParametersPatch.FI_transition.GetValue(obj) as AnimatorTransitionBase;
            
            using var so = new SerializedObject(transition);
            using var soOrig = new SerializedObject(originalTransition);
            using var m_InterruptionSource = soOrig.FindProperty("m_InterruptionSource");
            using var m_OrderedInterruption = soOrig.FindProperty("m_OrderedInterruption");
            so.CopyFromSerializedProperty(m_InterruptionSource);
            so.CopyFromSerializedProperty(m_OrderedInterruption);
            so.ApplyModifiedProperties();
        }
    }

    // 遷移条件のGUIの修正
    [HarmonyPatch]
    internal class AnimatorTransitionInspectorBaseDrawConditionsElementPatch
    {
        private static readonly FieldInfo FI_m_Conditions = T_AnimatorTransitionInspectorBase.GetField("m_Conditions", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo FI_m_Controller = T_AnimatorTransitionInspectorBase.GetField("m_Controller", BindingFlags.NonPublic | BindingFlags.Instance);
        private static List<AnimatorConditionMode> m_boolModes = new() { AnimatorConditionMode.If, AnimatorConditionMode.IfNot };
        private static List<AnimatorConditionMode> m_floatModes = new() { AnimatorConditionMode.Greater, AnimatorConditionMode.Less };
        private static List<AnimatorConditionMode> m_intModes = new() { AnimatorConditionMode.Greater, AnimatorConditionMode.Less, AnimatorConditionMode.Equals, AnimatorConditionMode.NotEqual };
        private static string[] m_boolModesLabel = new[] { "true", "false" };
        private static string[] m_floatModesLabel = m_floatModes.Select(m => m.ToString()).ToArray();
        private static string[] m_intModesLabel = m_intModes.Select(m => m.ToString()).ToArray();

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_AnimatorTransitionInspectorBase, "DrawConditionsElement");

        [HarmonyPostfix]
        private static bool Prefix(object __instance, Rect rect, int index, bool selected, bool focused)
        {
            if (!EditorToolboxSettings.instance.fixTransitionConditionGUI) return true;
            var m_Conditions = FI_m_Conditions.GetValue(__instance) as SerializedProperty;
            var m_Controller = FI_m_Controller.GetValue(__instance) as AnimatorController;
            if (m_Conditions == null || !m_Controller) return true;

            var center = rect.center;
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.center = center;

            using var element = m_Conditions.GetArrayElementAtIndex(index);
            using var m_ConditionEvent = element.FindPropertyRelative("m_ConditionEvent");
            var parameters = m_Controller.parameters;
            var parameterNames = parameters.Select(p => p.name).ToList();
            var parameterType = parameters.FirstOrDefault(p => p.name == m_ConditionEvent.stringValue)?.type;

            // パラメーター選択
            var paramIndex = parameterNames.IndexOf(m_ConditionEvent.stringValue);
            EditorGUI.showMixedValue = m_ConditionEvent.hasMultipleDifferentValues;
            var newParamIndex = EditorGUIWrap.AdvancedPopup(new Rect(rect) { width = rect.width / 2 - 4 }, paramIndex, parameterNames.ToArray());
            EditorGUI.showMixedValue = false;
            var paramChanged = paramIndex != newParamIndex && newParamIndex != -1;
            if (paramChanged) m_ConditionEvent.stringValue = parameterNames[newParamIndex];

            // パラメーターの型が変わった場合に条件設定
            using var m_ConditionMode = element.FindPropertyRelative("m_ConditionMode");
            using var m_EventTreshold = element.FindPropertyRelative("m_EventTreshold");
            var newParamType = parameters.FirstOrDefault(p => p.name == m_ConditionEvent.stringValue)?.type;
            if (parameterType != newParamType)
            {
                switch (newParamType)
                {
                    case AnimatorControllerParameterType.Float: m_ConditionMode.intValue = (int)AnimatorConditionMode.Greater; break;
                    case AnimatorControllerParameterType.Int: m_ConditionMode.intValue = (int)AnimatorConditionMode.Equals; m_EventTreshold.floatValue = (int)m_EventTreshold.floatValue; break;
                    case AnimatorControllerParameterType.Bool: m_ConditionMode.intValue = (int)AnimatorConditionMode.If; break;
                    case AnimatorControllerParameterType.Trigger: m_ConditionMode.intValue = (int)AnimatorConditionMode.If; break;
                }
            }

            if (!parameterNames.Contains(m_ConditionEvent.stringValue))
            {
                EditorGUI.LabelField(new Rect(rect) { xMin = rect.xMin + rect.width / 2 }, "Parameter does not exist in Controller");
                return false;
            }

            var animatorConditionMode = (AnimatorConditionMode)m_ConditionMode.intValue;
            void ConditionsGUI(Rect rect, List<AnimatorConditionMode> modes, string[] labels)
            {
                EditorGUI.showMixedValue = m_ConditionMode.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                var newCondition = EditorGUI.Popup(rect, modes.IndexOf(animatorConditionMode), labels);
                if (EditorGUI.EndChangeCheck()) m_ConditionMode.intValue = (int)modes[newCondition];
            }

            if (parameterType == AnimatorControllerParameterType.Float)
            {
                ConditionsGUI(new Rect(rect) { xMin = rect.xMin + rect.width / 2, xMax = rect.xMax - rect.width / 4 - 4 }, m_floatModes, m_floatModesLabel);
                EditorGUI.PropertyField(new Rect(rect) { xMin = rect.xMin + rect.width / 4 * 3 }, m_EventTreshold, GUIContent.none);
            }
            else if (parameterType == AnimatorControllerParameterType.Int)
            {
                ConditionsGUI(new Rect(rect) { xMin = rect.xMin + rect.width / 2, xMax = rect.xMax - rect.width / 4 - 4 }, m_intModes, m_intModesLabel);

                EditorGUI.showMixedValue = m_EventTreshold.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                var eventTreshold = EditorGUI.IntField(new Rect(rect) { xMin = rect.xMin + rect.width / 4 * 3 }, (int)m_EventTreshold.floatValue);
                if (EditorGUI.EndChangeCheck()) m_EventTreshold.floatValue = eventTreshold;
            }
            else if (parameterType == AnimatorControllerParameterType.Bool)
            {
                ConditionsGUI(new Rect(rect) { xMin = rect.xMin + rect.width / 2 }, m_boolModes, m_boolModesLabel);
            }

            EditorGUI.showMixedValue = false;
            return false;
        }
    }
}
#endif
