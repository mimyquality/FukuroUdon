#if LIL_HARMONY
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [HarmonyPatch]
    internal class AnimatorControllerEditorPatch
    {
        public static readonly Assembly A_Graphs = Assembly.Load("UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        public static readonly Type T_LayerControllerView = A_Graphs.GetType("UnityEditor.Graphs.LayerControllerView");
        public static readonly Type T_GraphGUI = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.GraphGUI");
        public static readonly Type T_StateNode = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.StateNode");
        public static readonly Type T_StateMachineNode = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.StateMachineNode");
        public static readonly Type T_AnyStateNode = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.AnyStateNode");
        public static readonly Type T_EntryNode = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.EntryNode");
        public static readonly Type T_AnimatorTransitionInspectorBase = A_Graphs.GetType("UnityEditor.Graphs.AnimationStateMachine.AnimatorTransitionInspectorBase");
        public static readonly Type T_ParameterControllerView = A_Graphs.GetType("UnityEditor.Graphs.ParameterControllerView");
        private static readonly FieldInfo FI_stateMachineGraphGUI = A_Graphs.GetType("UnityEditor.Graphs.AnimatorControllerTool").GetField("stateMachineGraphGUI", BindingFlags.Instance | BindingFlags.Public);
        private static readonly PropertyInfo PI_activeStateMachine = T_GraphGUI.GetProperty("activeStateMachine", BindingFlags.Instance | BindingFlags.Public);

        [InitializeOnLoadMethod]
        private static void DoPatching()
        {
            new Harmony("jp.lilxyzw.editortoolbox").PatchAll();
        }

        // LayerのWeight
        [HarmonyPatch(typeof(AnimatorController), "AddLayer", new[] { typeof(AnimatorControllerLayer) }), HarmonyPrefix]
        private static void PostfixAddLayer(ref AnimatorControllerLayer layer)
        {
            if (!EditorToolboxSettings.instance.defaultLayerWeight1 || !CallerUtils.CallerIs(AnimatorControllerToolWrap.type, 3)) return;
            layer.defaultWeight = 1.0f;
        }

        // Write Defaults
        [HarmonyPatch(typeof(AnimatorStateMachine), "AddState", new[] { typeof(AnimatorState), typeof(Vector3) }), HarmonyPostfix]
        private static void PostfixAddState(ref AnimatorState state)
        {
            if (!EditorToolboxSettings.instance.defaultWriteDefaultsOff || !CallerUtils.CallerIs(T_GraphGUI, 3)) return;
            state.writeDefaultValues = false;
        }

        // Transitionの初期値
        [HarmonyPatch(typeof(AnimatorState), "CreateTransition"), HarmonyPostfix]
        private static void PostfixCreateTransition(ref AnimatorStateTransition __result)
        {
            if (!CallerUtils.CallerIs(T_StateNode, 3)) return;
            if(EditorToolboxSettings.instance.defaultHasExitTimeOff) __result.hasExitTime = false;
            if(EditorToolboxSettings.instance.defaultExitTime1) __result.exitTime = 1;
            if(EditorToolboxSettings.instance.defaultDuration0) __result.duration = 0;
        }

        [HarmonyPatch(typeof(AnimatorStateMachine), "AddAnyStateTransition", new[] { typeof(AnimatorState) }), HarmonyPostfix]
        private static void PostfixAddAnyStateTransition(ref AnimatorStateTransition __result)
        {
            if (!CallerUtils.CallerIs(T_AnyStateNode, 2)) return;
            if(EditorToolboxSettings.instance.defaultHasExitTimeOff) __result.hasExitTime = false;
            if(EditorToolboxSettings.instance.defaultExitTime1) __result.exitTime = 1;
            if(EditorToolboxSettings.instance.defaultDuration0) __result.duration = 0;
            if(EditorToolboxSettings.instance.defaultCanTransitionSelfOff) __result.canTransitionToSelf = false;
        }

        public static AnimatorTransitionBase originalTransition;
        [HarmonyPatch(typeof(GenericMenu), "ShowAsContext"), HarmonyPrefix]
        private static void PrefixShowAsContext(object __instance)
        {
            var menu = __instance as GenericMenu;
            var caller2 = CallerUtils.GetCaller(2);
            // Layerコピペ
            if (EditorToolboxSettings.instance.addCopyAndPasteLayerMenu && caller2 == T_LayerControllerView)
            {
                menu.AddItem(new GUIContent("Copy"), false, new GenericMenu.MenuFunction(LayerCloner.CopyLayer));
                if (LayerCloner.layer != null) menu.AddItem(new GUIContent("Paste As New Layer"), false, new GenericMenu.MenuFunction(LayerCloner.PasteLayer));
                else menu.AddDisabledItem(new GUIContent("Paste As New Layer"));
                LayerCloner.instance = LayerCloner.currentInstance;
            }

            else if (caller2 == T_GraphGUI)
            {
                // Transitionコピペ
                if (EditorToolboxSettings.instance.addCopyTransitionSettingsMenu && Selection.objects.Any(o => o is AnimatorTransitionBase))
                {
                    menu.AddItem(new GUIContent("Copy Transition Settings"), false, new GenericMenu.MenuFunction(() => originalTransition = Selection.objects.First(o => o is AnimatorTransitionBase) as AnimatorTransitionBase));
                    if (originalTransition) menu.AddItem(new GUIContent("Paste Transition Settings"), false, new GenericMenu.MenuFunction(PasteTransitionSettings));
                    else menu.AddDisabledItem(new GUIContent("Paste Transition Settings"));
                }

                // Exitに入るTransition一括選択
                if (EditorToolboxSettings.instance.addSelectInOutTransitionsMenu && Selection.activeObject && Selection.activeObject.GetType().FullName == "UnityEditor.Graphs.AnimationStateMachine.ExitNode")
                {
                    var window = EditorWindow.focusedWindow;
                    if (window.GetType() != AnimatorControllerToolWrap.type) return;
                    var machine = PI_activeStateMachine.GetValue(FI_stateMachineGraphGUI.GetValue(window)) as AnimatorStateMachine;
                    menu.AddItem(new GUIContent("Select In Transitions"), false, new GenericMenu.MenuFunction(() => Selection.objects = GetTransitions(machine).Where(t => t.isExit).ToArray()));
                }
            }

            // 通常のStateから出入りするTransition一括選択
            else if (EditorToolboxSettings.instance.addSelectInOutTransitionsMenu && caller2 == T_StateNode)
            {
                if (Selection.activeObject is AnimatorState state)
                {
                    var window = EditorWindow.focusedWindow;
                    if (window.GetType() != AnimatorControllerToolWrap.type) return;
                    var machine = PI_activeStateMachine.GetValue(FI_stateMachineGraphGUI.GetValue(window)) as AnimatorStateMachine;
                    menu.AddItem(new GUIContent("Select Out Transitions"), false, new GenericMenu.MenuFunction(() => Selection.objects = state.transitions));
                    menu.AddItem(new GUIContent("Select In Transitions"), false, new GenericMenu.MenuFunction(() => Selection.objects = GetTransitions(machine).Where(t => t.destinationState == state).ToArray()));
                }
            }

            // AnyStateから出るTransition一括選択
            else if (EditorToolboxSettings.instance.addSelectInOutTransitionsMenu && caller2 == T_AnyStateNode)
            {
                var window = EditorWindow.focusedWindow;
                if (window.GetType() != AnimatorControllerToolWrap.type) return;
                var machine = PI_activeStateMachine.GetValue(FI_stateMachineGraphGUI.GetValue(window)) as AnimatorStateMachine;
                menu.AddItem(new GUIContent("Select Out Transitions"), false, new GenericMenu.MenuFunction(() => Selection.objects = machine.anyStateTransitions));
            }

            // Entryから出るTransition一括選択
            else if (EditorToolboxSettings.instance.addSelectInOutTransitionsMenu && caller2 == T_EntryNode)
            {
                var window = EditorWindow.focusedWindow;
                if (window.GetType() != AnimatorControllerToolWrap.type) return;
                var machine = PI_activeStateMachine.GetValue(FI_stateMachineGraphGUI.GetValue(window)) as AnimatorStateMachine;
                menu.AddItem(new GUIContent("Select Out Transitions"), false, new GenericMenu.MenuFunction(() => Selection.objects = machine.entryTransitions));
            }

            // Entryから出るTransition一括選択
            else if (EditorToolboxSettings.instance.addSelectInOutTransitionsMenu && caller2 == T_StateMachineNode)
            {
                if (Selection.activeObject is AnimatorStateMachine machine)
                {
                    var window = EditorWindow.focusedWindow;
                    if (window.GetType() != AnimatorControllerToolWrap.type) return;
                    var machine2 = PI_activeStateMachine.GetValue(FI_stateMachineGraphGUI.GetValue(window)) as AnimatorStateMachine;
                    var states = machine.states.Select(s => s.state);
                    menu.AddItem(new GUIContent("Select Out Transitions"), false, new GenericMenu.MenuFunction(() => Selection.objects = machine2.GetStateMachineTransitions(machine)));
                    menu.AddItem(new GUIContent("Select In Transitions"), false, new GenericMenu.MenuFunction(() => Selection.objects = GetTransitions(machine2).Where(t => t.destinationStateMachine == machine || states.Contains(t.destinationState)).ToArray()));
                }
            }

            // Parameterの参照元をチェックするウィンドウを表示
            else if (EditorToolboxSettings.instance.addParameterReferencesMenu && caller2 == T_ParameterControllerView)
            {
                menu.AddItem(new GUIContent("Show References"), false, new GenericMenu.MenuFunction(ParameterControllerViewPatch.ShowReferences));
            }
        }

        private static HashSet<AnimatorTransitionBase> GetTransitions(AnimatorStateMachine machine)
        {
            var transitions = new HashSet<AnimatorTransitionBase>();
            transitions.UnionWith(machine.entryTransitions);
            transitions.UnionWith(machine.anyStateTransitions);
            transitions.UnionWith(machine.states.SelectMany(s => s.state.transitions));
            transitions.UnionWith(machine.stateMachines.SelectMany(s => machine.GetStateMachineTransitions(s.stateMachine)));
            return transitions;
        }

        private static void PasteTransitionSettings()
        {
            foreach (var transition in Selection.objects.Where(o => o is AnimatorTransitionBase))
            {
                PasteTransitionSettings(transition as AnimatorTransitionBase);
            }
        }

        public static void PasteTransitionSettings(AnimatorTransitionBase transition)
        {
            ObjectUtils.CopyProperties(originalTransition, transition, "m_DstStateMachine", "m_DstState");
        }
    }
}
#endif
