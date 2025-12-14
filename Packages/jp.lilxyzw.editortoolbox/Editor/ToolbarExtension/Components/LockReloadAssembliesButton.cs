using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    [Tooltip("Locks the assembly to reduce the wait time for script compilation. This is useful if you frequently rewrite scripts.")]
    internal class LockReloadAssembliesButton : IToolbarExtensionComponent
    {
        public int Priority => 0;
        public bool InLeftSide => true;

        public VisualElement GetRootElement()
        {
            var root = new ToolbarToggle (){text = L10n.L("Assemblies Unlocked")};
            root.RegisterValueChangedCallback(e => {
                LockReloadAssemblies.ToggleLock();
                root.text = LockReloadAssemblies.isLocked ? L10n.L("Assemblies Locked") : L10n.L("Assemblies Unlocked");
            });
            return root;
        }
    }
}
