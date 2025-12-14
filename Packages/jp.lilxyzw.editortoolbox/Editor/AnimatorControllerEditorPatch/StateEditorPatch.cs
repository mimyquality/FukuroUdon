#if LIL_HARMONY
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static jp.lilxyzw.editortoolbox.AnimatorControllerEditorPatch;

namespace jp.lilxyzw.editortoolbox
{
    // State複数選択時にプロパティがコピーされてしまうUnityのバグの修正
    [HarmonyPatch]
    internal class StateEditorOnParametrizedValueGUIPatch
    {
        public static Type T_StateEditor = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.StateEditor");
        public static MethodInfo MI_TextFieldDropDown = typeof(EditorGUILayout).GetMethod("TextFieldDropDown", BindingFlags.Static | BindingFlags.NonPublic, null, new[] {
            typeof(GUIContent), typeof(string), typeof(string[]) },
        null);
        public static PropertyInfo PI_controllerContext = T_StateEditor.GetProperty("controllerContext", BindingFlags.Instance | BindingFlags.NonPublic);
        public static MethodInfo MI_CollectParameters = T_StateEditor.GetMethod("CollectParameters", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_StateEditor, "OnParametrizedValueGUI");

        [HarmonyPostfix]
        private static bool Prefix(object __instance, string name, SerializedProperty value, SerializedProperty valueParameter, SerializedProperty valueParameterActive, AnimatorControllerParameterType parameterType)
        {
            if (!EditorToolboxSettings.instance.fixStateMultipleEdit) return true;
            var controllerContext = PI_controllerContext.GetValue(__instance) as AnimatorController;
            if (value != null) EditorGUILayout.PropertyField(value);
            if (!controllerContext) return false;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            DrawParameter(__instance, "Multiplier", value, valueParameter, valueParameterActive, parameterType, controllerContext);
            EditorGUI.indentLevel--;
            DrawToggle(valueParameterActive);
            EditorGUILayout.EndHorizontal();

            return false;
        }

        public static void DrawParameter(object __instance, string name, SerializedProperty value, SerializedProperty valueParameter, SerializedProperty valueParameterActive, AnimatorControllerParameterType parameterType, AnimatorController controllerContext)
        {
            var list = MI_CollectParameters.Invoke(__instance, new object[] { controllerContext, parameterType }) as List<string>;
            if (list.Count == 0 && valueParameterActive.boolValue)
            {
                EditorGUILayout.HelpBox($"Must have at least one Parameter of type {parameterType} in the AnimatorController", MessageType.Error);
            }
            else
            {
                if (!valueParameterActive.hasMultipleDifferentValues && valueParameterActive.boolValue &&
                    valueParameter.hasMultipleDifferentValues && string.IsNullOrEmpty(valueParameter.stringValue) &&
                    list.Count > 0) valueParameter.stringValue = list[0];

                EditorGUI.showMixedValue = valueParameter.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                string stringValue = MI_TextFieldDropDown.Invoke(null, new object[] { new GUIContent(name), valueParameter.stringValue, list.ToArray() }) as string;
                if (EditorGUI.EndChangeCheck()) valueParameter.stringValue = stringValue;
                EditorGUI.showMixedValue = false;
            }
        }

        public static void DrawToggle(SerializedProperty valueParameterActive)
        {
            EditorGUI.showMixedValue = valueParameterActive.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            var boolValue = EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTextContent("Parameter", "Override this constant value with an AnimatorController's parameter to animate this property at runtime."), valueParameterActive.boolValue, GUILayout.MaxWidth(100f));
            if (EditorGUI.EndChangeCheck()) valueParameterActive.boolValue = boolValue;
            EditorGUI.showMixedValue = false;
        }
    }

    [HarmonyPatch]
    internal class StateEditorOnParametrizedValueGUIOverridePatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(StateEditorOnParametrizedValueGUIPatch.T_StateEditor, "OnParametrizedValueGUIOverride");

        [HarmonyPostfix]
        private static bool Prefix(object __instance, string name, SerializedProperty value, SerializedProperty valueParameter, SerializedProperty valueParameterActive, AnimatorControllerParameterType parameterType)
        {
            if (!EditorToolboxSettings.instance.fixStateMultipleEdit) return true;
            var controllerContext = StateEditorOnParametrizedValueGUIPatch.PI_controllerContext.GetValue(__instance) as AnimatorController;
            if (!controllerContext) return true;

            EditorGUILayout.BeginHorizontal();
            if (valueParameterActive.boolValue) StateEditorOnParametrizedValueGUIPatch.DrawParameter(__instance, name, value, valueParameter, valueParameterActive, parameterType, controllerContext);
            else if (value != null) EditorGUILayout.PropertyField(value);
            else EditorGUILayout.LabelField(new GUIContent(name));
            StateEditorOnParametrizedValueGUIPatch.DrawToggle(valueParameterActive);
            EditorGUILayout.EndHorizontal();
            return false;
        }
    }
}
#endif
