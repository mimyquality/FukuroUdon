/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.Animations;

    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/Active Relay/ActiveRelay to Component")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToComponent : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private Object[] _components = new Object[0];
        [SerializeField]
        private bool _invert = false;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            var count = 0;
            var tmp = new Object[_components.Length];
            foreach (var component in _components)
            {
                if (ValidateComponentType(component))
                {
                    tmp[count++] = component;
                }
            }
            System.Array.Resize(ref tmp, count);
            _components = tmp;
        }
#endif

        private void OnEnable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Active)
            {
                ToggleComponents(!_invert);
            }
        }

        private void OnDisable()
        {
            if (_eventType == ActiveRelayEventType.ActiveAndInactive
             || _eventType == ActiveRelayEventType.Inactive)
            {
                ToggleComponents(_invert);
            }
        }

        private bool ValidateComponentType(Object component)
        {
            if (!component) { return false; }

            var type = component.GetType();
            if (type == typeof(GameObject)) { return false; }
            // Collider
            else if (type == typeof(BoxCollider)) { return true; }
            else if (type == typeof(SphereCollider)) { return true; }
            else if (type == typeof(CapsuleCollider)) { return true; }
            else if (type == typeof(MeshCollider)) { return true; }
            else if (type == typeof(WheelCollider)) { return true; }
            //else if (type == typeof(TerrainCollider)) { return true; }
            // Renderer
            else if (type == typeof(MeshRenderer)) { return true; }
            else if (type == typeof(SkinnedMeshRenderer)) { return true; }
            else if (type == typeof(LineRenderer)) { return true; }
            else if (type == typeof(TrailRenderer)) { return true; }
            else if (type == typeof(BillboardRenderer)) { return true; }
            else if (type == typeof(SpriteRenderer)) { return true; }
            //else if (type == typeof(UnityEngine.Tilemaps.TilemapRenderer)) { return true; }
            // Constraint
            else if (type == typeof(AimConstraint)) { return true; }
            else if (type == typeof(LookAtConstraint)) { return true; }
            else if (type == typeof(ParentConstraint)) { return true; }
            else if (type == typeof(PositionConstraint)) { return true; }
            else if (type == typeof(RotationConstraint)) { return true; }
            else if (type == typeof(ScaleConstraint)) { return true; }
            // Behaviour
            else if (type == typeof(Light)) { return true; }
            else if (type == typeof(Camera)) { return true; }
            else if (type == typeof(Animator)) { return true; }
            // Extra
            else if (type == typeof(OcclusionPortal)) { return true; }
            else if (type == typeof(CanvasGroup)) { return true; }

            return false;
        }

        private void ToggleComponents(bool value)
        {
            foreach (var component in _components)
            {
                if (!component) continue;

                var type = component.GetType();
                if (type == typeof(GameObject)) { return; }
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
                // Constraint
                else if (type == typeof(AimConstraint)) { var downCasted = (AimConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(LookAtConstraint)) { var downCasted = (LookAtConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(ParentConstraint)) { var downCasted = (ParentConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(PositionConstraint)) { var downCasted = (PositionConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(RotationConstraint)) { var downCasted = (RotationConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(ScaleConstraint)) { var downCasted = (ScaleConstraint)component; downCasted.enabled = value; }
                // Behaviour
                else if (type == typeof(Light)) { var downCasted = (Light)component; downCasted.enabled = value; }
                else if (type == typeof(Camera)) { var downCasted = (Camera)component; downCasted.enabled = value; }
                else if (type == typeof(Animator)) { var downCasted = (Animator)component; downCasted.enabled = value; }
                // Extra
                // 用途的に、アクティブならClose・非アクティブならOpenのが都合が良さそうなので反転
                else if (type == typeof(OcclusionPortal)) { var downCasted = (OcclusionPortal)component; downCasted.open = !value; }
                else if (type == typeof(CanvasGroup)) { var downCasted = (CanvasGroup)component; downCasted.interactable = value; }
            }
        }
    }
}
