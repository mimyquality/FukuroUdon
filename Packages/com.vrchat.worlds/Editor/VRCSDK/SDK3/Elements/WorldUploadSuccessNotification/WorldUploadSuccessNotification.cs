using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDK3.Editor.Elements
{
    public class WorldUploadSuccessNotification: VisualElement
    {
        public WorldUploadSuccessNotification(string id)
        {
            Resources.Load<VisualTreeAsset>("WorldUploadSuccessNotification").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("WorldUploadSuccessNotificationStyles"));
            
            var openWorldPageButton = this.Q<Button>("open-world-page-button");
            openWorldPageButton.clicked += () =>
            {
                Application.OpenURL($"https://vrchat.com/home/world/{id}");
            };
        }
    }
}