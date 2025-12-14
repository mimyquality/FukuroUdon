using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Tool to open Unity-related folders",
        "You can open folders used by Unity with one click. You can easily open folders that you may want to check occasionally, such as the location of configuration files or the location of unity packages on the asset store."
    )]
    [DocsMenuLocation(Common.MENU_HEAD + "Folder Opener")]
    internal class FolderOpener : EditorWindow
    {
        public Vector2 scrollPos;

        [MenuItem(Common.MENU_HEAD + "Folder Opener")]
        static void Init() => GetWindow(typeof(FolderOpener)).Show();
        private static GUIContent icon;

        void OnGUI()
        {
            icon ??= EditorGUIUtility.IconContent("Folder Icon");

            EditorGUILayout.LabelField("EditorApplication", EditorStyles.boldLabel);
            PathField(EditorApplication.applicationPath);
            PathField(EditorApplication.applicationContentsPath);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Application", EditorStyles.boldLabel);
            PathField(Application.dataPath);
            PathField(Application.consoleLogPath);
            PathField(Application.persistentDataPath);
            PathField(Application.streamingAssetsPath);
            PathField(Application.temporaryCachePath);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("InternalEditorUtility", EditorStyles.boldLabel);
            PathField(InternalEditorUtility.unityPreferencesFolder);
            PathField(InternalEditorUtility.GetCrashReportFolder());
            PathField(InternalEditorUtility.GetEditorAssemblyPath());
            PathField(InternalEditorUtility.GetEngineAssemblyPath());
            PathField(InternalEditorUtility.GetEngineCoreModuleAssemblyPath());
            PathField(InternalEditorUtility.GetAssetsFolder());
            PathField(InternalEditorUtility.GetEditorFolder());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);
            PathField(InternalEditorUtility.unityPreferencesFolder + "/../../");
        }

        private static void PathField(string path)
        {
            var rect = EditorGUILayout.GetControlRect();
            var rectMax = rect.xMax;
            rect.width = 20;
            if(GUI.Button(rect, icon, EditorStyles.label))
            {
                var open = path;
                if(File.Exists(path)) open = Path.GetDirectoryName(path);
                System.Diagnostics.Process.Start(open);
            }

            rect.xMin = rect.xMax;
            rect.xMax = rectMax;
            EditorGUI.SelectableLabel(rect, path);
        }
    }
}
