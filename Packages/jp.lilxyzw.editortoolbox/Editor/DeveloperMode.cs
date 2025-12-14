using UnityEditor;

namespace jp.lilxyzw.editortoolbox
{
    internal static class DeveloperMode
    {
        private const string MENU_PATH = Common.MENU_HEAD + "Unity Developer Mode";
        private const string KEY = "DeveloperMode";
        [MenuItem(MENU_PATH)]
        private static void Toggle()
        {
            var isDev = EditorPrefs.GetBool(KEY, false);
            EditorPrefs.SetBool(KEY, !isDev);
            Menu.SetChecked(MENU_PATH, !isDev);
        }
    }
}
