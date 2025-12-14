#if LIL_HARMONY
using HarmonyLib;
using System.Reflection;
using static jp.lilxyzw.editortoolbox.AnimatorControllerEditorPatch;

namespace jp.lilxyzw.editortoolbox
{
    [HarmonyPatch]
    internal class GraphGUIPatch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod() => AccessTools.Method(T_GraphGUI, "CopySelectionToPasteboard");

        [HarmonyPostfix]
        private static void Prefix()
        {
            LayerCloner.layer = null;
        }
    }
}
#endif