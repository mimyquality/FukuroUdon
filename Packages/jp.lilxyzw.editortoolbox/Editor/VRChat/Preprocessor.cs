#if LIL_VRCSDK3
using jp.lilxyzw.editortoolbox.runtime;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.lilxyzw.editortoolbox.vrchat
{
    public class Preprocessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -10000;
        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            foreach(var c in avatarGameObject.GetComponentsInChildren<EditorOnlyBehaviour>())
                Object.DestroyImmediate(c);

            return true;
        }
    }
}
#endif
