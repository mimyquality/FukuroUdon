using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDKBase.Editor.Elements
{
    public class OptionsPopupContent: PopupWindowContent
    {
        private readonly Action<VisualElement> _setup;
        private readonly Action _onClose;
        private readonly Vector2 _windowSize;
        
        public override Vector2 GetWindowSize()
        {
            return _windowSize;
        }
        
        public OptionsPopupContent(Action<VisualElement> setup, Vector2 size, Action onClose = null)
        {
            _setup = setup;
            _windowSize = size;
            _onClose = onClose;
        }
        
        public override void OnGUI(Rect rect)
        {
            // Legacy stub per unity docs
            // https://docs.unity3d.com/2022.3/Documentation/Manual/UIE-create-a-popup-window.html
        }

        private VisualElement _root;

        // Essentially the `CreateGUI` alternative
        public override void OnOpen()
        {
            _root = editorWindow.rootVisualElement;
            _root.AddToClassList("options-popup-content");
            
            _root.styleSheets.Add(Resources.Load<StyleSheet>("OptionsPopupFieldStyles"));
            
            _setup(_root);
        }

        public override void OnClose()
        {
            _onClose?.Invoke();
        }
    }
}