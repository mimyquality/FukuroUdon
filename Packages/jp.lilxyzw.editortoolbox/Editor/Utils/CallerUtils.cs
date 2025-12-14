using System;

namespace jp.lilxyzw.editortoolbox
{
    public static class CallerUtils
    {
        public static bool CallerIs(Type type, int skipFrames)
        {
            return type == new System.Diagnostics.StackFrame(skipFrames + 1, false).GetMethod().ReflectedType;
        }

        public static Type GetCaller(int skipFrames)
        {
            return new System.Diagnostics.StackFrame(skipFrames + 1, false).GetMethod().ReflectedType;
        }
    }
}
