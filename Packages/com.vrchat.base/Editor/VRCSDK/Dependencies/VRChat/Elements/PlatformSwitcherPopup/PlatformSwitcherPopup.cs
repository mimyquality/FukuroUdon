using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class PlatformSwitcherPopup : OptionsPopupField<BuildTarget>
    {
        protected override IList<BuildTarget> GetOptions()
        {
            return VRC_EditorTools.GetBuildTargetOptionsAsEnum();
        }

        protected override bool IsOptionLocked(BuildTarget target)
        {
            var shouldLock = ShouldLockOption?.Invoke(target) ?? false;
            return shouldLock || !VRC_EditorTools.IsBuildTargetSupported(target);
        }

        protected override int GetOptionCount()
        {
            return GetOptions().Count;
        }

        protected override string GetOptionName(BuildTarget target)
        {
            return VRC_EditorTools.GetTargetName(target);
        }

        protected override string GetPopupButtonText()
        {
            if (SelectedOptions.Count == 1)
            {
                return GetOptionName(SelectedOptions[0]);
            }

            if (SelectedOptions.Count == 0)
            {
                return "None Selected";
            }

            if (SelectedOptions.Count == GetOptionCount())
            {
                return "All Platforms";
            }
            
            return $"{SelectedOptions?.Count ?? 0}/{GetOptionCount()} Selected";
        }

        public new class UxmlFactory : UxmlFactory<PlatformSwitcherPopup, UxmlTraits> { }
        public new class UxmlTraits : OptionsPopupField<BuildTarget>.UxmlTraits { }

        public Func<BuildTarget, bool> ShouldLockOption { get; set; }

        public PlatformSwitcherPopup()
        {}
        
        public PlatformSwitcherPopup(string label): base(label)
        {}
    }
}