#if LIL_HARMONY
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
using static jp.lilxyzw.editortoolbox.AnimatorControllerEditorPatch;

namespace jp.lilxyzw.editortoolbox
{
    [HarmonyPatch]
    internal class ParameterControllerViewPatch
    {
        private static object instance;

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_ParameterControllerView, "DoParameterList");

        [HarmonyPrefix]
        private static void PrefixDoParameterList(object __instance) => instance = __instance;

        public static void ShowReferences()
        {
            var pcv = new ParameterControllerViewWrap(instance);
            var controller = pcv.animatorController;
            var parameter = controller.parameters[pcv.m_LastSelectedIndex];
            ParameterReferenceWIndow.Show(parameter, controller);
        }
    }

    [HarmonyPatch]
    internal class ElementPatch
    {
        public static readonly Type T_FloatElement = T_ParameterControllerView.GetNestedType("FloatElement", BindingFlags.NonPublic);
        private static readonly Type T_BoolElement = T_ParameterControllerView.GetNestedType("BoolElement", BindingFlags.NonPublic);
        private static readonly Type T_IntElement = T_ParameterControllerView.GetNestedType("IntElement", BindingFlags.NonPublic);
        private static readonly Type T_TriggerElement = T_ParameterControllerView.GetNestedType("TriggerElement", BindingFlags.NonPublic);

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(ElementWrap.T_Element, "OnGUI");

        [HarmonyPrefix]
        private static void PrefixOnGUI(object __instance, ref Rect rect, int index)
        {
            if (!EditorToolboxSettings.instance.extendAnimatorControllerParameterGUI) return;
            var type = AnimatorControllerParameterType.Float;
            if (__instance.GetType() == T_BoolElement) type = AnimatorControllerParameterType.Bool;
            else if (__instance.GetType() == T_IntElement) type = AnimatorControllerParameterType.Int;
            else if (__instance.GetType() == T_TriggerElement) type = AnimatorControllerParameterType.Trigger;

            EditorGUI.BeginChangeCheck();
            type = (AnimatorControllerParameterType)EditorGUI.EnumPopup(new Rect(rect) { width = 64, yMin = rect.yMin+1 }, type);
            if (EditorGUI.EndChangeCheck())
            {
                var animatorController = new ElementWrap(__instance).animatorController;
                using var so = new SerializedObject(animatorController);
                using var m_AnimatorParameters = so.FindProperty("m_AnimatorParameters");
                using var element = m_AnimatorParameters.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("m_Type").intValue = (int)type;
                so.ApplyModifiedProperties();
            }
            rect.xMin += 72;
        }
    }

    [HarmonyPatch]
    internal class FloatFieldPatch
    {
        // Float値が整数の場合に.0を追加表示
        [HarmonyPatch(typeof(EditorGUI), "FloatField", new[] { typeof(Rect), typeof(float) }), HarmonyPrefix]
        private static void PrefixFloatField(float value)
        {
            if (!EditorToolboxSettings.instance.extendAnimatorControllerParameterGUI) return;
            if (!CallerUtils.CallerIs(ElementPatch.T_FloatElement, 2)) return;
            EditorGUIWrap.kFloatFieldFormatString = (value == (int)value) ? "#####0.0#####" : "g7";
        }

        [HarmonyPatch(typeof(EditorGUI), "FloatField", new[] { typeof(Rect), typeof(float) }), HarmonyPostfix]
        private static void PostfixFloatField()
        {
            if (!EditorToolboxSettings.instance.extendAnimatorControllerParameterGUI) return;
            EditorGUIWrap.kFloatFieldFormatString = "g7";
        }
    }

    internal class ParameterReferenceWIndow : EditorWindow
    {
        public AnimatorController controller;
        public string parameterName;
        public Object[] references;
        public static void Show(AnimatorControllerParameter parameter, AnimatorController controller)
        {
            var window = GetWindow<ParameterReferenceWIndow>();
            window.controller = controller;
            window.parameterName = parameter.name;
            window.titleContent = new("Parameter Reference");
            using var so = new SerializedObject(controller);
            using var iter = so.GetIterator();
            var references = new HashSet<Object>();
            window.FindReferences(new(), references, controller);
            window.references = references.ToArray();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField($"Parameter: {parameterName}");
            foreach (var reference in references)
            {
                if (!reference) continue;
                var content = EditorGUIUtility.ObjectContent(reference, reference.GetType());
                if (reference is AnimatorTransitionBase transition) content.text = transition.GetDisplayName(null);
                if (GUILayout.Button(content, GUILayout.MaxHeight(20f)))
                {
                    Selection.activeObject = reference;
                    int i = -1;
                    foreach (var layer in controller.layers)
                    {
                        i++;
                        if (layer == null || !layer.stateMachine) continue;
                        var list = new List<Object>();
                        bool GetReference(Object obj)
                        {
                            if (obj == reference) { return true; }
                            if (obj is AnimatorStateMachine machine)
                            {
                                foreach (var s in machine.states) if (GetReference(s.state)) { list.Add(obj); return true; }
                                foreach (var s in machine.stateMachines) if (GetReference(s.stateMachine)) { list.Add(obj); return true; }
                                foreach (var t in machine.entryTransitions) if (GetReference(t)) { list.Add(obj); return true; }
                                foreach (var t in machine.anyStateTransitions) if (GetReference(t)) { list.Add(obj); return true; }
                            }
                            else if (obj is AnimatorState state)
                            {
                                foreach (var t in state.transitions) if (GetReference(t)) { list.Add(obj); return true; }
                                if (GetReference(state.motion)) { list.Add(obj); return true; }
                            }
                            return false;
                        }
                        if (!GetReference(layer.stateMachine)) continue;

                        var window = new AnimatorControllerToolWrap(GetWindow(AnimatorControllerToolWrap.type));
                        if (window.animatorController != controller) window.animatorController = controller;
                        if (window.selectedLayerIndex != i) window.selectedLayerIndex = i;
                        list.Reverse();
                        window.SetBreadCrumbs(list.Where(o => o is AnimatorStateMachine).ToArray());
                    }
                }
            }
        }

        private void FindReferences(HashSet<Object> scaned, HashSet<Object> references, Object obj)
        {
            if (!obj || !scaned.Add(obj)) return;
            if (Common.SkipScan(obj)) return;

            if (obj is AnimatorTransitionBase transition)
            {
                foreach (var condition in transition.conditions)
                {
                    if (condition.parameter == parameterName)
                    {
                        references.Add(obj);
                        break;
                    }
                }
            }
            else if (obj is AnimatorState state)
            {
                if (state.speedParameterActive && state.speedParameter == parameterName) references.Add(obj);
                if (state.timeParameterActive && state.timeParameter == parameterName) references.Add(obj);
                if (state.mirrorParameterActive && state.mirrorParameter == parameterName) references.Add(obj);
                if (state.cycleOffsetParameterActive && state.cycleOffsetParameter == parameterName) references.Add(obj);
            }

            using var so = new SerializedObject(obj);
            using var iter = so.GetIterator();
            var enterChildren = true;
            while (iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if (iter.propertyType == SerializedPropertyType.ObjectReference)
                    FindReferences(scaned, references, iter.objectReferenceValue);
            }
        }
    }

    internal class ParameterControllerViewWrap : WrapBase
    {
        private static Type T_IAnimatorControllerEditor = A_Graphs.GetType("UnityEditor.Graphs.IAnimatorControllerEditor");
        private static readonly (Delegate g, Delegate s) FI_m_Host = GetFieldIns(T_ParameterControllerView, "m_Host", T_IAnimatorControllerEditor);
        private static readonly (Delegate g, Delegate s) FI_m_LastSelectedIndex = GetFieldIns(T_ParameterControllerView, "m_LastSelectedIndex", typeof(int));
        private static readonly PropertyInfo PI_animatorController = T_IAnimatorControllerEditor.GetProperty("animatorController", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private object instance;
        public ParameterControllerViewWrap(object instance) => this.instance = instance;
        public AnimatorController animatorController => PI_animatorController.GetValue(FI_m_Host.g.DynamicInvoke(instance)) as AnimatorController;
        public int m_LastSelectedIndex => (int)FI_m_LastSelectedIndex.g.DynamicInvoke(instance);
    }

    internal class ElementWrap : WrapBase
    {
        public static readonly Type T_Element = T_ParameterControllerView.GetNestedType("Element", BindingFlags.NonPublic);
        private static readonly (Delegate g, Delegate s) FI_m_Parameter = GetFieldIns(T_Element, "m_Parameter", typeof(AnimatorControllerParameter));
        private static readonly (Delegate g, Delegate s) FI_m_Host = GetFieldIns(T_Element, "m_Host", T_ParameterControllerView);
        private object instance;
        public ElementWrap(object instance) => this.instance = instance;
        public AnimatorControllerParameter m_Parameter
        {
            get => (AnimatorControllerParameter)FI_m_Parameter.g.DynamicInvoke(instance);
            set => FI_m_Parameter.s.DynamicInvoke(instance, value);
        }
        public AnimatorController animatorController => new ParameterControllerViewWrap(FI_m_Host.g.DynamicInvoke(instance)).animatorController;
    }
}
#endif
