using UnityEngine;

namespace jp.lilxyzw.editortoolbox.runtime
{
    [Docs(
        "ObjectMarker",
        "By attaching this component to a GameObject, you can change the background color of the hierarchy to highlight it."
    )]
    [DocsHowTo("It is intended to be used to organize the hierarchy. For example, you can create a GameObject named `Summer`, add an ObjectMarker component, and place summer costumes under it, and similarly create `Autumn` and place autumn costumes under it, etc.")]
    [DisallowMultipleComponent]
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(ObjectMarker))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(ObjectMarker))]
    internal class ObjectMarker : EditorOnlyBehaviour
    {
        [Tooltip("The background color. You can set the color using the color picker.")]
        public Color color = new(1,0,0,0.2f);

        [Tooltip("Instead of painting the color over the entire background, it paints it as an underline.")]
        public bool underline = false;
    }
}
