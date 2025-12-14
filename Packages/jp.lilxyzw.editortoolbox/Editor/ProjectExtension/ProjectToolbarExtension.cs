using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    internal class ProjectToolbarExtension
    {
        internal static void Add(VisualElement visualElement) => root.Add(visualElement);
        private static readonly VisualElement root = new();
        private static readonly HashSet<EditorWindow> modifiedWindows = new();

        private static void AddButton()
        {
            var windows = ProjectBrowserWrap.GetAllProjectBrowsers();
            foreach(var window in windows) AddButton(window.w);
            EditorApplication.update -= AddButton;
        }

        private static void AddButton(EditorWindow window)
        {
            if(modifiedWindows.Add(window))
                window.rootVisualElement.Add(root);
        }

        private static EditorWindow prevWindow;
        private static void AddButtonPerWindow()
        {
            if (!EditorWindow.focusedWindow || prevWindow == EditorWindow.focusedWindow || EditorWindow.focusedWindow.GetType().FullName != "UnityEditor.ProjectBrowser") return;
            prevWindow = EditorWindow.focusedWindow;
            AddButton(prevWindow);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            root.style.marginRight = 470; // 検索バーをクリックできるように
            root.style.marginLeft = 36;
            root.style.height = 20;
            root.style.flexDirection = FlexDirection.Row;
            EditorApplication.update -= AddButton;
            EditorApplication.update += AddButton;
            EditorApplication.update -= AddButtonPerWindow;
            EditorApplication.update += AddButtonPerWindow;
        }
    }
}
