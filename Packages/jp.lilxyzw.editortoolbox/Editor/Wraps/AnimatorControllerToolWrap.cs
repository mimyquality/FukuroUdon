using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{
    internal class AnimatorControllerToolWrap : WrapBase
    {
        private static readonly Assembly A_Graphs = Assembly.Load("UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        public static readonly Type type = A_Graphs.GetType("UnityEditor.Graphs.AnimatorControllerTool");
        private static readonly (Delegate g, Delegate s) FI_animatorController = GetPropertyIns(type, "animatorController", typeof(AnimatorController));
        private static readonly (Delegate g, Delegate s) FI_selectedLayerIndex = GetPropertyIns(type, "selectedLayerIndex", typeof(int));
        private static readonly (Delegate g, Delegate s) FI_m_BreadCrumbs = GetFieldIns(type, "m_BreadCrumbs", typeof(List<>).MakeGenericType(BreadCrumbElementWrap.type));
        private static readonly MethodInfo MI_UpdateStateMachineView = type.GetMethod("UpdateStateMachineView", BindingFlags.Instance | BindingFlags.NonPublic);

        public EditorWindow w;
        public AnimatorControllerToolWrap(object instance) => w = instance as EditorWindow;
        public AnimatorController animatorController
        {
            get => (AnimatorController)FI_animatorController.g.DynamicInvoke(w);
            set => FI_animatorController.s.DynamicInvoke(w, value);
        }
        public int selectedLayerIndex
        {
            get => (int)FI_selectedLayerIndex.g.DynamicInvoke(w);
            set => FI_selectedLayerIndex.s.DynamicInvoke(w, value);
        }
        public object m_BreadCrumbs
        {
            get => FI_m_BreadCrumbs.g.DynamicInvoke(w);
            set => FI_m_BreadCrumbs.s.DynamicInvoke(w, value);
        }
        public void SetBreadCrumbs(params Object[] objs)
        {
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(BreadCrumbElementWrap.type));
            var add = list.GetType().GetMethod("Add");
            foreach (var o in objs) add.Invoke(list, new object[] { new BreadCrumbElementWrap(o).instance });
            m_BreadCrumbs = list;
            MI_UpdateStateMachineView.Invoke(w, null);
        }
    }

    internal class BreadCrumbElementWrap : WrapBase
    {
        private static readonly Assembly A_Graphs = Assembly.Load("UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        public static readonly Type type = A_Graphs.GetType("UnityEditor.Graphs.AnimatorControllerTool").GetNestedType("BreadCrumbElement", BindingFlags.NonPublic);
        private static readonly (Delegate g, Delegate s) FI_m_Target = GetFieldIns(type, "m_Target", typeof(Object));
        private static readonly ConstructorInfo CI = type.GetConstructor(new[] { typeof(Object) });
        public object instance;
        public BreadCrumbElementWrap(object instance) => this.instance = instance;
        public BreadCrumbElementWrap(Object instance) => this.instance = CI.Invoke(new object[] { instance });

        public Object m_Target
        {
            get => (Object)FI_m_Target.g.DynamicInvoke(instance);
            set => FI_m_Target.s.DynamicInvoke(instance, value);
        }
    }
}
