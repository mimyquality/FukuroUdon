using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "Extended Inspector",
        "This allows you to treat the Inspector like a browser tab. This is an Inspector that is fixed to one object, so the display content does not change even if you select another object. This window is not opened from the menu bar, but from the + icon on the toolbar at the top of Unity. If you do not have the + icon, you can add it by turning on the `Add Inspector Tab Button` in the Toolbar Extensions of Preferences."
    )]
    internal class TabInspector : EditorWindow
    {
        public List<Object> targets;
        public InspectorWindowWrap inspector;

        [DocsField] private static readonly string[] L_NORMAL = {"Normal", "This is a regular Inspector."};
        [DocsField] private static readonly string[] L_DEBUG = {"Debug", "Inspector for debugging. Normally, you do not need to use it."};
        [DocsField] private static readonly string[] L_DEVELOPER = {"Developer", "This is a special Inspector that can only be seen in developer mode."};

        //[MenuItem("Assets/Open in new Inspector", false, 15)]
        internal static void Init()
        {
            if(!Selection.activeObject) return;
            var window = CreateWindow<TabInspector>(Selection.activeObject.name, new[]{InspectorWindowWrap.type});
            window.titleContent.image = AssetPreview.GetMiniThumbnail(Selection.activeObject);
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            targets ??= Selection.objects.ToList();

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;

            // Target
            var objfield = new ObjectField{value = targets[0]};
            objfield.style.flexGrow = 1;
            header.Add(objfield);

            // Mode Button
            var buttonNormal = new Button{text = L10n.L(L_NORMAL[0]), tooltip = L10n.L(L_NORMAL[1])};
            buttonNormal.clicked += () => inspector.SetNormal();
            header.Add(buttonNormal);
            var buttonDebug = new Button{text = L10n.L(L_DEBUG[0]), tooltip = L10n.L(L_DEBUG[1])};
            buttonDebug.clicked += () => inspector.SetDebug();
            header.Add(buttonDebug);
            var buttonInternal = new Button{text = L10n.L(L_DEVELOPER[0]), tooltip = L10n.L(L_DEVELOPER[1])};
            buttonInternal.clicked += () => inspector.SetDebugInternal();
            header.Add(buttonInternal);

            // Close Button
            var button = new Button{text = "☓", tooltip = L10n.L("Close this tab.")};
            button.clicked += () => Close();
            header.Add(button);

            // Inspector
            inspector = new InspectorWindowWrap(CreateInstance(InspectorWindowWrap.type));
            inspector.SetObjectsLocked(targets);
            inspector.w.rootVisualElement.style.flexGrow = 1;

            rootVisualElement.Add(header);
            rootVisualElement.Add(inspector.w.rootVisualElement);
        }

        private void Update() => inspector.Update();
    }
}
