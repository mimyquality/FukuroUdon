using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class StepFoldout: Foldout
    {
        public new class UxmlFactory : UxmlFactory<StepFoldout, UxmlTraits> {}

        public new class UxmlTraits : Foldout.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _stepName = new() { name = "stepName" };
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var stepFoldout = (StepFoldout) ve;
                stepFoldout.InsertStepName(_stepName.GetValueFromBag(bag, cc));
            }
        }
        
        private string _stepName;
        private Label _headerLabel;

        private void InsertStepName(string stepName)
        {
            if (string.IsNullOrWhiteSpace(stepName)) return;
            _stepName = stepName;


            // We insert the step right before the main label
            _headerLabel = this.Q<Toggle>().Q<Label>();
            var headerIndex = _headerLabel.parent.hierarchy.IndexOf(_headerLabel);
            var stepLabel = new Label(_stepName);
            stepLabel.AddToClassList("step-label");
            _headerLabel.parent.Insert(headerIndex, stepLabel);
        }

        public StepFoldout()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("StepFoldoutStyles"));
        }
        
        public void SetTitle(string title)
        {
            _headerLabel.text = title;
        }
    }
}