/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
//using UnityEngine.UI;
//using TMPro;
//using VRC.SDKBase;
//using VRC.SDK3.Components;
//using VRC.Udon;

namespace MimyLab
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayToComponent : UdonSharpBehaviour
    {
        [SerializeField]
        bool _invert = false;

        [SerializeField]
        Object[] _components = new Object[0];

        System.Type _type;

        void OnEnable()
        {
            ToggleComponents(!_invert);
        }

        void OnDisable()
        {
            ToggleComponents(_invert);
        }

        void ToggleComponents(bool enabled)
        {
            foreach (var component in _components)
            {
                if (!component) continue;

                _type = component.GetType();
                if (_type == typeof(GameObject)) { var downCasted = (GameObject)component; downCasted.SetActive(enabled); }
                // Collider
                else if (_type == typeof(BoxCollider)) { var downCasted = (BoxCollider)component; downCasted.enabled = enabled; }
                else if (_type == typeof(SphereCollider)) { var downCasted = (SphereCollider)component; downCasted.enabled = enabled; }
                else if (_type == typeof(CapsuleCollider)) { var downCasted = (CapsuleCollider)component; downCasted.enabled = enabled; }
                else if (_type == typeof(MeshCollider)) { var downCasted = (MeshCollider)component; downCasted.enabled = enabled; }
                else if (_type == typeof(WheelCollider)) { var downCasted = (WheelCollider)component; downCasted.enabled = enabled; }
                //else if (type == typeof(TerrainCollider)) { var downCasted = (TerrainCollider)component; downCasted.enabled = enabled; }
                // Renderer
                else if (_type == typeof(MeshRenderer)) { var downCasted = (MeshRenderer)component; downCasted.enabled = enabled; }
                else if (_type == typeof(SkinnedMeshRenderer)) { var downCasted = (SkinnedMeshRenderer)component; downCasted.enabled = enabled; }
                else if (_type == typeof(LineRenderer)) { var downCasted = (LineRenderer)component; downCasted.enabled = enabled; }
                else if (_type == typeof(TrailRenderer)) { var downCasted = (TrailRenderer)component; downCasted.enabled = enabled; }
                else if (_type == typeof(BillboardRenderer)) { var downCasted = (BillboardRenderer)component; downCasted.enabled = enabled; }
                else if (_type == typeof(SpriteRenderer)) { var downCasted = (SpriteRenderer)component; downCasted.enabled = enabled; }
                //else if (type == typeof(UnityEngine.Tilemaps.TilemapRenderer)) { var downCasted = (UnityEngine.Tilemaps.TilemapRenderer)component; downCasted.enabled = enabled; }
                // 用途的に、アクティブならClose・非アクティブならOpenのが都合が良さそうなので反転
                else if (_type == typeof(OcclusionPortal)) { var downCasted = (OcclusionPortal)component; downCasted.open = !enabled; }
                // Constraint
                else if (_type == typeof(AimConstraint)) { var downCasted = (AimConstraint)component; downCasted.enabled = enabled; }
                else if (_type == typeof(LookAtConstraint)) { var downCasted = (LookAtConstraint)component; downCasted.enabled = enabled; }
                else if (_type == typeof(ParentConstraint)) { var downCasted = (ParentConstraint)component; downCasted.enabled = enabled; }
                else if (_type == typeof(PositionConstraint)) { var downCasted = (PositionConstraint)component; downCasted.enabled = enabled; }
                else if (_type == typeof(RotationConstraint)) { var downCasted = (RotationConstraint)component; downCasted.enabled = enabled; }
                else if (_type == typeof(ScaleConstraint)) { var downCasted = (ScaleConstraint)component; downCasted.enabled = enabled; }
            }
        }
    }
}
