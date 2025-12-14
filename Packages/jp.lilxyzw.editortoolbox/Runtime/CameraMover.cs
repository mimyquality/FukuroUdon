using UnityEngine;

namespace jp.lilxyzw.editortoolbox.runtime
{
    [Docs(
        "CameraMover",
        "By attaching this component to a camera, you can control the camera with WASDQE."
    )]
    [DocsHowTo("Just attach the component to the camera object and press the `Operate` button. You can end the interaction by clicking it again or pressing the escape key.")]
    [DisallowMultipleComponent]
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(CameraMover))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(CameraMover))]
    internal class CameraMover : EditorOnlyBehaviour
    {
        [Tooltip("The moving speed of the camera.")]
        public float moveSpeed = 1;

        [Tooltip("The amount the camera rotates when you move the mouse.")]
        public float sensitivity = 15;
    }
}
