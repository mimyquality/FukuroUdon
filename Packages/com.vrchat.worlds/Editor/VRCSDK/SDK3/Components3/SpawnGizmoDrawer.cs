using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using VRCSceneDescriptor = VRC.SDK3.Components.VRCSceneDescriptor;

namespace VRC.SDK3.Editor
{
    [InitializeOnLoad]
    public static class SpawnGizmoDrawer
    {
        private static VRCSceneDescriptor sceneDescriptor;
        
        static SpawnGizmoDrawer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (Selection.activeTransform == null)
                return;

            if (sceneDescriptor == null)
            {
                sceneDescriptor = GameObject.FindObjectOfType<VRCSceneDescriptor>();
                if (sceneDescriptor == null)
                    return;
            }

            Transform selected = Selection.activeTransform;
            foreach (Transform spawn in sceneDescriptor.spawns)
            {
                if (spawn == null)
                    continue;

                if (spawn == selected)
                {
                    if (sceneDescriptor.spawnRadius != 0)
                        Handles.DrawWireDisc(spawn.position, Vector3.up, sceneDescriptor.spawnRadius);
                    else
                    {
                        float size = HandleUtility.GetHandleSize(spawn.position) * 0.5f;
                        Vector3 pos = spawn.position;
                        Vector3 offset1 = (Vector3.forward + Vector3.right).normalized * size;
                        Vector3 offset2 = (Vector3.forward - Vector3.right).normalized * size;
                        Handles.DrawLine(pos + offset1, pos - offset1);
                        Handles.DrawLine(pos + offset2, pos - offset2);
                    }
                }
            }
        }
    }
}