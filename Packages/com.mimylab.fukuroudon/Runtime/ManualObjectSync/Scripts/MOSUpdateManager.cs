/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using System;
    using UdonSharp;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
#endif

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Manual-ObjectSync#%E6%9B%B4%E6%96%B0%E7%AE%A1%E7%90%86%E3%82%AA%E3%83%96%E3%82%B8%E3%82%A7%E3%82%AF%E3%83%88")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MOSUpdateManager : UdonSharpBehaviour
    {
        internal float _respawnHeightY = -100.0f;
        private ManualObjectSync[] _mosList = Array.Empty<ManualObjectSync>();

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            SetupAllMOS();
        }

        internal void SetupAllMOS()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

            Scene scene = gameObject.scene;
            if (!scene.IsValid()) return;
            if (!scene.isLoaded) return;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                ManualObjectSync[] tmp_mosList = obj.GetComponentsInChildren<ManualObjectSync>(true);
                foreach (ManualObjectSync tmp_mos in tmp_mosList)
                {
                    tmp_mos.SetUpdateManager(this);
                    tmp_mos.SetRespawnHeightY(_respawnHeightY);
                    tmp_mos.RecordSelf();
                }
            }
        }
#endif
    }
}
