using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    
    public class TagsFieldButton: Button
    {
        public new class UxmlFactory : UxmlFactory<TagsFieldButton, UxmlTraits> {}
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
        }
        
        public void SetTagCount(int count)
        {
            text = $"{count} Tag(s)";
        }
        
        public TagsFieldButton() : base()
        {
            var spacerElement = new VisualElement
            {
                name = "spacer"
            };
            var editElement = new VisualElement
            {
                name = "edit"
            };
            
            Add(spacerElement);
            Add(editElement);
        }
    }
}