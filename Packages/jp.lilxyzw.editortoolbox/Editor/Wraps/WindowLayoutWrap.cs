using System;
using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal class WindowLayoutWrap : WrapBase
    {
        private static readonly Type type = typeof(Editor).Assembly.GetType("UnityEditor.WindowLayout");
        internal static Action UpdateWindowLayoutMenu = GetAction(type, "UpdateWindowLayoutMenu");
    }
}
