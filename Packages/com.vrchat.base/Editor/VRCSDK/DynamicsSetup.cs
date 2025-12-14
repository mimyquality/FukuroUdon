using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone;

namespace VRC.SDK3.Dynamics
{
    /// <summary>
    /// Project agnostic VRC dynamics setup.
    /// </summary>
    public static class DynamicsSetup
    {
        [InitializeOnLoadMethod]
        private static void EditorInit()
        {
            VRCConstraintManager.CanExecuteConstraintJobsInEditMode = VRC.SDKBase.Editor.VRCSettings.VrcConstraintsInEditMode;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                    VRCDynamicsScheduler.HandleEditorPlayModeToggle();
                    break;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            //Create singleton MonoBehaviours needed by dynamics

            //Contact Manager
            if (ContactManager.Inst == null)
            {
                var obj = new GameObject("ContactManager");
                UnityEngine.Object.DontDestroyOnLoad(obj);
                ContactManager.Inst = obj.AddComponent<ContactManager>();

                obj.hideFlags = HideFlags.HideInHierarchy;
            }

            //PhysBone Manager
            if (PhysBoneManager.Inst == null)
            {
                var obj = new GameObject("PhysBoneManager");
                UnityEngine.Object.DontDestroyOnLoad(obj);

                PhysBoneManager.Inst = obj.AddComponent<PhysBoneManager>();
                PhysBoneManager.Inst.IsSDK = true;
                PhysBoneManager.Inst.Init();

                obj.hideFlags = HideFlags.HideInHierarchy;
            }

            //Constraint Manager is not a MonoBehaviour
        }
    }
}