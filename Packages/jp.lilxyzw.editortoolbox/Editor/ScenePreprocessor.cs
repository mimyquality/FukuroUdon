using jp.lilxyzw.editortoolbox.runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace jp.lilxyzw.editortoolbox
{
    internal class ScenePreprocessor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if(!EditorApplication.isPlaying)
            foreach(var root in scene.GetRootGameObjects())
            foreach(var component in root.GetComponentsInChildren<EditorOnlyBehaviour>(true))
            {
                Debug.Log(component);
                Object.DestroyImmediate(component);
            }
        }
    }
}
