#if LIL_HARMONY
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
using ReorderableList = UnityEditorInternal.ReorderableList;
using static jp.lilxyzw.editortoolbox.AnimatorControllerEditorPatch;

namespace jp.lilxyzw.editortoolbox
{
    // Layerのコピペを実装
    [HarmonyPatch]
    internal class LayerCloner
    {
        public static object currentInstance;
        public static object instance;
        public static AnimatorController originalController;
        public static AnimatorControllerLayer layer;
        private static bool isBase = true;
        public static readonly FieldInfo FI_m_LayerList = T_LayerControllerView.GetField("m_LayerList", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo FI_m_Host = T_LayerControllerView.GetField("m_Host", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo PI_animatorController = A_Graphs.GetType("UnityEditor.Graphs.IAnimatorControllerEditor").GetProperty("animatorController", BindingFlags.Instance | BindingFlags.Public);
        public static AnimatorController animatorController => PI_animatorController.GetValue(FI_m_Host.GetValue(instance)) as AnimatorController;

        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_LayerControllerView, "OnDrawLayer");

        [HarmonyPrefix]
        private static void Prefix(object __instance, Rect rect, int index, bool selected, bool focused)
        {
            currentInstance = __instance;
        }

        public static void CopyLayer()
        {
            var m_LayerList = FI_m_LayerList.GetValue(instance) as ReorderableList;
            layer = m_LayerList.list[m_LayerList.index] as AnimatorControllerLayer;
            isBase = m_LayerList.index == 0;
            originalController = animatorController;
        }

        public static void PasteLayer()
        {
            if (layer == null || !originalController) return;
            var controller = animatorController;
            var parameters = new HashSet<string>();
            var newStateMachine = Clone(layer.stateMachine, animatorController, new Dictionary<Object, Object>(), parameters) as AnimatorStateMachine;

            // Parameter
            using var so = new SerializedObject(controller);
            using var m_AnimatorParameters = so.FindProperty("m_AnimatorParameters");
            foreach (var parameter in originalController.parameters.Where(p => parameters.Contains(p.name) && controller.parameters.All(p2 => p2.name != p.name)))
            {
                m_AnimatorParameters.InsertArrayElementAtIndex(m_AnimatorParameters.arraySize);
                using var element = m_AnimatorParameters.GetArrayElementAtIndex(m_AnimatorParameters.arraySize - 1);
                using var m_Name = element.FindPropertyRelative("m_Name"); m_Name.stringValue = parameter.name;
                using var m_Type = element.FindPropertyRelative("m_Type"); m_Type.intValue = (int)parameter.type;
                using var m_DefaultFloat = element.FindPropertyRelative("m_DefaultFloat"); m_DefaultFloat.floatValue = parameter.defaultFloat;
                using var m_DefaultInt = element.FindPropertyRelative("m_DefaultInt"); m_DefaultInt.intValue = parameter.defaultInt;
                using var m_DefaultBool = element.FindPropertyRelative("m_DefaultBool"); m_DefaultBool.boolValue = parameter.defaultBool;
            }

            // Layer
            using var m_AnimatorLayers = so.FindProperty("m_AnimatorLayers");
            {
                m_AnimatorLayers.InsertArrayElementAtIndex(m_AnimatorLayers.arraySize - 1);
                using var layerElement = m_AnimatorLayers.GetArrayElementAtIndex(m_AnimatorLayers.arraySize - 1);
                var layername = layer.name;
                if(controller.layers.Any(l => l.name == layername)) layername += " (Clone)";
                using var m_Name = layerElement.FindPropertyRelative("m_Name"); m_Name.stringValue = layername;
                using var m_StateMachine = layerElement.FindPropertyRelative("m_StateMachine"); m_StateMachine.objectReferenceValue = newStateMachine;
                using var m_Mask = layerElement.FindPropertyRelative("m_Mask"); m_Mask.objectReferenceValue = layer.avatarMask;
                using var m_Motions = layerElement.FindPropertyRelative("m_Motions");
                using var m_Behaviours = layerElement.FindPropertyRelative("m_Behaviours");
                using var m_BlendingMode = layerElement.FindPropertyRelative("m_BlendingMode"); m_BlendingMode.intValue = (int)layer.blendingMode;
                using var m_SyncedLayerIndex = layerElement.FindPropertyRelative("m_SyncedLayerIndex"); m_SyncedLayerIndex.intValue = layer.syncedLayerIndex;
                using var m_DefaultWeight = layerElement.FindPropertyRelative("m_DefaultWeight"); m_DefaultWeight.floatValue = isBase ? 1f : layer.defaultWeight;
                using var m_IKPass = layerElement.FindPropertyRelative("m_IKPass"); m_IKPass.boolValue = layer.iKPass;
                using var m_SyncedLayerAffectsTiming = layerElement.FindPropertyRelative("m_SyncedLayerAffectsTiming"); m_SyncedLayerAffectsTiming.boolValue = layer.syncedLayerAffectsTiming;
            }
            so.ApplyModifiedProperties();
        }

        private static Object Clone(Object obj, Object parent, Dictionary<Object, Object> map, HashSet<string> parameters)
        {
            if (!obj || AssetDatabase.IsMainAsset(obj) || obj is MonoScript) return obj;
            if (map.TryGetValue(obj, out var value)) return value;
            Object newObj;
            if (obj is AnimatorStateMachine)
            {
                newObj = new AnimatorStateMachine();
                ObjectUtils.CopyProperties(obj, newObj);
            }
            else if (obj is AnimatorState state)
            {
                newObj = new AnimatorState();
                ObjectUtils.CopyProperties(obj, newObj);
                if(state.speedParameterActive) parameters.Add(state.speedParameter);
                if(state.timeParameterActive) parameters.Add(state.timeParameter);
                if(state.mirrorParameterActive) parameters.Add(state.mirrorParameter);
                if(state.cycleOffsetParameterActive) parameters.Add(state.cycleOffsetParameter);
            }
            else newObj = Object.Instantiate(obj);
            if (obj is AnimatorTransitionBase transitionBase)
            {
                parameters.UnionWith(transitionBase.conditions.Select(c => c.parameter));
            }
            newObj.name = obj.name;
            newObj.hideFlags = obj.hideFlags;
            map[obj] = newObj;

            AssetDatabase.AddObjectToAsset(newObj, parent);
            using var so = new SerializedObject(newObj);
            using var iter = so.GetIterator();
            bool enterChildren = true;
            while (iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if (iter.propertyType == SerializedPropertyType.ObjectReference) iter.objectReferenceValue = Clone(iter.objectReferenceValue, parent, map, parameters);
            }
            so.ApplyModifiedProperties();
            return newObj;
        }
    }

    [HarmonyPatch]
    internal class LayerClonerShortcut
    {
        public static bool isCopy = false;
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_LayerControllerView, "KeyboardHandling");

        [HarmonyPrefix]
        private static void PrefixKeyboardHandling(object __instance)
        {
            if (!EditorToolboxSettings.instance.addCopyAndPasteLayerMenu) return;
            var m_LayerList = LayerCloner.FI_m_LayerList.GetValue(__instance) as ReorderableList;
            if (!m_LayerList.HasKeyboardControl()) return;

            var e = Event.current;
            if (e.type != EventType.KeyDown || !e.control) return;

            LayerCloner.instance = __instance;
            switch (e.keyCode)
            {
                case KeyCode.C: LayerCloner.CopyLayer(); e.Use(); break;
                case KeyCode.V:
                    if (LayerCloner.layer != null && LayerCloner.originalController)
                    {
                        LayerCloner.PasteLayer();
                        e.Use();
                    }
                    break;
            }
        }
    }
}
#endif
