using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Silksprite.Kogapen
{
    public static class KogapenEditorUtils
    {
        static readonly GUIStyle HeaderStyle = new GUIStyle { fontStyle = FontStyle.Bold, padding = new RectOffset(-6, 0, 10, 0) };

        public static void Header(string label)
        {
            GUILayout.Label(label, HeaderStyle);
        }

        #region copy of UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader()

        public static bool DrawAutoSyncTypeUdonSharpBehaviourHeader(Object target, bool skipLine = false , bool drawScript = true)
        {
            if (UdonSharpGUI.DrawProgramSource(target, drawScript)) return true;

            KogapenRuntimeUtils.ValidateAutoSyncType((UdonSharpBehaviour)target);
            using (new EditorGUI.DisabledScope(true)) UdonSharpGUI.DrawSyncSettings(target);
            UdonSharpGUI.DrawInteractSettings(target);
            UdonSharpGUI.DrawUtilities(target);
            
            UdonSharpGUI.DrawCompileErrorTextArea();

            if (!skipLine)
                UdonSharpGUI.DrawUILine();

            return false;
        }

        public static bool DrawAutoSyncTypeUdonSharpBehaviourHeader(Object[] targets, bool skipLine = false , bool drawScript = true)
        {
            if (UdonSharpGUI.DrawProgramSource(targets, drawScript)) return true;

            foreach (var target in targets) KogapenRuntimeUtils.ValidateAutoSyncType((UdonSharpBehaviour)target);
            using (new EditorGUI.DisabledScope(true)) UdonSharpGUI.DrawSyncSettings(targets);
            UdonSharpGUI.DrawInteractSettings(targets);
            UdonSharpGUI.DrawUtilities(targets);
            
            UdonSharpGUI.DrawCompileErrorTextArea();

            if (!skipLine)
                UdonSharpGUI.DrawUILine();

            return false;
        }

        #endregion
    }
}