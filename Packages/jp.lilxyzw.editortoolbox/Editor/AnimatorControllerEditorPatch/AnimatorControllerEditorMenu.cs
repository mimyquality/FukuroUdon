using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    [Docs(
        "AnimatorController related extension menus",
        "These are some additional menus related to the AnimatorController.",
        "",
        "## Tools/lilEditorToolbox/Cleanup Animator Controller",
        "",
        "Detect and remove unnecessary sub-assets contained in AnimatorController.",
        "",
        "## Copy and paste menu for StateMachineBehavior",
        "",
        "StateMachineBehaviour can be copied and pasted just like a component.",
        "",
        "## Assets/Create/BlendTree",
        "",
        "Allows you to create BlendTree like any other asset."
    )]
    internal class AnimatorControllerEditorMenu
    {
        [MenuItem(Common.MENU_HEAD + "Cleanup Animator Controller")]
        private static void CleanupAnimatorController()
        {
            SubAssetCleaner.RemoveUnusedSubAssets(Selection.objects.Where(o => o is AnimatorController));
        }

        private static StateMachineBehaviour originalBehaviour;

        [MenuItem("CONTEXT/StateMachineBehaviour/Copy")]
        private static void CopyStateMachineBehaviour(MenuCommand command)
        {
            originalBehaviour = command.context as StateMachineBehaviour;
        }

        [MenuItem("CONTEXT/StateMachineBehaviour/Paste Behaviour As New")]
        private static void PasteStateMachineBehaviour(MenuCommand command)
        {
            var window = EditorWindow.focusedWindow;
            if (window.GetType() != InspectorWindowWrap.type) return;
            var inspector = new InspectorWindowWrap(window);
            var targets = inspector.GetInspectedObjects();
            foreach (var target in targets)
            {
                if (target is AnimatorState state)
                {
                    var copy = Object.Instantiate(originalBehaviour);
                    copy.name = originalBehaviour.name;
                    copy.hideFlags = originalBehaviour.hideFlags;
                    AssetDatabase.AddObjectToAsset(copy, state);
                    using var so = new SerializedObject(state);
                    using var m_StateMachineBehaviours = so.FindProperty("m_StateMachineBehaviours");
                    m_StateMachineBehaviours.arraySize++;
                    using var element = m_StateMachineBehaviours.GetArrayElementAtIndex(m_StateMachineBehaviours.arraySize - 1);
                    element.objectReferenceValue = copy;
                    so.ApplyModifiedProperties();
                }
            }
        }

        [MenuItem("CONTEXT/StateMachineBehaviour/Paste Behaviour Values")]
        private static void PasteStateMachineBehaviourValues(MenuCommand command)
        {
            var behaviour = command.context as StateMachineBehaviour;
            ObjectUtils.CopyProperties(originalBehaviour, behaviour);
        }

        [MenuItem("CONTEXT/StateMachineBehaviour/Paste Behaviour As New", true)]
        private static bool PasteStateMachineBehaviour() => originalBehaviour;

        [MenuItem("CONTEXT/StateMachineBehaviour/Paste Behaviour Values", true)]
        private static bool PasteStateMachineBehaviourValues() => originalBehaviour;

        #if !LIL_MODULAR_AVATAR
        [MenuItem("Assets/Create/BlendTree", false, 411)]
        private static void CreateBlendTree()
        {
            ProjectWindowUtil.CreateAsset(new BlendTree(), "New BlendTree.asset");
        }
        #endif
    }
}
