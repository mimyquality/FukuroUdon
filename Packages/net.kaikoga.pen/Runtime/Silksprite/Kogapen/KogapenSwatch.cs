using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Silksprite.Kogapen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KogapenSwatch : UdonSharpBehaviour
    {
        [SerializeField] internal MeshRenderer swatchMesh;
        [SerializeField] internal LineRenderer lineRenderer;
        [SerializeField] internal Color color;

        MaterialPropertyBlock _workMaterialPropertyBlock;
        int _colorProperty;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        void OnValidate()
        {
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)) return;
            _RefreshColors();
        }
#endif

        void Start()
        {
            _colorProperty = VRCShader.PropertyToID("_Color");
            _RefreshColors();                                                                                         
        }

        public void _RefreshColors()
        {
            if (lineRenderer)
            {
                lineRenderer.endColor = lineRenderer.startColor = color;
                lineRenderer.loop = true; // PROTOCOL: loop = true indicates a swatch
            }

            if (!swatchMesh) return;
            _workMaterialPropertyBlock = _workMaterialPropertyBlock ?? new MaterialPropertyBlock();
            swatchMesh.GetPropertyBlock(_workMaterialPropertyBlock, 0);
            _workMaterialPropertyBlock.SetColor(_colorProperty, color.gamma); // conform to LineRenderer's color
            swatchMesh.SetPropertyBlock(_workMaterialPropertyBlock, 0);
        }

    }
}
