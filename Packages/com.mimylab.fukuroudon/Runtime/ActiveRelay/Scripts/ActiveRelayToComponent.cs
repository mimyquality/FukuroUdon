/*
Copyright (c) 2022 Mimy Quality
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

namespace MimyLab.FukuroUdon
{
    using UdonSharp;
    using UnityEngine;
    using VRC.Udon;

    [HelpURL("https://github.com/mimyquality/FukuroUdon/wiki/Active-Relay#activerelay-to-component")]
    [Icon(ComponentIconPath.FukuroUdon)]
    [AddComponentMenu("Fukuro Udon/ActiveRelay to/ActiveRelay to Component")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActiveRelayToComponent : ActiveRelayTo
    {
        [SerializeField]
        private ActiveRelayActiveEvent _eventType = default;
        [SerializeField]
        private Component[] _components = new Component[0];
        [SerializeField]
        private bool _invert = false;

        [SerializeField, HideInInspector]
        private UdonSharpBehaviour[] _udonSharpBehaviours = new UdonSharpBehaviour[0];

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnValidate()
        {
            var tmp_Components = new Component[_components.Length];
            var componentCount = 0;
            // U# はプロキシー概念があるので別途 UdonSharpBehaviour 配列を用意して管理する
            var tmp_udonSharpBehaviours = new UdonSharpBehaviour[_components.Length];
            var usbCount = 0;
            foreach (Component component in _components)
            {
                if (ValidateComponentType(component))
                {
                    tmp_Components[componentCount++] = component;

                    if (component is UdonSharpBehaviour usb)
                    {
                        tmp_udonSharpBehaviours[usbCount++] = usb;
                    }
                }
            }
            System.Array.Resize(ref tmp_Components, componentCount);
            _components = tmp_Components;
            System.Array.Resize(ref tmp_udonSharpBehaviours, usbCount);
            _udonSharpBehaviours = tmp_udonSharpBehaviours;
        }

        private bool ValidateComponentType(Component component)
        {
            if (!component) { return false; }

            if (component is Behaviour) { return true; }
            // Behaviour でないが enabled が存在するもの
            if (component is Collider) { return true; }
            if (component is Renderer) { return true; }
            if (component is UdonBehaviour) { return true; }
            if (component is UdonSharpBehaviour) { return true; }
            // 特殊用途のもの
            if (component is OcclusionPortal) { return true; }
            if (component is CanvasGroup) { return true; }

            return false;
        }
#endif

        private protected override void OnEnable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Active)
            {
                ToggleComponents(!_invert);
            }
        }

        private protected override void OnDisable()
        {
            if (_eventType == ActiveRelayActiveEvent.ActiveAndInactive
             || _eventType == ActiveRelayActiveEvent.Inactive)
            {
                ToggleComponents(_invert);
            }
        }

        private void ToggleComponents(bool value)
        {
            foreach (Component component in _components)
            {
                if (!component) continue;

                System.Type type = component.GetType();
                if (type == typeof(GameObject)) { continue; }
                // Extra
                // 用途的に、アクティブならClose・非アクティブならOpenのが都合が良さそうなので反転
                else if (type == typeof(OcclusionPortal)) { var downCasted = (OcclusionPortal)component; downCasted.open = !value; }
                else if (type == typeof(CanvasGroup)) { var downCasted = (CanvasGroup)component; downCasted.interactable = value; }
                // Common
                else if (type.IsSubclassOf(typeof(Behaviour))) { var downCasted = (Behaviour)component; downCasted.enabled = value; }
                else if (type.IsSubclassOf(typeof(Collider))) { var downCasted = (Collider)component; downCasted.enabled = value; }
                else if (type.IsSubclassOf(typeof(Renderer))) { var downCasted = (Renderer)component; downCasted.enabled = value; }
                // enabled が not exposed
                // else if (type == typeof(Cloth)) { var downCasted = (Cloth)component; downCasted.enabled = value; }
            }
            foreach (UdonSharpBehaviour udonSharpBehaviour in _udonSharpBehaviours)
            {
                if (!udonSharpBehaviour) continue;
                udonSharpBehaviour.enabled = value;
            }
        }
    }
}
