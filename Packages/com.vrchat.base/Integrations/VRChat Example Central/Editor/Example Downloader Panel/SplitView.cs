using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.ExampleCentral.Window
{
    
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
    }

    public class PackageButton : VisualElement
    {
        
        public new class UxmlFactory : UxmlFactory<PackageButton, UxmlTraits> { }
        public Label PackageLabel;
        
        private VisualElement selectionElement;
        private Clickable clickable;
        
        /// <summary>
        /// Called when Package Button is clicked.
        /// </summary>
        public event Action clicked
        {
            add { if (clickable != null) clickable.clicked += value; }
            remove { if (clickable != null) clickable.clicked -= value; }
        }

        public PackageButton()
        {
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("PackageTabButton");
            uxml.CloneTree(this);
            
            PackageLabel = this.Query<Label>();
            selectionElement = this.Query<VisualElement>("selection-element");
            
            clickable = new Clickable(OnClicked);
            this.AddManipulator(clickable);
        }

        private void OnClicked(EventBase obj)
        {
            Select(true);
        }

        public void Select(bool value)
        {
            selectionElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
    }
    
}