using System;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal class EditorGUIWrap : WrapBase
    {
        private static readonly Type type = typeof(EditorGUI);
        internal static readonly Func<Rect,int,string[],int> AdvancedPopup = GetFunc<Rect,int,string[],int>(type, "AdvancedPopup");
        private static readonly (Delegate g, Delegate s) FI_kFloatFieldFormatString = GetField(type, "kFloatFieldFormatString", typeof(string));
        public static string kFloatFieldFormatString
        {
            get => (string)FI_kFloatFieldFormatString.g.DynamicInvoke(null);
            set => FI_kFloatFieldFormatString.s.DynamicInvoke(value);
        }
    }
}
