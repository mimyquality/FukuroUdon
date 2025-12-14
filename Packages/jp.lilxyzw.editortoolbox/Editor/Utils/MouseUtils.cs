using System.Runtime.InteropServices;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal static class MouseUtils
    {
        #if UNITY_EDITOR_WIN
        [DllImport ("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref POINT pt);

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool show);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        internal static Vector2 SetPos(Vector2 pos)
        {
            SetCursorPos((int)pos.x, (int)pos.y);
            return pos;
        }

        internal static Vector2 GetPos()
        {
            var pt = new POINT();
            GetCursorPos(ref pt);
            return new(pt.x, pt.y);
        }

        internal static void Hide(bool hide) => ShowCursor(!hide);
        #else
        internal static Vector2 SetPos(Vector2 pos) => Event.current.mousePosition;
        internal static Vector2 GetPos() => Event.current.mousePosition;
        internal static void Hide(bool hide){}
        #endif
    }
}
