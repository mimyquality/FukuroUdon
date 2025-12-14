using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;

namespace VRC.SDK3.Editor.Elements
{
    public class WorldUploadErrorNotification: VisualElement
    {
        public WorldUploadErrorNotification(string error)
        {
            Resources.Load<VisualTreeAsset>("WorldUploadErrorNotification").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("WorldUploadErrorNotificationStyles"));

            this.Q<Label>("notification-error-reason").text = error;
            
            var openUnityConsole = this.Q<Button>("open-unity-console");
            openUnityConsole.clicked += VRC_EditorTools.OpenConsoleWindow;
        }
    }
}