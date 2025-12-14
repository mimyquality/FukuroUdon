/*
Copyright (c) 2023 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using UnityEditor;
using UdonSharp;
using UdonSharpEditor;
using QvPen.UdonScript;

namespace MimyLab.PenOptimizationUtility
{
    [CustomEditor(typeof(PenOptimizeForQvPen))]
    public class PenOptimizeForQvPenEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) { return; }

            var behaviour = (PenOptimizeForQvPen)target;
            if (GUILayout.Button("Setup")) { Setup(behaviour); }

            UdonSharpGUI.DrawVariables(behaviour);
        }

        public void Setup(PenOptimizeForQvPen behaviour)
        {
            if (!behaviour.sharedMaterial_PC)
            {
                behaviour.sharedMaterial_PC = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("f0474e21fecf57342afc8aa44596180c"));
            }
            if (!behaviour.sharedMaterial_Mobile)
            {
                behaviour.sharedMaterial_Mobile = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("652005b0959593a46b5ca60a3873731b"));
            }
            behaviour.sharedMaterial_PC.enableInstancing = true;
            behaviour.sharedMaterial_Mobile.enableInstancing = true;

            var qvPenManagers = behaviour.GetComponentsInChildren<QvPen_PenManager>(true);
            var penColor = Color.white;
            var qvPens = new QvPen_Pen[qvPenManagers.Length];
            var penMeshs = new MeshRenderer[qvPenManagers.Length];
            var penMPOs = new MaterialPropertyOverwriter[qvPenManagers.Length];
            for (int i = 0; i < qvPenManagers.Length; i++)
            {
                penColor = qvPenManagers[i].colorGradient.colorKeys[0].color;

                qvPens[i] = qvPenManagers[i].GetComponentInChildren<QvPen_Pen>(true);
                for (int j = 0; j < qvPens[i].transform.childCount; j++)
                {
                    if (penMeshs[i] = qvPens[i].transform.GetChild(j).GetComponent<MeshRenderer>())
                    {
                        penMPOs[i] = SetupPenMesh(penMeshs[i], behaviour.materialIndex, penColor);
                        break;
                    }
                }
            }
            behaviour.sharingTargets = penMPOs;
            UdonSharpEditorUtility.CopyProxyToUdon(behaviour);
        }

        private MaterialPropertyOverwriter SetupPenMesh(MeshRenderer penMesh, int materialIndex, Color penColor)
        {
            penMesh.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            var mpo = penMesh.GetComponent<MaterialPropertyOverwriter>() ?? UdonSharpUndo.AddComponent<MaterialPropertyOverwriter>(penMesh.gameObject);
            mpo.executeOnActive = true;
            mpo.materialIndex = materialIndex;
            mpo._ClearPropertyAll();
            // 虹グラデーションになってそうなら白にしてテクスチャーを設定
            var penTexture = penMesh.sharedMaterials[materialIndex].GetTexture("_MainTex");
            if (penTexture)
            {
                mpo._AddColorProperty("_Color", Color.white);
                mpo._AddTextureProperty("_MainTex", penTexture);
            }
            else
            {
                mpo._AddColorProperty("_Color", penColor);
            }
            UdonSharpEditorUtility.CopyProxyToUdon(mpo);

            return mpo;
        }
    }
}
