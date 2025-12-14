using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{
    internal class InspectorWindowWrap : WrapBase
    {
        public static readonly Type type = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        private static readonly Type T_PropertyEditor = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor");
        private static readonly MethodInfo MI_SetObjectsLocked = type.GetMethod("SetObjectsLocked", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo MI_SetNormal = T_PropertyEditor.GetMethod("SetNormal", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo MI_SetDebug = T_PropertyEditor.GetMethod("SetDebug", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo MI_SetDebugInternal = T_PropertyEditor.GetMethod("SetDebugInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo MI_Update = T_PropertyEditor.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo MI_GetInspectedObjects = type.GetMethod("GetInspectedObjects", BindingFlags.NonPublic | BindingFlags.Instance);

        public EditorWindow w;
        public InspectorWindowWrap(object instance) => w = instance as EditorWindow;
        public void SetNormal() => MI_SetNormal.Invoke(w, null);
        public void SetDebug() => MI_SetDebug.Invoke(w, null);
        public void SetDebugInternal() => MI_SetDebugInternal.Invoke(w, null);
        public void SetObjectsLocked(List<Object> objs) => MI_SetObjectsLocked.Invoke(w, new object[]{objs});
        public void Update() => MI_Update.Invoke(w, null);
        public Object[] GetInspectedObjects() => MI_GetInspectedObjects.Invoke(w, null) as Object[];
    }
}
