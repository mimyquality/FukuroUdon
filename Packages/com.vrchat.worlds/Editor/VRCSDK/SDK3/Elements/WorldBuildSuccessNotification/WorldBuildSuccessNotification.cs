using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDK3A.Editor.Elements
{
    public class WorldBuildSuccessNotification: VisualElement
    {
        public WorldBuildSuccessNotification()
        {
            Resources.Load<VisualTreeAsset>("WorldBuildSuccessNotification").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("WorldBuildSuccessNotificationStyles"));
        }
    }
}