using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "A tool to view and edit any object in json format",
        "For developers. When editing an Object as a SerializedObject, if you need to check the property names or structure, you can display it as JSON to quickly check the contents. You can also edit JSON directly and reflect it in the Object."
    )]
    [DocsMenuLocation(Common.MENU_HEAD + "Json Object Viewer")]
    internal class JsonObjectViewer : EditorWindow
    {
        public Vector2 scrollPos;
        public Object target;
        public string json;

        [MenuItem(Common.MENU_HEAD + "Json Object Viewer")]
        static void Init() => GetWindow(typeof(JsonObjectViewer)).Show();

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            target = EditorGUILayout.ObjectField(target, typeof(Object), true);
            if(EditorGUI.EndChangeCheck()) json = target ? EditorJsonUtility.ToJson(target, true) : "";
            if(!target) return;

            if(L10n.Button("Refresh")) json = EditorJsonUtility.ToJson(target, true);
            if(L10n.Button("Apply Modification")) ObjectUtils.FromJsonOverwrite(json, target);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            json = EditorGUILayout.TextArea(json);
            EditorGUILayout.EndScrollView();
        }
    }
}
