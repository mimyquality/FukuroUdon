using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal static class LockReloadAssemblies
    {
        private const string MENU_PATH = Common.MENU_HEAD + "Lock Reload Assemblies";
        internal static bool isLocked = false;
        [MenuItem(MENU_PATH)]
        internal static void ToggleLock()
        {
            isLocked = !isLocked;
            Menu.SetChecked(MENU_PATH, isLocked);
            if(isLocked) EditorApplication.LockReloadAssemblies();
            else EditorApplication.UnlockReloadAssemblies();
        }
    }
}
