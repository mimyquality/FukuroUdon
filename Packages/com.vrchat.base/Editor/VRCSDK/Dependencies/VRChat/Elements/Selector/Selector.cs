using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public abstract class Selector<T> : VisualElement
    {
        public bool PopupEnabled
        {
            get => _popupField.enabledSelf;
            set => _popupField.SetEnabled(value);
        }

        private readonly string _popupFieldName;
        private readonly bool _pingWhenOptionsSet;
        private PopupField<T> _popupField;
        private VisualElement _popupInput;
        private EventCallback<ChangeEvent<T>> _changeCallback;

        protected Selector(List<T> options, string popupFieldName, string styleSheetPath, string labelText, string labelName, bool pingWhenOptionsSet)
        {
            _popupFieldName = popupFieldName;
            _pingWhenOptionsSet = pingWhenOptionsSet;

            styleSheets.Add(Resources.Load<StyleSheet>(styleSheetPath));
            var label = new Label(labelText)
            {
                name = labelName
            };
            Add(label);
            SetOptions(options, 0);
        }

        private void CreateField(List<T> options, int selectedIndex)
        {
            if (Contains(_popupField))
            {
                Remove(_popupField);
            }

            if (options == null || options.Count == 0)
            {
                return;
            }
            
            _popupField = new PopupField<T>(
                null,
                options,
                selectedIndex,
                prop => FormatElementName(options, prop),
                prop => FormatElementName(options, prop)
            );
            _popupInput = _popupField.Q<VisualElement>(null, "unity-popup-field__input");
            _popupField.name = _popupFieldName;
            _popupField.AddToClassList("flex-grow-1");
            if (_changeCallback != null)
            {
                _popupField.RegisterValueChangedCallback(_changeCallback);
            }
            Add(_popupField);
        }

        protected abstract string FormatElementName(List<T> options, T element);

        public void SetOptions(List<T> options, int selectedIndex)
        {
            CreateField(options, selectedIndex);
            if (_pingWhenOptionsSet)
            {
                PingField();
            }
        }

        public void SetValue(T element, bool setWithoutNotify = false)
        {
            if (_popupField == null) return;

            if (setWithoutNotify)
            {
                _popupField.SetValueWithoutNotify(element);
            }
            else
            {
                _popupField.value = element;
            }
        }
        
        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<T>> callback)
        {
            _changeCallback = callback;
            if (_changeCallback != null)
            {
                _popupField.RegisterValueChangedCallback(_changeCallback);
            }
        }

        private void PingField()
        {
            if (_popupField == null) return;
            _popupField.schedule.Execute(() =>
            {
                var baseColor = _popupInput.resolvedStyle.backgroundColor;
                _popupInput.experimental.animation.Start(new StyleValues
                {
                    backgroundColor = new Color(0.3f, 0.71f, 0.37f, 0.53f)
                }, new StyleValues
                {
                    backgroundColor = baseColor
                }, 500);
            }).ExecuteLater(10);
        }
    }
}