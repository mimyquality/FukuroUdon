#if VRC_SDK_VRCSDK3 && UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Versioning;
using VRCPickup = VRC.SDK3.Components.VRCPickup;

namespace VRC.SDK3.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRCPickup))]
    public class VRCPickupEditor3 : VRCInspectorBase
    {
        private SerializedProperty propVersion;
        private SerializedProperty propMomentumTransferMethod;
        private SerializedProperty propDisallowTheft;
        private SerializedProperty propExactGun;
        private SerializedProperty propExactGrip;
        private SerializedProperty propAllowManipulationWhenEquipped;
        private SerializedProperty propOrientation;
        private SerializedProperty propAutoHold;
        private SerializedProperty propInteractionText;
        private SerializedProperty propUseText;
        private SerializedProperty propThrowVelocityBoostMinSpeed;
        private SerializedProperty propThrowVelocityBoostScale;
        private SerializedProperty propPickupable;
        private SerializedProperty propProximity;

        private PropertyField fieldMomentumTransferMethod;
        private PropertyField fieldDisallowTheft;
        private PropertyField fieldExactGun;
        private PropertyField fieldExactGrip;
        private PropertyField fieldAllowManipulationWhenEquipped;
        private PropertyField fieldOrientation;
        private PropertyField fieldAutoHold;
        private PropertyField fieldInteractionText;
        private PropertyField fieldUseText;
        private PropertyField fieldThrowVelocityBoostMinSpeed;
        private PropertyField fieldThrowVelocityBoostScale;
        private PropertyField fieldPickupable;
        private PropertyField fieldProximity;
        
        private ComponentVersionUI<VRCPickup.Version> versionUI;

        
        private void OnEnable()
        {
            propVersion = serializedObject.FindProperty(nameof(VRCPickup.version));
            propMomentumTransferMethod = serializedObject.FindProperty(nameof(VRCPickup.MomentumTransferMethod));
            propDisallowTheft = serializedObject.FindProperty(nameof(VRCPickup.DisallowTheft));
            propExactGun = serializedObject.FindProperty(nameof(VRCPickup.ExactGun));
            propExactGrip = serializedObject.FindProperty(nameof(VRCPickup.ExactGrip));
            propAllowManipulationWhenEquipped = serializedObject.FindProperty(nameof(VRCPickup.allowManipulationWhenEquipped));
            propOrientation = serializedObject.FindProperty(nameof(VRCPickup.orientation));
            propAutoHold = serializedObject.FindProperty(nameof(VRCPickup.AutoHold));
            propInteractionText = serializedObject.FindProperty(nameof(VRCPickup.InteractionText));
            propUseText = serializedObject.FindProperty(nameof(VRCPickup.UseText));
            propThrowVelocityBoostMinSpeed = serializedObject.FindProperty(nameof(VRCPickup.ThrowVelocityBoostMinSpeed));
            propThrowVelocityBoostScale = serializedObject.FindProperty(nameof(VRCPickup.ThrowVelocityBoostScale));
            propPickupable = serializedObject.FindProperty(nameof(VRCPickup.pickupable));
            propProximity = serializedObject.FindProperty(nameof(VRCPickup.proximity));
        }

        #region Inspector GUI Construction (Common Pattern for All Custom Editors)
        
        public override void BuildInspectorGUI()
        {
            base.BuildInspectorGUI();
            
            // Add version system
            var migrator = new VRCPickupVersionMigrator(propAutoHold, propOrientation, propInteractionText, OnVersionChanged);
            versionUI = AddVersionSystem(propVersion, migrator, 
                "https://creators.vrchat.com/worlds/components/vrc_pickup#versions");
            // Create AutoHold enum field (shown in V1.0, hidden in V1.1+ when toggle is used)
            fieldAutoHold = AddField(propAutoHold);
            fieldUseText = AddFieldTooltip(propUseText,
                "Text to display describing action for clicking button, when this pickup is already being held.");
            fieldInteractionText = AddFieldTooltip(propInteractionText, 
                "Text displayed when user hovers over the pickup.");
            fieldProximity = AddField(propProximity);
            
            fieldOrientation = AddField(propOrientation);
            fieldOrientation.RegisterValueChangeCallback(OrientationCallback);
            
            fieldExactGun = AddField(propExactGun);
            fieldExactGrip = AddField(propExactGrip);
            fieldPickupable = AddField(propPickupable);
            fieldDisallowTheft = AddField(propDisallowTheft);
            fieldAllowManipulationWhenEquipped = AddField(propAllowManipulationWhenEquipped);
            fieldMomentumTransferMethod = AddField(propMomentumTransferMethod);
            fieldThrowVelocityBoostMinSpeed = AddField(propThrowVelocityBoostMinSpeed);
            fieldThrowVelocityBoostScale = AddField(propThrowVelocityBoostScale);
            
            // Now that all fields are created, we can update their visibility based on version
            var pickup = target as VRCPickup;
            SetupAutoHoldField(pickup.version);
            RefreshAllFieldVisibility(pickup.version);
        }

        // Creates the toggle field for modern AutoHold UI (V1.1+)
        private void SetupAutoHoldField(VRCPickup.Version version)
        {
            var container = fieldAutoHold.parent;
            
            if (!fieldAutoHold.ClassListContains("auto-hold-enum-field"))
                fieldAutoHold.AddToClassList("auto-hold-enum-field");
        
            var toggleField = container.Query().Name("autoHoldToggle").First() as Toggle;
            if (toggleField == null)
            {
                toggleField = new Toggle("Auto Hold")
                {
                    name = "autoHoldToggle",
                    tooltip = "When enabled, the pickup will stay in hand when trigger is released"
                };
                toggleField.AddToClassList("auto-hold-toggle-field");
        
                toggleField.RegisterValueChangedCallback(evt => {
                    serializedObject.Update();
                    propAutoHold.enumValueIndex = evt.newValue ?
                        (int)VRC_Pickup.AutoHoldMode.Yes :
                        (int)VRC_Pickup.AutoHoldMode.No;
                    serializedObject.ApplyModifiedProperties();
                    fieldUseText.SetVisible(evt.newValue);
                });
        
                container.Insert(container.IndexOf(fieldAutoHold) + 1, toggleField);
            }
        }
        
        // Switches between enum dropdown (V1.0) and toggle (V1.1+) for AutoHold field
        private void UpdateAutoHoldFieldVisibility(VRCPickup.Version version)
        {
            var container = fieldAutoHold.parent;
            var toggleField = container.Query().Name("autoHoldToggle").First() as Toggle;
        
            if (toggleField == null) return;
        
            if (IsModernVersion(version))
            {
                fieldAutoHold.style.display = DisplayStyle.None;
                toggleField.style.display = DisplayStyle.Flex;
                UpdateToggleFromEnumValue(toggleField);
            }
            else
            {
                fieldAutoHold.style.display = DisplayStyle.Flex;
                toggleField.style.display = DisplayStyle.None;
            }
        }
        
        // Syncs toggle state with underlying enum value without triggering callbacks
        private void UpdateToggleFromEnumValue(Toggle toggle)
        {
            toggle.SetValueWithoutNotify(IsAutoHoldEnabled(propAutoHold.enumValueIndex));
        }
        
        #endregion

        #region VRC Pickup-Specific Helper Methods
        
        // Checks if version uses modern UI (toggle instead of enum dropdown)
        private bool IsModernVersion(VRCPickup.Version version)
        {
            return version > VRCPickup.Version.Version_1_0;
        }
        
        // Determines if AutoHold is enabled based on enum value (handles Sometimes and Yes modes)
        private bool IsAutoHoldEnabled(int enumValue)
        {
            return enumValue == (int)VRC_Pickup.AutoHoldMode.Yes ||
                   enumValue == (int)VRC_Pickup.AutoHoldMode.Sometimes;
        }
        
        // Updates UseText field visibility when AutoHold setting changes
        private void AutoHoldChanged()
        {
            var pickup = target as VRCPickup;
            bool isAutoHoldEnabled = IsAutoHoldEnabled(propAutoHold.enumValueIndex);
            
            fieldUseText.SetVisible(isAutoHoldEnabled);
            
            // Update version upgrade info visibility
            versionUI?.RefreshUpgradeInfo();
        }

        private void OrientationCallback(SerializedPropertyChangeEvent evt) => OrientationChanged();

        // Shows/hides ExactGun and ExactGrip fields based on pickup orientation
        private void OrientationChanged()
        {
            switch ((VRC_Pickup.PickupOrientation)propOrientation.enumValueIndex)
            {
                case VRC_Pickup.PickupOrientation.Any:
                    fieldExactGun.SetVisible(true);
                    fieldExactGrip.SetVisible(true);
                    break;
                case VRC_Pickup.PickupOrientation.Grip:
                    fieldExactGun.SetVisible(false);
                    fieldExactGrip.SetVisible(true);
                    break;
                case VRC_Pickup.PickupOrientation.Gun:
                    fieldExactGun.SetVisible(true);
                    fieldExactGrip.SetVisible(false);
                    break;
            }
            
            // Update version upgrade info since orientation affects AutoDetect migration
            versionUI?.RefreshUpgradeInfo();
        }
        
        #endregion
        
        #region Version-Dependent UI Management (Pattern for Versioned Components)
        
        // Refreshes all field visibility based on current version and field states
        private void RefreshAllFieldVisibility(VRCPickup.Version version)
        {
            versionUI?.RefreshUpgradeInfo();
            UpdateAutoHoldFieldVisibility(version);
            OrientationChanged();
            AutoHoldChanged();
        }
        
        // Called when version changes to update UI elements
        private void OnVersionChanged(VRCPickup.Version newVersion)
        {
            RefreshAllFieldVisibility(newVersion);
        }
        
        #endregion
    }
}
#endif