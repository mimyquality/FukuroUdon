using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class VRCTextField: TextField
    {
        public new class UxmlFactory : UxmlFactory<VRCTextField, UxmlTraits> {}

        public new class UxmlTraits : TextField.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _placeholder = new() { name = "placeholder" };
            private readonly UxmlBoolAttributeDescription _required = new() { name = "required" };
            private readonly UxmlBoolAttributeDescription _vertical = new() { name="vertical" };
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var textField = (VRCTextField) ve;
                textField._placeholder = _placeholder.GetValueFromBag(bag, cc);
                textField._required = _required.GetValueFromBag(bag, cc);
                var vertical = _vertical.GetValueFromBag(bag, cc);
                if (textField._required)
                {
                    textField.label += "*";
                }
                if (vertical)
                {
                    textField.AddToClassList("col");
                }
            }
        }

        private string _placeholder;
        private static readonly string PlaceholderClass = ussClassName + "__placeholder";
        private bool _required;
        private bool _loading;
        private bool _vertical;

        public bool Loading
        {
            get => _loading;
            set
            {
                _loading = value;
                SetEnabled(!_loading);
                if (_loading)
                {
                    text = "Loading...";
                }
                else
                {
                    if (text == "Loading...")
                    {
                        text = "";
                    }
                    FocusOut();
                }
                EnableInClassList(ussClassName + "__loading", _loading);
            }
        }

        public VRCTextField(): base()
        {
            RegisterCallback<FocusOutEvent>(evt => FocusOut());
            RegisterCallback<FocusInEvent>(evt => FocusIn());
            this.RegisterValueChangedCallback(ValueChanged);
        }

        public void Reset()
        {
            if (string.IsNullOrEmpty(text))
            {
                FocusOut();
                return;
            };
            RemoveFromClassList(PlaceholderClass);
        }

        private void ValueChanged(ChangeEvent<string> evt)
        {
            if (IsPlaceholder() && !string.IsNullOrEmpty(evt.newValue))
            {
                RemoveFromClassList(PlaceholderClass);
            }
            if (!_required) return;
            this.Q<TextInputBase>().EnableInClassList("border-red", string.IsNullOrWhiteSpace(evt.newValue));
        }

        private void FocusOut()
        {
            if (string.IsNullOrWhiteSpace(_placeholder)) return;
            if (!string.IsNullOrEmpty(text)) return;
            SetValueWithoutNotify(_placeholder);
            AddToClassList(PlaceholderClass);
        }

        private void FocusIn()
        {
            if (string.IsNullOrWhiteSpace(_placeholder)) return;
            if (!this.ClassListContains(PlaceholderClass)) return;
            this.value = string.Empty;
            this.RemoveFromClassList(PlaceholderClass);
        }
        
        public bool IsPlaceholder()
        {
            return ClassListContains(PlaceholderClass);
        }
    }
}