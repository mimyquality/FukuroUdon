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
    using VRC.Dynamics;
    using VRC.SDK3.Dynamics.Constraint.Components;
    using VRC.SDK3.Dynamics.Contact.Components;
    using VRC.SDK3.Dynamics.PhysBone.Components;
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-component")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to Component")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToComponent : UdonSharpBehaviour
    {
        [SerializeField]
        private ActiveRelayEventType _eventType = default;
        [SerializeField]
        private Component[] _components = new Component[0];
        [SerializeField]
        private bool _invert = false;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            var count = 0;
            var tmp = new Component[_components.Length];
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

        private bool ValidateComponentType(Component component)
        {
            if (!component) { return false; }

            if (component is Collider) { return true; }
            if (component is Renderer) { return true; }
            // 個別に羅列の必要ある
            //if (component is Behaviour) { return true; }
            if (component is OcclusionPortal) { return true; }
            if (component is CanvasGroup) { return true; }
            if (component is ParentConstraint) { return true; }
            if (component is PositionConstraint) { return true; }
            if (component is RotationConstraint) { return true; }
            if (component is ScaleConstraint) { return true; }
            if (component is AimConstraint) { return true; }
            if (component is LookAtConstraint) { return true; }
            if (component is AudioSource) { return true; }
            if (component is AudioLowPassFilter) { return true; }
            if (component is AudioHighPassFilter) { return true; }
            if (component is AudioEchoFilter) { return true; }
            if (component is AudioDistortionFilter) { return true; }
            if (component is AudioChorusFilter) { return true; }
            if (component is AudioReverbFilter) { return true; }
            if (component is AudioReverbZone) { return true; }
            if (component is Light) { return true; }
            if (component is Camera) { return true; }
            if (component is Animator) { return true; }
            if (component is UdonBehaviour) { return true; }
            //if (component is UdonSharpBehaviour) { return true; } // なんとかする
            if (component is VRCParentConstraint) { return true; }
            if (component is VRCPositionConstraint) { return true; }
            if (component is VRCRotationConstraint) { return true; }
            if (component is VRCScaleConstraint) { return true; }
            if (component is VRCAimConstraint) { return true; }
            if (component is VRCLookAtConstraint) { return true; }
            if (component is VRCPhysBone) { return true; }
            if (component is VRCContactSender) { return true; }
            if (component is VRCContactReceiver) { return true; }

            return false;
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

        private void ToggleComponents(bool value)
        {
            foreach (var component in _components)
            {
                if (!component) continue;

                var type = component.GetType();
                if (type == typeof(GameObject)) { continue; }
                // Extra
                // 用途的に、アクティブならClose・非アクティブならOpenのが都合が良さそうなので反転
                else if (type == typeof(OcclusionPortal)) { var downCasted = (OcclusionPortal)component; downCasted.open = !value; }
                else if (type == typeof(CanvasGroup)) { var downCasted = (CanvasGroup)component; downCasted.interactable = value; }
                // Common
                else if (type.IsSubclassOf(typeof(Collider))) { var downCasted = (Collider)component; downCasted.enabled = value; }
                else if (type.IsSubclassOf(typeof(Renderer))) { var downCasted = (Renderer)component; downCasted.enabled = value; }
                // Constraint
                else if (type == typeof(ParentConstraint)) { var downCasted = (ParentConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(PositionConstraint)) { var downCasted = (PositionConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(RotationConstraint)) { var downCasted = (RotationConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(ScaleConstraint)) { var downCasted = (ScaleConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(AimConstraint)) { var downCasted = (AimConstraint)component; downCasted.enabled = !value; }
                else if (type == typeof(LookAtConstraint)) { var downCasted = (LookAtConstraint)component; downCasted.enabled = value; }
                // Audio
                else if (type == typeof(AudioSource)) { var downCasted = (AudioSource)component; downCasted.enabled = value; }
                else if (type == typeof(AudioLowPassFilter)) { var downCasted = (AudioLowPassFilter)component; downCasted.enabled = value; }
                else if (type == typeof(AudioHighPassFilter)) { var downCasted = (AudioHighPassFilter)component; downCasted.enabled = value; }
                else if (type == typeof(AudioEchoFilter)) { var downCasted = (AudioEchoFilter)component; downCasted.enabled = value; }
                else if (type == typeof(AudioDistortionFilter)) { var downCasted = (AudioDistortionFilter)component; downCasted.enabled = value; }
                else if (type == typeof(AudioChorusFilter)) { var downCasted = (AudioChorusFilter)component; downCasted.enabled = value; }
                else if (type == typeof(AudioReverbFilter)) { var downCasted = (AudioReverbFilter)component; downCasted.enabled = value; }
                else if (type == typeof(AudioReverbZone)) { var downCasted = (AudioReverbZone)component; downCasted.enabled = value; }
                // Behaviour
                else if (type == typeof(Light)) { var downCasted = (Light)component; downCasted.enabled = value; }
                else if (type == typeof(Camera)) { var downCasted = (Camera)component; downCasted.enabled = value; }
                else if (type == typeof(Animator)) { var downCasted = (Animator)component; downCasted.enabled = value; }
                // VRChat
                else if (type == typeof(UdonBehaviour)) { var downCasted = (UdonBehaviour)component; downCasted.enabled = value; }
                else if (type == typeof(VRCParentConstraint)) { var downCasted = (VRCParentConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(VRCPositionConstraint)) { var downCasted = (VRCPositionConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(VRCRotationConstraint)) { var downCasted = (VRCRotationConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(VRCScaleConstraint)) { var downCasted = (VRCScaleConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(VRCAimConstraint)) { var downCasted = (VRCAimConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(VRCLookAtConstraint)) { var downCasted = (VRCLookAtConstraint)component; downCasted.enabled = value; }
                else if (type == typeof(VRCPhysBone)) { var downCasted = (VRCPhysBone)component; downCasted.enabled = value; }
                else if (type == typeof(VRCContactSender)) { var downCasted = (VRCContactSender)component; downCasted.enabled = value; }
                else if (type == typeof(VRCContactReceiver)) { var downCasted = (VRCContactReceiver)component; downCasted.enabled = value; }
                // enabled が not exposed
                // else if (type.IsSubclassOf(typeof(Behaviour))) { var downCasted = (Behaviour)component; downCasted.enabled = value; }
                // else if (type == typeof(AudioListener)) { var downCasted = (AudioListener)component; downCasted.enabled = value; }
                // else if (type == typeof(Cloth)) { var downCasted = (Cloth)component; downCasted.enabled = value; }
            }
        }
    }
}
