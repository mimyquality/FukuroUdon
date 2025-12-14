using System;
using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal class MenuWrap : WrapBase
    {
        private static readonly Type type = typeof(Menu);
        internal static readonly Action<string, string, bool, int, Action, Func<bool>> AddMenuItem = GetAction<string, string, bool, int, Action, Func<bool>>(type, "AddMenuItem");
        internal static readonly Action<string> RemoveMenuItem = GetAction<string>(type, "RemoveMenuItem");
        internal static readonly Action RebuildAllMenus = GetAction(type, "RebuildAllMenus");
    }
}
