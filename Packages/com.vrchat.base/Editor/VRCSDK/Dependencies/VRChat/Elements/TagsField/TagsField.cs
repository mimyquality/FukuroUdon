using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class TagsField: VisualElement
    {
        private Label _tagsLabel;
        private TagsFieldButton _tagsButton;
        private Modal _tagsModal;
        private VisualElement _tagsRow;
        private Button _addTagButton;
        private VRCTextField _tagInput;

        private List<string> _tags;
        public IList<string> tags
        {
            get => _tags;
            set
            {
                var copied = new List<string>(value);
                if (TagFilter != null)
                {
                    copied = TagFilter(copied);
                }
                _tags = copied;
                _tagsButton?.SetTagCount(_tags.Count);
                UpdateTags(ref _tagsRow);
                _tagsModal?.SetTitle($"Manage Your Tags ({copied.Count})");
            }
        }

        public EventHandler<string> OnAddTag;
        public EventHandler<string> OnRemoveTag;
        public Func<bool> CanAddTag;
        public Func<string, bool> IsProtectedTag = input => false;
        public Func<string, string> FormatTagDisplay = input => input; 
        public Func<List<string>, List<string>> TagFilter;
        public int TagLimit = 5;

        public new class UxmlFactory : UxmlFactory<TagsField, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var tagsField = (TagsField) ve;
                var label = _label.GetValueFromBag(bag, cc);
                if (string.IsNullOrWhiteSpace(label))
                {
                    tagsField.Q<Label>("tags-label").AddToClassList("d-none");
                }
                else
                {
                    tagsField.Q<Label>("tags-label").text = label;
                }
            }
        }
        
        public TagsField()
        {
            Resources.Load<VisualTreeAsset>("TagsField").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("TagsFieldStyles"));
            
            _tagsLabel = this.Q<Label>("tags-label");
            _tagsButton = this.Q<TagsFieldButton>();
            _tagsRow = this.Q("tags-row");
            _tagsModal = this.Q<Modal>("tags-modal");
            _tagsButton.clicked += _tagsModal.Open;
            _tagsModal.styleSheets.Add(Resources.Load<StyleSheet>("TagsFieldStyles"));

            _tagInput = _tagsModal.Q<VRCTextField>("tag-add-field");
            _tagInput.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode != KeyCode.Return) return;
                AddTag();
            });

            // Comma is a valid input event, so adding a tag and clearing input on KeyDown causes internal errors
            // so we do it on key up and trim the end
            _tagInput.RegisterCallback<KeyUpEvent>(e =>
            {
                if (e.keyCode != KeyCode.Comma) return;
                _tagInput.value = _tagInput.value[..^1];
                AddTag();
            });
            _addTagButton = _tagsModal.Q<Button>("tag-add-button");
            _addTagButton.clicked += AddTag;
            
            tags = new List<string>();
            
            // Anchor the modal to the content-info block
            RegisterCallback<AttachToPanelEvent>(e =>
            {
                _tagsModal.SetAnchor(e.destinationPanel.visualTree.Q("content-info"));
            });
        }

        public TagsField(List<string> tags) : this()
        {
            this.tags = tags;
        }

        /// <summary>
        /// Stops the editing of the tags list, closes the modal and clears the input field
        /// </summary>
        [PublicAPI]
        public void StopEditing()
        {
            _tagInput.value = string.Empty;
            _tagsModal.Close();
        }

        private void AddTag()
        {
            if (tags.Count >= TagLimit)
            {
                return;
            }
            
            if (CanAddTag != null && !CanAddTag())
            {
                return;
            }

            if (_tagInput.IsPlaceholder()) return;
            
            if (_tagInput.value.Length == 0) return;
            
            OnAddTag?.Invoke(this, _tagInput.text);
            _tagInput.value = string.Empty;
        }

        private void UpdateTags(ref VisualElement container)
        {
            container.Clear();
            foreach (var tag in tags)
            {
                var tagElement = new VisualElement();
                tagElement.AddToClassList("tag");
                tagElement.AddToClassList("row");
                tagElement.AddToClassList("mr-2");
                tagElement.AddToClassList("mb-2");
                
                tagElement.Add(new Label(FormatTagDisplay(tag)));
                if (!IsProtectedTag(tag))
                {
                    var removeButton = new Button(() =>
                    {
                        OnRemoveTag?.Invoke(this, tag);
                    });
                    removeButton.AddToClassList("tag-remove-button");
                    tagElement.Add(removeButton);
                }
                container.Add(tagElement);
            }
        }
    }
}