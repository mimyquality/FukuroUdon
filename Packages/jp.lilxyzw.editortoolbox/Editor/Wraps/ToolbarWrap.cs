using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    internal class ToolbarWrap : WrapBase
    {
        private static readonly Type type = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static readonly Func<object> FI_get = GetField(type, "get", type).g;
        private static readonly Delegate FI_m_Root = GetFieldIns(type, "m_Root", typeof(VisualElement)).g;

        public object instance;
        public ToolbarWrap(object instance) => this.instance = instance;

        public static ToolbarWrap get => new(FI_get());
        public VisualElement m_Root => FI_m_Root.DynamicInvoke(instance) as VisualElement;
    }
}
