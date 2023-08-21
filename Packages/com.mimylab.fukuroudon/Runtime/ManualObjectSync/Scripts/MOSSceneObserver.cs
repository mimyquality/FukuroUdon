/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using VRC.SDK3.Components;

namespace MimyLab
{
    [ExecuteAlways]
    public class MOSSceneObserver : MonoBehaviour
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private VRCSceneDescriptor _sceneDescriptor;
        private float _respawnHeightY = -100;

        private MOSUpdateManager _updateManager;

        private void Start()
        {
            this.gameObject.tag = "EditorOnly";
            this.gameObject.hideFlags = HideFlags.HideInHierarchy;
            _updateManager = GetComponentInParent<MOSUpdateManager>();
        }

        private void Update()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { return; }
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) { return; }
            if (PrefabUtility.IsPartOfPrefabAsset(this)) { return; }

            var scene = this.gameObject.scene;
            if (!scene.IsValid()) { return; }
            if (!scene.isLoaded) { return; }

            if (!_sceneDescriptor)
            {
                if (!(_sceneDescriptor = FindObjectOfType<VRCSceneDescriptor>())) { return; }
            }

            if (_respawnHeightY != _sceneDescriptor.RespawnHeightY)
            {
                _respawnHeightY = _sceneDescriptor.RespawnHeightY;
                _updateManager.respawnHeightY = _respawnHeightY;
                EditorUtility.SetDirty(_updateManager);
                PrefabUtility.RecordPrefabInstancePropertyModifications(_updateManager);
                _updateManager.SetupAllMOS();
            }
        }
#endif
    }
}
