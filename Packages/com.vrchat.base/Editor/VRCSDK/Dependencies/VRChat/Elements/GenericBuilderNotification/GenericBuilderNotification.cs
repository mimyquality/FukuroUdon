using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDK3.Editor.Elements
{
    public class GenericBuilderNotification: VisualElement
    {
        public GenericBuilderNotification(string text, string details = null, string actionText = null, Action action = null)
        {
            Resources.Load<VisualTreeAsset>("GenericBuilderNotification").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("GenericBuilderNotificationStyles"));

            this.Q<Label>("main-text").text = text;

            if (!string.IsNullOrWhiteSpace(details))
            {
                var detailsLabel = this.Q<Label>("details-text");
                detailsLabel.text = details;
                detailsLabel.RemoveFromClassList("d-none");
            }

            if (action != null)
            {
                var actionButton = this.Q<Button>("action-button");
                actionButton.text = actionText;
                actionButton.clicked += action;
                actionButton.RemoveFromClassList("d-none");
            }
        }
    }
}