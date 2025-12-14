using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal class ProjectBrowserWrap : WrapBase
    {
        public static readonly Type type = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        private static readonly Func<object> MI_GetAllProjectBrowsers = GetFunc<object>(type, "GetAllProjectBrowsers");

        private static Type T_List_ProjectBrowser = typeof(List<>).MakeGenericType(type);
        private static readonly PropertyInfo PI_Count = T_List_ProjectBrowser.GetProperty("Count");
        private static readonly PropertyInfo PI_Item = T_List_ProjectBrowser.GetProperty("Item");

        public EditorWindow w;
        public ProjectBrowserWrap(object instance) => w = instance as EditorWindow;

        public static List<ProjectBrowserWrap> GetAllProjectBrowsers()
        {
            var list = MI_GetAllProjectBrowsers();
            var count = (int)PI_Count.GetValue(list, null);

            var res = new List<ProjectBrowserWrap>();
            for(int i = 0; i < count; i++)
            {
                res.Add(new ProjectBrowserWrap(PI_Item.GetValue(list, new object[]{i})));
            }
            return res;
        }
    }
}
