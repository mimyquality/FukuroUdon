using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    internal class IconPopup : VisualElement
    {
        private ToolbarButton root;
        private Image icon;
        public string value;
        public List<string> choices;
        public Action<string> valueChanged;
        public Action rightClicked;

        public IconPopup(Texture tex, List<string> choices, string defaultValue)
        {
            root = new ToolbarButton();
            icon = new Image{image = tex};
            value = defaultValue;
            this.choices = choices;

            root.Add(icon);
            root.clicked += ShowMenu;
            root.RegisterCallback<PointerDownEvent>((e) => {
                if(e.button == 1) rightClicked?.Invoke();
            });
            Add(root);
        }

        private void ShowMenu()
        {
            var menu = new GenericDropdownMenu();
            foreach(var choice in choices) menu.AddItem(choice, choice == value, () => {
                value = choice;
                valueChanged?.Invoke(choice);
            });
            var rect = worldBound;
            rect.width = choices.Max(c => Common.GetTextWidth(c)) + 28;
            menu.DropDown(rect, this, anchored: true);
        }
    }
}
