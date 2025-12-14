#if LIL_HARMONY
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using static jp.lilxyzw.editortoolbox.AnimatorControllerEditorPatch;

namespace jp.lilxyzw.editortoolbox
{
    [HarmonyPatch]
    internal class StateNodePatch
    {
        private static MethodInfo T_MakeTransitionCallback = T_StateNode.GetMethod("MakeTransitionCallback", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_StateNode, "NodeUI");

        [HarmonyPrefix]
        private static void Prefix(object __instance) => DoubleClickDetector.Invoke(T_MakeTransitionCallback, __instance, EditorToolboxSettings.instance.makeTransitionWithDoubleClick);
    }

    [HarmonyPatch]
    internal class AnyStateNodePatch
    {
        private static MethodInfo T_MakeTransitionCallback = T_AnyStateNode.GetMethod("MakeTransitionCallback", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_AnyStateNode, "NodeUI");

        [HarmonyPrefix]
        private static void Prefix(object __instance) => DoubleClickDetector.Invoke(T_MakeTransitionCallback, __instance, EditorToolboxSettings.instance.makeTransitionWithDoubleClick);
    }

    [HarmonyPatch]
    internal class EntryNodePatch
    {
        private static MethodInfo T_MakeTransitionCallback = T_EntryNode.GetMethod("MakeTransitionCallback", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_EntryNode, "NodeUI");

        [HarmonyPrefix]
        private static void Prefix(object __instance) => DoubleClickDetector.Invoke(T_MakeTransitionCallback, __instance, EditorToolboxSettings.instance.makeTransitionWithDoubleClick);
    }

    internal class DoubleClickDetector
    {
        private static DateTime lastClickTime = DateTime.MinValue;
        private static bool IsDoubleClicking()
        {
            var e = Event.current;
            if (e.type != EventType.MouseDown || e.button != 0) return false;
            var isDoubleClicking = lastClickTime.AddMilliseconds(250) > DateTime.Now;
            lastClickTime = DateTime.Now;
            return isDoubleClicking;
        }

        public static void Invoke(MethodInfo methodInfo, object instance, bool needToDo)
        {
            if (needToDo && IsDoubleClicking()) methodInfo.Invoke(instance, null);
        }
    }
}
#endif
