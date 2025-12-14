using UnityEditor;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Versioning;

namespace VRC.SDK3.Editor
{
    /// <summary>
    /// Version migration logic for VRCPickup
    /// </summary>
    internal class VRCPickupVersionMigrator : ComponentVersionMigrator<VRCPickup.Version>
    {
        private readonly SerializedProperty autoHoldProperty;
        private readonly SerializedProperty orientationProperty;
        private readonly SerializedProperty interactionTextProperty;
        private readonly System.Action<VRCPickup.Version> onVersionChangedCallback;
        
        public VRCPickupVersionMigrator(SerializedProperty autoHoldProperty, SerializedProperty orientationProperty, SerializedProperty interactionTextProperty, System.Action<VRCPickup.Version> onVersionChangedCallback = null)
        {
            this.autoHoldProperty = autoHoldProperty;
            this.orientationProperty = orientationProperty;
            this.interactionTextProperty = interactionTextProperty;
            this.onVersionChangedCallback = onVersionChangedCallback;
        }
        
        // Creates the success dialog message explaining what specific changes were made during upgrade
        public override string GetUpgradeChangesMessage(VRCPickup.Version oldVersion, VRCPickup.Version newVersion, SerializedObject serializedObject)
        {
            if (newVersion <= oldVersion) return null;
            
            // Handle multi-selection with simplified message
            if (serializedObject.isEditingMultipleObjects)
                return $"Version upgraded successfully!\n{serializedObject.targetObjects.Length} pickup(s) upgraded to Version 1.1.";
            
            var orientation = (VRC_Pickup.PickupOrientation)orientationProperty.enumValueIndex;
            var message = "Version upgraded successfully!\n";
            
            int oldAutoHoldValue = autoHoldProperty.enumValueIndex;
            var changes = new System.Collections.Generic.List<string>();
            
            if (oldAutoHoldValue == (int)VRC_Pickup.AutoHoldMode.Sometimes)
            {
                changes.Add("Changed 'Sometimes' to 'Yes' for better controller compatibility");
            }
            else if (oldAutoHoldValue == (int)VRC_Pickup.AutoHoldMode.AutoDetect)
            {
                bool willBeAutoHold = VRC_Pickup.IsGlobalAutoHoldPickup((VRC_Pickup.AutoHoldMode)oldAutoHoldValue, orientation);
                var newValue = willBeAutoHold ? "Yes" : "No";
                               
                var change = $"Changed 'Auto Detect' to '{newValue}' based on orientation ({orientation})";
                
                if (newValue == "No")
                {
                    change += "\n'Any' orientation pickups don't use auto-hold";
                }
                else
                {
                    change += "\nGrip/Gun orientation pickups use auto-hold";
                }
                
                changes.Add(change);
            }
            
            if (changes.Count > 0)
            {
                message += "Changes made:\n" + string.Join("\n", changes);
            }
            else
            {
                message += "No changes needed - your settings are already compatible with this version.";
            }
            
            return message;
        }
        
        // Performs the actual data migration when upgrading to a specific version
        public override void MigrateToVersion(VRCPickup.Version oldVersion, VRCPickup.Version newVersion, SerializedObject serializedObject)
        {
            // Handle migration from old busted AutoHold to Global AutoHold
            if (oldVersion == VRCPickup.Version.Version_1_0)
            {
                var currentOrientation = (VRC_Pickup.PickupOrientation)orientationProperty.enumValueIndex;
                var currentAutoHoldMode = (VRC_Pickup.AutoHoldMode)autoHoldProperty.enumValueIndex;
                
                bool willBeAutoHold = VRC_Pickup.IsGlobalAutoHoldPickup(currentAutoHoldMode, currentOrientation);
                var newAutoHoldMode = willBeAutoHold ? VRC_Pickup.AutoHoldMode.Yes : VRC_Pickup.AutoHoldMode.No;
                
                if ((int)newAutoHoldMode != autoHoldProperty.enumValueIndex)
                {
                    autoHoldProperty.enumValueIndex = (int)newAutoHoldMode;
                    autoHoldProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }
        
        // Determines when to show the upgrade info box at the top of the inspector
        public override bool ShouldShowUpgradePrompt(VRCPickup.Version version, SerializedObject serializedObject)
        {
            // Always show upgrade for ANY Version 1.0 pickup in multi-selection
            if (serializedObject.isEditingMultipleObjects)
                return version == VRCPickup.Version.Version_1_0;
            
            return version == VRCPickup.Version.Version_1_0 && 
                   (autoHoldProperty.enumValueIndex == (int)VRC_Pickup.AutoHoldMode.Sometimes ||
                    autoHoldProperty.enumValueIndex == (int)VRC_Pickup.AutoHoldMode.AutoDetect);
        }
        
        // Returns message about why they should upgrade
        public override string GetUpgradePromptText(VRCPickup.Version currentVersion, VRCPickup.Version latestVersion)
        {
            // Multi-selection mode - simplified message
            if (autoHoldProperty.serializedObject.isEditingMultipleObjects)
                return "Selected pickups use older settings.\nUpgrade all to Version 1.1 for better compatibility and simplified options.";
            
            int currentAutoHoldValue = autoHoldProperty.enumValueIndex;
            var orientation = (VRC_Pickup.PickupOrientation)orientationProperty.enumValueIndex;
            bool willBeAutoHold = VRC_Pickup.IsGlobalAutoHoldPickup((VRC_Pickup.AutoHoldMode)currentAutoHoldValue, orientation);
            
            // Handle Sometimes - always problematic
            if (currentAutoHoldValue == (int)VRC_Pickup.AutoHoldMode.Sometimes)
            {
                return "This pickup uses older auto-hold settings that don't work for many controllers.\n" +
                       "Upgrade to Version 1.1 for better compatibility and reduced hand fatigue.";
            }
            
            // Handle AutoDetect - check what it will become
            if (currentAutoHoldValue == (int)VRC_Pickup.AutoHoldMode.AutoDetect)
            {
                if (willBeAutoHold)
                {
                    return "This pickup uses older auto-hold settings that don't work for many controllers.\n" +
                           "Upgrade to Version 1.1 for better compatibility and reduced hand fatigue.";
                }
            }
            
            // Handle No and AutoDetect that becomes No - won't change function but will simplify UI
            if (!willBeAutoHold)
            {
                return "This pickup uses older auto-hold settings.\nUpgrading won't change the way it functions,\n" +
                       "but it will simplify the options below.";
            }
            
            // Default fallback
            return "Upgrade to Version 1.1 for improved compatibility and simplified settings.";
        }
        
        // Called after version change to notify editor for UI updates
        public override void OnVersionChanged(VRCPickup.Version version, SerializedObject serializedObject)
        {
            onVersionChangedCallback?.Invoke(version);
        }
        
    }
}