using UnityEngine.SceneManagement;
using UnityEditor.SceneTemplate;

/// <summary>Used to perform any pre- or post-processing when creating the default scene from template on first project launch</summary>
public class DefaultSceneTemplatePipeline : ISceneTemplatePipeline
{
    public virtual bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
    {
        return true;
    }

    public virtual void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive, string sceneName)
    {
        // run pre-instantiation logic here
    }

    public virtual void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    {
        // run post-instantiation logic here
    }
}
