/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
//using VRC.SDKBase;
using VRC.SDK3.Components;
//using VRC.Udon;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace MimyLab
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MOSUpdateManager : UdonSharpBehaviour
    {
        internal float respawnHightY = -100.0f;
        private ManualObjectSync[] _mosList = new ManualObjectSync[0];

#if !COMPILER_UDONSHARP && UNITY_EDITOR        
        private void Reset()
        {
            SetupAllMOS();
        }

        internal void SetupAllMOS()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { return; }
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) { return; }
            if (PrefabUtility.IsPartOfPrefabAsset(this)) { return; }

            var scene = this.gameObject.scene;
            if (!scene.IsValid()) { return; }
            if (!scene.isLoaded) { return; }

            var sceneDescriptor = FindObjectOfType<VRCSceneDescriptor>();
            respawnHightY = (sceneDescriptor) ? sceneDescriptor.RespawnHeightY : respawnHightY;

            var rootObjects = scene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                var tmp_mosList = obj.GetComponentsInChildren<ManualObjectSync>(true);
                foreach (var tmp_mos in tmp_mosList)
                {
                    tmp_mos.SetUpdateManager(this);
                    tmp_mos.SetRespawnHeightY(respawnHightY);
                    tmp_mos.RecordSelf();
                }
            }
        }
#endif

        public override void PostLateUpdate()
        {
            var pauseUpdate = true;
            for (int i = 0; i < _mosList.Length; i++)
            {
                if (_mosList[i])
                {
                    _mosList[i]._OnPostLateUpdate();
                    pauseUpdate = false;
                }
            }

            if (pauseUpdate) { this.enabled = false; }
        }

        public void EnablePostLateUpdate(ManualObjectSync mos)
        {
            this.enabled = true;

            var index = System.Array.IndexOf(_mosList, mos);
            if (index >= 0) { return; }

            index = System.Array.IndexOf(_mosList, null);
            if (index >= 0)
            {
                _mosList[index] = mos;
            }
            else
            {
                var tmp_MosList = new ManualObjectSync[_mosList.Length + 1];
                tmp_MosList[0] = mos;
                _mosList.CopyTo(tmp_MosList, 1);
                _mosList = tmp_MosList;
            }
        }

        public void DisablePostLateUpdate(ManualObjectSync mos)
        {
            if (_mosList.Length <= 0) { return; }

            var index = System.Array.IndexOf(_mosList, mos);
            if (index >= 0)
            {
                _mosList[index] = null;
            }
        }
    }
}
