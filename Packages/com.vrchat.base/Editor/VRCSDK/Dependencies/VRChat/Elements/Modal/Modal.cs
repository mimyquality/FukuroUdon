using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class Modal : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Modal, UxmlTraits> {}
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _title = new() { name = "title" };
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get
                {
                    yield return new UxmlChildElementDescription(typeof(VisualElement));
                }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var modal = (Modal) ve;
                modal._title.text = _title.GetValueFromBag(bag, cc);
            }
        }
        
        private readonly Label _title;
        private readonly Button _closeButton;
        private readonly VisualElement _container;
        private readonly VisualElement _contentWrapper;
        private readonly VisualElement _actionButtonWrapper;
        private readonly Button _actionButton;
        private readonly VisualElement _icon;
        private StyleLength _parentHeight;

        private bool _hasActionButton;
        
        public override VisualElement contentContainer => _container;

        [PublicAPI]
        public EventHandler OnClose;
        [PublicAPI]
        public bool IsOpen { get; private set; }
        [PublicAPI]
        public EventHandler OnCancel;

        private VisualElement _anchor;
        private VisualElement _originalParent;

        private bool _isTemporary;

        public Modal()
        {
            Resources.Load<VisualTreeAsset>("Modal").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("ModalStyles"));
            
            AddToClassList("d-none");
            AddToClassList("absolute");
            AddToClassList("col");

            _title = this.Q<Label>("modal-title");
            _closeButton = this.Q<Button>("modal-close-btn");
            _contentWrapper = this.Q("modal-content-wrapper");
            _container = _contentWrapper.Q("modal-content");
            _icon = this.Q<VisualElement>("modal-icon");
            _actionButtonWrapper = this.Q("modal-action-button-wrapper");
            _actionButton = this.Q<Button>("modal-action-button");
            var backdrop = this.Q("modal-backdrop");
            
            
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                // Only save the initial parent, ignoring the future re-parenting
                if (_originalParent != null) return;
                _originalParent = parent;
            });

            void OnCloseCancel()
            {
                // If there is an action button, treat the backdrop/close click as a cancel
                if (_hasActionButton)
                {
                    OnCancel?.Invoke(this, EventArgs.Empty);
                }

                Close();
            }

            _closeButton.clicked += OnCloseCancel;
            backdrop.RegisterCallback<MouseDownEvent>(_ =>
            {
                OnCloseCancel();
            });
        }

        public Modal(VisualElement anchor) : this()
        {
            _anchor = anchor;
        }
        
        public Modal(string title, string content, VisualElement anchor) : this(anchor)
        {
            _title.text = title;
            var splitContent = content.Split('\n');
            _container.AddToClassList("p-3");
            foreach (var line in splitContent)
            {
                var label = new Label(line)
                {
                    style =
                    {
                        whiteSpace = WhiteSpace.Normal
                    }
                };
                _container.Add(label);
            }
        }
        
        public Modal(string title, string content, Action buttonAction, string buttonActionText, VisualElement anchor) : this(title, content, anchor)
        {
            _actionButton.clicked += () =>
            {
                buttonAction?.Invoke();
                Close();
            };
            _actionButtonWrapper.RemoveFromClassList("d-none");
            _actionButton.text = !string.IsNullOrWhiteSpace(buttonActionText) ? buttonActionText : "OK";
            _hasActionButton = true;
            _container.AddToClassList("mr-2");
        }
        
        /// <summary>
        /// Shorthand method for creating and showing a modal in place
        /// Calling `Close` on such a modal - immediately removes it from the hierarchy
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        [PublicAPI]
        public static Modal CreateAndShow(string title, string content, VisualElement anchor)
        {
            var modal = new Modal(title, content, anchor)
            {
                _isTemporary = true
            };
            anchor.Add(modal);
            modal.Open();
            return modal;
        }
        
        /// <summary>
        /// Shorthand method for creating and showing a modal in place
        /// Calling `Close` on such a modal - immediately removes it from the hierarchy
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="anchor"></param>
        /// <param name="buttonAction">If provided - adds a button that calls this action</param>
        /// <param name="buttonActionText">Sets the text of the action button if `buttonAction` is provided</param>
        /// <returns></returns>
        [PublicAPI]
        public static Modal CreateAndShow(string title, string content, Action buttonAction, string buttonActionText, VisualElement anchor)
        {
            var modal = new Modal(title, content, buttonAction, buttonActionText, anchor)
            {
                _isTemporary = true
            };
            anchor.Add(modal);
            modal.Open();
            return modal;
        }
        
        /// <summary>
        /// Sets the element to re-anchor into
        /// </summary>
        /// <param name="anchor"></param>
        [PublicAPI]
        public void SetAnchor(VisualElement anchor)
        {
            _anchor = anchor;
            RemoveFromHierarchy();
            _anchor.Add(this);
        }

        [PublicAPI]
        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            RemoveFromClassList("d-none");
            _parentHeight = parent.style.height;
            schedule.Execute(() =>
            {
                if (parent.contentRect.height < layout.height + 40)
                {
                    parent.style.height = layout.height + 40;
                }
            }).ExecuteLater(1);
        }

        [PublicAPI]
        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            AddToClassList("d-none");
            parent.style.height = _parentHeight;
            OnClose?.Invoke(this, EventArgs.Empty);
            // If there is no action button - treat any close as cancel
            if (!_hasActionButton)
            {
                OnCancel?.Invoke(this, EventArgs.Empty);
            }
            if (_isTemporary)
            {
                RemoveFromHierarchy();
            }
        }
        
        [PublicAPI]
        public void SetTitle(string title)
        {
            _title.text = title;
        }

        [PublicAPI]
        public void SetIcon(string resourceName)
        {
            _icon.RemoveFromClassList("d-none");
            _icon.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>(resourceName));
        }
    }
}