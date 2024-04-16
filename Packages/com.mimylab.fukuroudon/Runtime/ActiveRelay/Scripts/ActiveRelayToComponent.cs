/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.Animations;
    //using UnityEngine.UI;
    //using TMPro;
    //using VRC.SDKBase;
    //using VRC.SDK3.Components;
    //using VRC.Udon;

    [AddComponentMenu("Fukuro Udon/Active Relay/Active Relay to Component")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ActiveRelayToComponent : UdonSharpBehaviour
    {
        [SerializeField]
        Object[] _components = new Object[0];
        [SerializeField]
        bool _invert = false;

        void OnEnable()
        {
            ToggleComponents(!_invert);
        }

        void OnDisable()
        {
            ToggleComponents(_invert);
        }

        void ToggleComponents(bool value)
        {
            foreach (var component in _components)
            {
                if (!component) continue;

                var type = component.GetType();
                if (type == typeof(GameObject)) { var downCasted = (GameObject)component; downCasted.SetActive(value); }
                // Collider
                else if (type == typeof(BoxCollider)) { var downCasted = (BoxCollider)component; downCasted.enabled = value; }
                else if (type == typeof(SphereCollider)) { var downCasted = (SphereCollider)component; downCasted.enabled = value; }
                else if (type == typeof(CapsuleCollider)) { var downCasted = (CapsuleCollider)component; downCasted.enabled = value; }
                else if (type == typeof(MeshCollider)) { var downCasted = (MeshCollider)component; downCasted.enabled = value; }
                else if (type == typeof(WheelCollider)) { var downCasted = (WheelCollider)component; downCasted.enabled = value; }
                //else if (type == typeof(TerrainCollider)) { var downCasted = (TerrainCollider)component; downCasted.enabled = enabled; }
                // Renderer
                else if (type == typeof(MeshRenderer)) { var downCasted = (MeshRenderer)component; downCasted.enabled = value; }
                else if (type == typeof(SkinnedMeshRenderer)) { var downCasted = (SkinnedMeshRenderer)component; downCasted.enabled = value; }
                else if (type == typeof(LineRenderer)) { var downCasted = (LineRenderer)component; downCasted.enabled = value; }
                else if (type == typeof(TrailRenderer)) { var downCasted = (TrailRenderer)component; downCasted.enabled = value; }
                else if (type == typeof(BillboardRenderer)) { var downCasted = (BillboardRenderer)component; downCasted.enabled = value; }
                else if (type == typeof(SpriteRenderer)) { var downCasted = (SpriteRenderer)component; downCasted.enabled = value; }
                //else if (type == typeof(UnityEngine.Tilemaps.TilemapRenderer)) { var downCasted = (UnityEngine.Tilemaps.TilemapRenderer)component; downCasted.enabled = enabled; }
                // 用途的に、アクティブならClose・非アクティブならOpenのが都合が良さそうなので反転
                else if (type == typeof(OcclusionPortal)) { var downCasted = (OcclusionPortal)component; downCasted.open = !value; }
                // Constraint
                else if (type == typeof(AimConstraint)) { var downCasted = (AimConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(LookAtConstraint)) { var downCasted = (LookAtConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(ParentConstraint)) { var downCasted = (ParentConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(PositionConstraint)) { var downCasted = (PositionConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(RotationConstraint)) { var downCasted = (RotationConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(ScaleConstraint)) { var downCasted = (ScaleConstraint)component; downCasted.enabled = value; }
            }
        }
    }
}
