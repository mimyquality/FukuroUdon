using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class BuilderProgress: VisualElement
    {
        public new class UxmlFactory : UxmlFactory<BuilderProgress, UxmlTraits> {}
        
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }
        
        public struct ProgressBarStateData
        {
            public bool Visible { get; set; }
            public string Text { get; set; }
            public float Progress { get; set; }
        }
        
        public ProgressBarStateData State => _state;
        public EventHandler OnCancel;
        
        private ProgressBarStateData _state;
        private readonly VisualElement _progressBlock;
        private readonly VisualElement _progressBar;
        private readonly Label _progressText;
        private readonly Button _cancelButton;
        private VisualElement _visualRoot;
        
        
        public BuilderProgress()
        {
            Resources.Load<VisualTreeAsset>("BuilderProgress").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("BuilderProgressStyles"));
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                _visualRoot = evt.destinationPanel.visualTree;
            });
            _progressBlock = this;
            _progressBlock.AddToClassList("d-none");
            _progressBar = this.Q("progress-bar");
            _progressText = this.Q<Label>("progress-text");
            _cancelButton = this.Q<Button>("cancel-button");
            _cancelButton.clicked += () => OnCancel?.Invoke(this, EventArgs.Empty);
        }


        public void SetProgress(ProgressBarStateData state)
        {
            if (_state.Visible != state.Visible)
            {
                _progressBlock.EnableInClassList("d-none", !state.Visible);
                if (state.Visible)
                {
                    _progressBar.style.width = 0;
                }
            }

            _progressText.text = state.Text;
            if (Mathf.Abs(_state.Progress - state.Progress) > float.Epsilon)
            {
                // execute on next frame to allow for layout to calculate
                _visualRoot.schedule.Execute(() =>
                {
                    _progressBar.experimental.animation.Start(
                        new StyleValues {width = _progressBar.layout.width, height = 28f}, 
                        new StyleValues {width = _progressBlock.layout.width * state.Progress, height = 28f}, 
                        500
                    );
                }).StartingIn(50);
            }
            _state = state;
        }

        public void ClearProgress()
        {
            _progressBar.style.width = 0;
        }

        public void HideProgress()
        {
            SetProgress(new ProgressBarStateData { Visible = false });
        }

        public void SetCancelButtonVisibility(bool isVisible)
        {
            _cancelButton.EnableInClassList("d-none", !isVisible);
        }
    }
}