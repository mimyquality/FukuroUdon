using System;
using System.Reflection;
using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal class MenuServiceWrap : WrapBase
    {
        private static readonly Type type = typeof(Editor).Assembly.GetType("UnityEditor.MenuService");
        internal static readonly Func<MethodInfo,bool> ValidateMethodForMenuCommand = GetFunc<MethodInfo,bool>(type, "ValidateMethodForMenuCommand");
        internal static readonly Func<string,string> SanitizeMenuItemName = GetFunc<string,string>(type, "SanitizeMenuItemName");
    }
}
