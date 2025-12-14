using System;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace VRC.SDKBase.Editor.Versioning
{
    /// <summary>
    /// Abstract base class for component version migration logic.
    /// Provides sensible defaults for common scenarios while allowing full customization.
    /// </summary>
    public abstract class ComponentVersionMigrator<TVersion> where TVersion : Enum
    {
        /// <summary>
        /// Core migration logic - must be implemented by each component
        /// </summary>
        /// <param name="version">Target version to migrate to</param>
        /// <param name="serializedObject">Component's serialized object</param>
        public abstract void MigrateToVersion(TVersion oldVersion, TVersion newVersion, SerializedObject serializedObject);
        
        /// <summary>
        /// Gets a message describing what changes were made during an upgrade
        /// Must be implemented to provide component-specific upgrade details
        /// </summary>
        /// <param name="oldVersion">Version before upgrade</param>
        /// <param name="newVersion">Version after upgrade</param>
        /// <param name="serializedObject">Component's serialized object</param>
        /// <returns>Message describing changes made, or null if no message needed</returns>
        public abstract string GetUpgradeChangesMessage(TVersion oldVersion, TVersion newVersion, SerializedObject serializedObject);
        
        /// <summary>
        /// Gets the latest version available for this component.
        /// Default implementation automatically detects the highest enum value using VersionHelper.
        /// Override only if you need custom latest version logic.
        /// </summary>
        /// <returns>Latest version enum value</returns>
        public virtual TVersion GetLatestVersion()
        {
            return VersionHelper.GetLatestVersion<TVersion>();
        }
        
        /// <summary>
        /// Gets a warning message for version downgrades
        /// Override to provide component-specific downgrade warnings
        /// </summary>
        /// <param name="oldVersion">Current version</param>
        /// <param name="newVersion">Target downgrade version</param>
        /// <returns>Warning message, or null if no warning needed</returns>
        public virtual string GetDowngradeWarning(TVersion oldVersion, TVersion newVersion)
        {
            return "Changing to an older version may reintroduce issues that were fixed in newer versions.\n" +
                   "Consider keeping the current version for best compatibility.";
        }
        
        /// <summary>
        /// Gets the text to display in the upgrade prompt info box
        /// Override to provide component-specific upgrade messaging
        /// </summary>
        /// <param name="currentVersion">Current version</param>
        /// <param name="latestVersion">Latest available version</param>
        /// <returns>Text for the upgrade prompt</returns>
        public virtual string GetUpgradePromptText(TVersion currentVersion, TVersion latestVersion)
        {
            return $"This component uses an older version. Upgrade to {GetVersionDisplayName(latestVersion)} for improved compatibility.";
        }
        
        /// <summary>
        /// Determines if an upgrade prompt should be shown for the current state
        /// Default implementation shows prompt if not at latest version
        /// Override for component-specific upgrade prompt logic
        /// </summary>
        /// <param name="version">Current version</param>
        /// <param name="serializedObject">Component's serialized object</param>
        /// <returns>True if upgrade prompt should be shown</returns>
        public virtual bool ShouldShowUpgradePrompt(TVersion version, SerializedObject serializedObject)
        {
            return Convert.ToInt32(version) < Convert.ToInt32(GetLatestVersion());
        }
        
        /// <summary>
        /// Called after a version change is applied to refresh version-dependent UI
        /// </summary>
        /// <param name="version">New version</param>
        /// <param name="serializedObject">Component's serialized object</param>
        public virtual void OnVersionChanged(TVersion version, SerializedObject serializedObject)
        {
            // Override in derived classes if needed
        }
        
        /// <summary>
        /// Gets a display name for a version, using InspectorName attribute if available
        /// </summary>
        /// <param name="version">Version to get display name for</param>
        /// <returns>Display name for the version</returns>
        public string GetVersionDisplayName(TVersion version)
        {
            var enumType = typeof(TVersion);
            var enumName = Enum.GetName(enumType, version);
            var field = enumType.GetField(enumName);
            
            if (field != null)
            {
                var inspectorNameAttr = field.GetCustomAttributes(typeof(InspectorNameAttribute), false);
                if (inspectorNameAttr.Length > 0)
                {
                    return ((InspectorNameAttribute)inspectorNameAttr[0]).displayName;
                }
            }
            
            return enumName;
        }
    }
}