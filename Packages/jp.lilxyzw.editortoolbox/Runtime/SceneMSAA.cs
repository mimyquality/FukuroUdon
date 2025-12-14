using UnityEngine;

namespace jp.lilxyzw.editortoolbox.runtime
{
    [Docs(
        "SceneMSAA",
        "Merged into [Scene View Extension](../SceneExtension)."
    )]
    [AddComponentMenu("/")]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(SceneMSAA))]
    internal class SceneMSAA : EditorOnlyBehaviour
    {
        #if UNITY_EDITOR
        void OnValidate()
        {
            void DestroySelf()
            {
                DestroyImmediate(this);
                UnityEditor.EditorApplication.update -= DestroySelf;
            }
            UnityEditor.EditorApplication.update += DestroySelf;
        }
        #endif
    }
}
