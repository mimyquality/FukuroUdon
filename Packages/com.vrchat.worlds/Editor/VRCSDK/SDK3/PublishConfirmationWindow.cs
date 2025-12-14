using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDK3.Editor
{
    public class PublishConfirmationWindow: PopupWindowContent
    {
        //Set the window size
        public override Vector2 GetWindowSize()
        {
            return new Vector2(400, 300);
        }

        public override void OnGUI(Rect rect)
        {
            // Intentionally left empty
        }
        
        private string _title;
        private string _text;
        private string _yesText;
        private string _noText;
        private Action _yesAction;
        private Action _noAction;

        public PublishConfirmationWindow(string title, string text, string yesText, string noText, Action yesAction = null, Action noAction = null)
        {
            _title = title;
            _text = text;
            _yesText = yesText;
            _noText = noText;
            _yesAction = yesAction;
            _noAction = noAction;
        }

        private VisualElement _r;

        public override void OnOpen()
        {
            _r = editorWindow.rootVisualElement;
            Resources.Load<VisualTreeAsset>("PublishConfirmationWindow").CloneTree(_r);
            _r.styleSheets.Add(Resources.Load<StyleSheet>("PublishConfirmationWindowStyles"));
            _r.styleSheets.Add(Resources.Load<StyleSheet>("VRCSdkPanelStyles"));
            
            _r.Q<Label>("title").text = _title;
            _r.Q<Label>("body").text = _text;
            _r.Q<Button>("yes-button").text = _yesText;
            _r.Q<Button>("no-button").text = _noText;
            if (_yesAction != null)
            {
                _r.Q<Button>("yes-button").clicked += _yesAction;
            }
            _r.Q<Button>("yes-button").clicked += editorWindow.Close;
            if (_noAction != null)
            {
                _r.Q<Button>("no-button").clicked += _noAction;
            }
            _r.Q<Button>("no-button").clicked += editorWindow.Close;
        }
    }
}