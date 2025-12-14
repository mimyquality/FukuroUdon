using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRC.SDKBase.Editor.Versioning
{
    /// <summary>
    /// Reusable UI system for component version management
    /// </summary>
    public class ComponentVersionUI<TVersion> where TVersion : Enum
    {
        private readonly SerializedProperty versionProperty;
        private readonly ComponentVersionMigrator<TVersion> migrator;
        private readonly string documentationUrl;
        private readonly TVersion latestVersion;
        
        private EnumField versionField;
        private VisualElement versionUpgradeInfo;
        
        public ComponentVersionUI(SerializedProperty versionProperty, ComponentVersionMigrator<TVersion> migrator, TVersion latestVersion, string documentationUrl = null)
        {
            this.versionProperty = versionProperty;
            this.migrator = migrator;
            this.latestVersion = latestVersion;
            this.documentationUrl = documentationUrl;
        }
        
        /// <summary>
        /// Creates the version dropdown field with help button
        /// </summary>
        public void CreateVersionField(VisualElement root)
        {
            var versionContainer = new VisualElement();
            versionContainer.style.flexDirection = FlexDirection.Row;
            versionContainer.style.marginBottom = 10;
            
            // Use the version value from the component
            TVersion defaultVersion = (TVersion)Enum.ToObject(typeof(TVersion), versionProperty.enumValueIndex);
            
            // Create version enum field
            versionField = new EnumField("Version", defaultVersion);
            versionField.style.flexGrow = 1;
            versionField.RegisterValueChangedCallback(OnVersionChanged);
            
            versionContainer.Add(versionField);
            
            // Create help button if documentation URL provided
            if (!string.IsNullOrEmpty(documentationUrl))
            {
                var helpButton = new Button(() => {
                    Application.OpenURL(documentationUrl);
                })
                {
                    text = "?",
                    style = { width = 30 }
                };
                versionContainer.Add(helpButton);
            }
            
            root.Insert(0, versionContainer);
        }
        
        /// <summary>
        /// Creates the upgrade info box (initially hidden)
        /// </summary>
        public void CreateUpgradeInfo(VisualElement root)
        {
            if (versionUpgradeInfo != null) return;
            
            versionUpgradeInfo = new VisualElement()
            {
                name = "versionUpgradeInfo"
            };
            versionUpgradeInfo.style.marginBottom = 10;
            versionUpgradeInfo.style.marginTop = 5;
            versionUpgradeInfo.style.display = DisplayStyle.None;
            
            var currentVersion = (TVersion)Enum.ToObject(typeof(TVersion), versionProperty.enumValueIndex);
            var helpBox = new HelpBox(
                migrator.GetUpgradePromptText(currentVersion, latestVersion),
                HelpBoxMessageType.Info
            );
            versionUpgradeInfo.Add(helpBox);
            
            var upgradeButton = new Button(() => 
            {
                // Trigger version change via the dropdown (this will handle the popup)
                if (versionField != null)
                {
                    versionField.value = latestVersion;
                }
            })
            {
                text = $"Upgrade to {migrator.GetVersionDisplayName(latestVersion)}",
                style = { marginTop = 5 }
            };
            
            versionUpgradeInfo.Add(upgradeButton);
            
            // Insert at the top after the version field
            root.Insert(1, versionUpgradeInfo);
        }
        
        /// <summary>
        /// Updates the visibility and content of the upgrade info box
        /// </summary>
        public void RefreshUpgradeInfo()
        {
            if (versionUpgradeInfo == null) return;
            
            var currentVersion = (TVersion)Enum.ToObject(typeof(TVersion), versionProperty.enumValueIndex);
            bool shouldShow = migrator.ShouldShowUpgradePrompt(currentVersion, versionProperty.serializedObject);
            
            versionUpgradeInfo.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            
            // Always update the help box text when the upgrade info exists
            var helpBox = versionUpgradeInfo.Query<HelpBox>().First();
            if (helpBox != null)
            {
                helpBox.text = migrator.GetUpgradePromptText(currentVersion, latestVersion);
            }
        }
        
        private void OnVersionChanged(ChangeEvent<Enum> evt)
        {
            var newVersion = (TVersion)evt.newValue;
            var oldVersion = (TVersion)evt.previousValue;

            if (!newVersion.Equals(oldVersion))
            {
                // Get appropriate message (warning for downgrade, changes for upgrade)
                string message = GetVersionChangeMessage(oldVersion, newVersion);
                if (!string.IsNullOrEmpty(message))
                {
                    bool isDowngrade = Convert.ToInt32(newVersion) < Convert.ToInt32(oldVersion);
                    string title = isDowngrade ? "Version Downgrade Warning" : "Upgrade Complete";
                    string okButton = isDowngrade ? "Continue" : "OK";
                    string cancelButton = isDowngrade ? "Cancel" : null;
                    
                    bool proceed = cancelButton != null ? 
                        EditorUtility.DisplayDialog(title, message, okButton, cancelButton) :
                        EditorUtility.DisplayDialog(title, message, okButton);
                        
                    if (!proceed && cancelButton != null)
                    {
                        // User cancelled downgrade, revert the change
                        versionField.SetValueWithoutNotify(oldVersion);
                        return;
                    }
                }

                // Update version in the component and apply changes
                versionProperty.serializedObject.Update();
                versionProperty.enumValueIndex = Convert.ToInt32(newVersion);
                versionProperty.serializedObject.ApplyModifiedProperties();

                // Apply migration and refresh UI
                migrator.MigrateToVersion(oldVersion, newVersion, versionProperty.serializedObject);
                migrator.OnVersionChanged(newVersion, versionProperty.serializedObject);
                RefreshUpgradeInfo();
            }
        }
        
        private string GetVersionChangeMessage(TVersion oldVersion, TVersion newVersion)
        {
            bool isUpgrade = Convert.ToInt32(newVersion) > Convert.ToInt32(oldVersion);
            bool isDowngrade = Convert.ToInt32(newVersion) < Convert.ToInt32(oldVersion);
            
            if (isDowngrade)
            {
                return migrator.GetDowngradeWarning(oldVersion, newVersion);
            }
            else if (isUpgrade)
            {
                return migrator.GetUpgradeChangesMessage(oldVersion, newVersion, versionProperty.serializedObject);
            }
            
            return null; // No message for same version
        }
        
    }
}