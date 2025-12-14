using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class OptionsPopupField<T> : VisualElement
    {
        #region Child API
        protected virtual int GetOptionCount()
        {
            return 0;
        }

        protected virtual IList<T> GetOptions()
        {
            return new List<T>();
        }
        
        protected virtual string GetOptionName(T option)
        {
            return option.ToString();
        }

        
        protected virtual bool IsOptionLocked(T option)
        {
            return false;
        }
        
        protected virtual string GetPopupButtonText()
        {
            return $"{_selectedOptions?.Count ?? 0}/{GetOptionCount()} Selected";
        }

        protected virtual int GetPopupHeight()
        {
            return GetOptionCount() * 26;
        }
        #endregion

        private VisualElement _optionsContainer;
        private Label _label;
        private readonly Button _popupButton;

        private IList<T> _selectedOptions;
        public IList<T> SelectedOptions
        {
            get => _selectedOptions;
            set
            {
                // Ensure we're not sharing references
                _selectedOptions = value.ToList();
                var validOptions = GetOptions();
                foreach (var option in value)
                {
                    if (option == null)
                    {
                        _selectedOptions.Remove(option);
                    }
                    if (string.IsNullOrWhiteSpace(GetOptionName(option)))
                    {
                        _selectedOptions.Remove(option);
                        continue;
                    }
                    if (!validOptions.Contains(option))
                    {
                        _selectedOptions.Remove(option);
                    }
                }
                _popupButton.text = GetPopupButtonText();
                UpdateOptions(ref _optionsContainer);
            }
        }
        
        private IList<T> _originalOptions = new List<T>();
        public IList<T> OriginalOptions
        {
            get => _originalOptions;
            set
            {
                // Ensure we're not sharing references
                _originalOptions = value.ToList();
                var validOptions = GetOptions();
                foreach (var option in value)
                {
                    if (option == null)
                    {
                        _originalOptions.Remove(option);
                    }
                    if (string.IsNullOrWhiteSpace(GetOptionName(option)))
                    {
                        _originalOptions.Remove(option);
                        continue;
                    }
                    if (!validOptions.Contains(option))
                    {
                        _originalOptions.Remove(option);
                    }
                }
                UpdateOptions(ref _optionsContainer);
            }
        }

        private bool _loading;

        public bool Loading
        {
            get => _loading;
            set
            {
                _loading = value;
                _popupButton.text = value ? "Loading..." : GetPopupButtonText();
            }
        }

        public EventHandler<T> OnToggleOption;
        public EventHandler<List<T>> OnPopupClosed;
        private VisualElement _popupBoundsReference;

        public new class UxmlFactory : UxmlFactory<OptionsPopupField<T>, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new() { name = "label" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var optionsField = (OptionsPopupField<T>)ve;
                var label = _label.GetValueFromBag(bag, cc);

                if (string.IsNullOrWhiteSpace(label))
                    optionsField.Q<Label>("label").AddToClassList("d-none");
                else
                    optionsField.Q<Label>("label").text = label;
            }
        }

        public OptionsPopupField()
        {
            Resources.Load<VisualTreeAsset>("OptionsPopupField").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("OptionsPopupFieldStyles"));

            var dropdown = this.Q("dropdown");

            _popupButton = new Button
            {
                text = "0/5 Selected",
                name = "popup-button"
            };

            var spacerElement = new VisualElement
            {
                name = "spacer"
            };

            var arrowElement = new VisualElement
            {
                name = "arrow"
            };
            
            _popupButton.Add(spacerElement);
            _popupButton.Add(arrowElement);
            dropdown.Add(_popupButton);
            
            _popupButton.clicked += () =>
            {
                PopupWindow.Show(_popupButton.worldBound, new OptionsPopupContent(r =>
                {
                    _optionsContainer = new VisualElement();
                    r.Add(_optionsContainer);
                    UpdateOptions(ref _optionsContainer);
                }, new Vector2((_popupBoundsReference ?? _popupButton).worldBound.width, GetPopupHeight()), () =>
                {
                    OnPopupClosed?.Invoke(this, _selectedOptions.ToList());
                }));
            };
        }

        public OptionsPopupField(string label): this()
        {
            this.Q<Label>("label").text = label;
        }
        
        public void SetPopupBoundsReference(VisualElement reference)
        {
            _popupBoundsReference = reference;
        }

        private VisualElement CreateOption(T option)
        {
            var optionElement = new VisualElement();
            optionElement.AddToClassList("row");
            optionElement.AddToClassList("option");
            optionElement.SetEnabled(!IsOptionLocked(option));
            
            var optionToggle = new Toggle
            {
                value = SelectedOptions.Contains(option)
            };
            optionToggle.RegisterValueChangedCallback(_ => OnToggleOption?.Invoke(this, option));
            optionElement.Add(optionToggle);
            
            var optionText = GetOptionName(option);
            var optionLabel = new Label(optionText);
            optionElement.Add(optionLabel);

            return optionElement;
        }

        private void UpdateOptions(ref VisualElement optionContainer)
        {
            if (optionContainer == null)
                return;

            optionContainer.Clear();
            foreach (var option in GetOptions())
                optionContainer.Add(CreateOption(option));
        }

        public void Refresh()
        {
            var allOptions = GetOptions().ToList();
            SelectedOptions = SelectedOptions.Where(o => allOptions.Contains(o)).ToList();
        }
    }
}