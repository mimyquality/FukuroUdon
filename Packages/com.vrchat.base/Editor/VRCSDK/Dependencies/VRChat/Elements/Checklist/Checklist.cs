using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class Checklist: VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Checklist, UxmlTraits> {}
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };
            private readonly UxmlStringAttributeDescription _iconName = new UxmlStringAttributeDescription { name = "icon-name" };
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var checklistField = (Checklist) ve;
                var label = _label.GetValueFromBag(bag, cc);
                if (string.IsNullOrWhiteSpace(label))
                {
                    checklistField.Q("checklist-label").AddToClassList("d-none");
                }
                else
                {
                    checklistField.Q<Label>("checklist-label-text").text = label;
                }

                var iconName = _iconName.GetValueFromBag(bag, cc);
                if (string.IsNullOrWhiteSpace(iconName))
                {
                    checklistField.Q("checklist-label-icon").AddToClassList("d-none");
                }
                else
                {
                    var icon = EditorGUIUtility.IconContent(iconName);
                    var darkIcon = EditorGUIUtility.IconContent($"d_{iconName}");
                    if (EditorGUIUtility.isProSkin && darkIcon != null)
                    {
                        checklistField.Q("checklist-label-icon").style.backgroundImage = (Texture2D) darkIcon.image;  
                    }
                }
            }
        }

        public class ChecklistItem
        {
            public string Value { get; set; }
            public string Label { get; set; }
            public bool Checked { get; set; }
        }

        private VisualElement _itemsContainer;
        private List<ChecklistItem> _items;

        public List<ChecklistItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                RenderItems();
            }
        }

        public Checklist()
        {
            Resources.Load<VisualTreeAsset>("ChecklistLayout").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("ChecklistStyles"));
            _itemsContainer = this.Q("checklist-items");
            _items = new List<ChecklistItem>();
        }
        
        public void MarkItem(string value, bool checkState)
        {
            var itemIndex = _items.FindIndex(i => i.Value == value);
            if (itemIndex < 0) return;

            _items[itemIndex].Checked = checkState;
            RenderItems();
        }

        private void RenderItems()
        {
            _itemsContainer.Clear();
            foreach (var item in _items)
            {
                var container = new VisualElement();
                container.AddToClassList("row");
                container.AddToClassList("align-items-center");
                var icon = new VisualElement();
                icon.AddToClassList("icon");
                icon.AddToClassList(item.Checked ? "check-icon" : "cross-icon");
                container.Add(icon);
                var label = new Label(item.Label)
                {
                    name = item.Value
                };
                container.Add(label);
                _itemsContainer.Add(container);
            }
        }
    }
}